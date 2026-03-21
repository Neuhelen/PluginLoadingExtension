using Plugin.Interfaces;
using Plugin.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Plugin.PluginHost
{
    public class PluginLoader : IPluginLoader
    {
        private readonly string baseDirectory;
        private string dependencyFolder;
        private string signatureFolder;

        private static readonly Dictionary<Type, Dictionary<string, Type>> pluginTypes = new Dictionary<Type, Dictionary<string, Type>>();
        private static readonly Dictionary<Type, Dictionary<string, object>> plugins = new Dictionary<Type, Dictionary<string, object>>();
        private static readonly ConcurrentDictionary<string, Assembly> loadedAssemblies = new ConcurrentDictionary<string, Assembly>
            (StringComparer.OrdinalIgnoreCase);

        private static readonly object handlerLock = new object();
        private static bool assemblyResolveAttached = false;

        private readonly IPluginVerifier pluginVerifier;
        private static readonly ILogger logger = new Logger();

        public object GetPlugin(Type pluginType, string pluginIdentifier)
        {
            if (!plugins.TryGetValue(pluginType, out var innerObjectDictionary))
            {
                innerObjectDictionary = new Dictionary<string, object>();
                plugins[pluginType] = innerObjectDictionary;
            }

            if (!innerObjectDictionary.TryGetValue(pluginIdentifier, out var plugin))
            {
                logger.Error($"The '{pluginType}' Type plugin with the plugin identifier '{pluginIdentifier}' has not been instantiated " +
                    $"and could not be returned."); 
                return null;
            }

            logger.Info($"The '{pluginType}' Type plugin with the plugin identifier '{pluginIdentifier}' has been returned.");

            return plugin;
        }

        public PluginLoader()
        {
            pluginVerifier = new PluginVerifier();
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            foreach (Assembly assemblies in AppDomain.CurrentDomain.GetAssemblies())
            {
                loadedAssemblies.TryAdd(assemblies.FullName, assemblies);
            }

            lock (handlerLock)
            {
                if (!assemblyResolveAttached)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
                    assemblyResolveAttached = true;
                }
            }
        }

        public bool LoadPluginFromDLL(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint)
        {
            // If the key already exists, then the plugin is not added to the Dictionary. 
            if (!pluginTypes.TryGetValue(pluginType, out var innerDictionary))
            {
                innerDictionary = new Dictionary<string, Type>();
                pluginTypes[pluginType] = innerDictionary;
            }

            if(innerDictionary.ContainsKey(pluginIdentifier))
                return true;

            // If another method is used to specify the subfolders of the DependencyFolder and SignatureFolder,
            // then it may be a good idea to use the MakeValidFolderName function. 
            dependencyFolder = Path.Combine(baseDirectory, "DependencyFolder", pluginType.Name, pluginIdentifier);
            signatureFolder = Path.Combine(baseDirectory, "SignatureFolder", pluginType.Name);

            logger.Debug($"The designated dependency folder is: {dependencyFolder}");
            logger.Debug($"The designated signature folder is: {signatureFolder}");

            if (!IsFileValid(pluginType, fileName, pluginIdentifier, pinnedThumbprint))
                return false;

            signatureFolder = Path.Combine(baseDirectory, "SignatureFolder", pluginType.Name, pluginIdentifier);

            try
            {
                Assembly assembly = LoadAssembly(pluginType, fileName, pluginIdentifier, pinnedThumbprint);

                if (assembly == null || !AddPlugin(pluginType, pluginIdentifier, assembly, innerDictionary))
                {
                    logger.Error("The assembly is null or could not be added to the dictionary.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // If an Exception occurs, log the offending Assembly, and return false. 
                logger.Error($"Failed to load assembly {fileName}: {ex.Message}");
                return false;
            }
        }

        private bool IsFileValid(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint)
        {
            if (!File.Exists(fileName))
            {
                logger.Error($"The given file did not exist. The file was: {fileName}");
                return false;
            }

            if (!VerifyHash(fileName, pluginIdentifier) ||
                !VerifyDigitalSignature(pluginType, fileName, pinnedThumbprint))
                return false;

            logger.Info($"The given file '{fileName}' was valid.");

            return true;
        }

        private bool VerifyHash(string fileName, string pluginIdentifier)
        {
            // This part computes the SHA256 of the file. 
            string actualHash = null;

            using (var fileStream = File.OpenRead(fileName))
            using (var sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(fileStream);
                actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }

            // This part compares the actual hash value with the expected, whitelisted value. 
            if (!actualHash.Equals(pluginIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                logger.Error($"The given file's hash value did not match the " +
                        $"whitelisted hash value. The file was: {fileName}");
                return false;
            }

            return true;
        }

        private bool VerifyDigitalSignature(Type pluginType, string fileName, string pinnedThumbprint)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string[] fileExtensions = { ".p7", ".p7s" };

            var signaturePath =
                fileExtensions
                .Select(fileExtension => Path.Combine(signatureFolder, fileNameWithoutExtension + fileExtension))
                .FirstOrDefault(File.Exists);

            // Verifies the file using the found digital signature. 
            if (signaturePath == null || !pluginVerifier.VerifyDigitalSignature(signaturePath, fileName, pinnedThumbprint))
            {
                logger.Error("The digital signature could not be verified.");
                return false;
            }

            return true;
        }

        private bool AddPlugin(Type pluginType, string pluginIdentifier, Assembly assembly, Dictionary<string, Type> dictionary)
        {
            var implementedType = assembly.GetTypes().FirstOrDefault(type =>
                pluginType.IsAssignableFrom(type) &&
                type.IsClass &&
                !type.IsAbstract &&
                type != pluginType);

            if (implementedType == null)
            {
                logger.Error($"The given base Type, {pluginType}, was not implemented in the plugin, {pluginIdentifier}.");
                return false;
            }

            dictionary.Add(pluginIdentifier, implementedType);

            logger.Info($"The plugin '{implementedType.FullName}' from the assembly '{assembly.FullName}' was added to the dictionary.");

            return true;
        }

        private Assembly LoadAssembly(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint)
        {
            Assembly assembly = Assembly.LoadFile(fileName);

            logger.Info($"Loaded the assembly '{assembly.FullName}' from the file '{fileName}");

            if (!loadedAssemblies.ContainsKey(assembly.FullName) && !loadedAssemblies.TryAdd(assembly.FullName, assembly))
            {
                logger.Error($"The assembly {assembly.FullName} of Type {pluginType} was not contained by the " +
                    $"Dictionary loadedAssemblies, but could not be added to it.");

                return null;
            }

            if (!LoadDependencies(pluginType, assembly, pinnedThumbprint))
            {
                logger.Error($"The dependencies of the  assembly, {assembly.FullName}, could not be loaded.");
                return null;
            }

            return assembly;
        }


        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).FullName;

            if (loadedAssemblies.TryGetValue(name, out Assembly assembly))
            {
                // Returns the previously loaded assembly of the same FullName. 
                return assembly;
            }

            logger.Error($"Could not resolve the assembly: {name}.");

            return null;
        }

        private bool LoadDependencies(Type pluginType, Assembly assembly, string pinnedThumbprint)
        {
            try
            {
                AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
                string assemblyFullName;

                foreach (AssemblyName referencedAssembly in referencedAssemblies)
                {
                    assemblyFullName = referencedAssembly.FullName;

                    if (loadedAssemblies.ContainsKey(assemblyFullName))
                        continue;

                    foreach (var file in Directory.EnumerateFiles(dependencyFolder, "*.dll", SearchOption.AllDirectories))
                    {
                        string actualFullName = AssemblyName.GetAssemblyName(file).FullName;
                        if (actualFullName.Equals(referencedAssembly.FullName))
                        {
                            logger.Debug($"A file with the correct FullName has been found in the " +
                                $"designated dependency folder, {dependencyFolder}. The file was: {file}");

                            if (!VerifyDigitalSignature(pluginType, file, pinnedThumbprint))
                                return false;

                            if (LoadAssembly(pluginType, file, assemblyFullName, pinnedThumbprint) == null)
                                return false;

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"The dependencies of the {assembly.FullName} could not be resolved, due to the Exception: {ex.Message}.");
            }

            return true;
        }

        public object InstantiatePlugin(Type pluginType, string pluginIdentifier, params object[] args)
        {
            // If the key already exists, then the plugin is not added to the Dictionary. 
            if (!plugins.TryGetValue(pluginType, out var innerObjectDictionary))
            {
                innerObjectDictionary = new Dictionary<string, object>();
                plugins[pluginType] = innerObjectDictionary;
            }

            if (innerObjectDictionary.ContainsKey(pluginIdentifier))
            {
                var innerObject = innerObjectDictionary[pluginIdentifier];
                logger.Info($"An object {pluginIdentifier} of type {pluginType} known as {innerObject} exists and has been returned.");
                return innerObject;
            }

            // If a plugin Type does not exist in the Dictionary, there is nothing to make an plugin object of. 

            if (!pluginTypes.TryGetValue(pluginType, out var innerTypeDictionary))
            {
                logger.Error($"An object {pluginIdentifier} of type {pluginType} has not been loaded.");
                return null;
            }

            // Instantiates a plugin object. 

            logger.Debug($"Trying to instantiate {pluginIdentifier} of type { pluginType }.");

            Type implementedType = innerTypeDictionary[pluginIdentifier];

            object pluginInstance = Activator.CreateInstance(implementedType, args);

            innerObjectDictionary.Add(pluginIdentifier, pluginInstance);

            logger.Info($"{pluginIdentifier} of type {pluginType} has been instantiated.");

            return pluginInstance;
        }

        private string MakeValidFolderName(string name)
        {
            // This gets all the invalid characters for Windows file and folder names. 
            var invalidChars = Path.GetInvalidFileNameChars();

            // This replaces the characters underscores
            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }

            // This trims any additional spaces and dots at the end of the string. 
            return name.Trim().TrimEnd('.');
        }

        public bool LoadPluginFromZip(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint)
        {
            // If the key already exists, then the plugin is not added to the Dictionary.
            if (!pluginTypes.TryGetValue(pluginType, out var innerTypeDictionary))
            {
                innerTypeDictionary = new Dictionary<string, Type>();
                pluginTypes[pluginType] = innerTypeDictionary;
            }

            if (innerTypeDictionary.ContainsKey(fileName))
                return true;

            // Used to inspect the assembly's FullName before it is fully loaded. 
            AppDomain temporaryAppDomain = null; 

            try
            {
                signatureFolder = Path.Combine(baseDirectory, "SignatureFolder", pluginType.Name);

                // Verfies the zip file before opening it. 
                if (!IsFileValid(pluginType, fileName, pluginIdentifier, pinnedThumbprint))
                    return false;

                // Used to ensure that only one plugin is added from a single zip file. 
                bool pluginAdded = false;

                temporaryAppDomain = CreateTemporaryAppDomain();

                logger.Debug("The temporary AppDomain has been instantiated.");

                using (var fileStream = File.OpenRead(fileName))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))

                    foreach (var entry in archive.Entries.Where(e =>
                                 e.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                    {
                        byte[] assemblyBytes;

                        // Extracts the DLL file into memory.
                        using (var dllStream = entry.Open())
                        using (var dllMemory = new MemoryStream())
                        {
                            dllStream.CopyTo(dllMemory);
                            assemblyBytes = dllMemory.ToArray();
                        }

                        if (!DoesZipAssemblyExist(temporaryAppDomain, assemblyBytes))
                            continue; 

                        // Loads the assembly from memory.if it has not been loaded already.  
                        var assembly = Assembly.Load(assemblyBytes);

                        logger.Debug($"The assembly '{assembly.FullName}' has been loaded from the zip file '{fileName}'.");

                        // Adds the assembly to the appropriate Dictionaries. 
                        loadedAssemblies.TryAdd(assembly.FullName, assembly);

                        if (pluginAdded)
                            continue;

                        if (AddPlugin(pluginType, pluginIdentifier, assembly, innerTypeDictionary))
                            pluginAdded = true;
                    }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load assembly {fileName}: {ex.Message}");
                return false;
            }
            finally
            {
                if (temporaryAppDomain != null)
                    AppDomain.Unload(temporaryAppDomain);
                logger.Debug("The temporary AppDomain has been unloaded."); 
            }
        }
        private bool DoesZipAssemblyExist(AppDomain appDomain, byte[] assemblyBytes)
        {
            // Inspects the FullName of the Assembly through Reflection and a temporary AppDomain before unloading it. 

            IAssemblyInspector inspector = (AssemblyInspector)appDomain.CreateInstanceAndUnwrap(
                typeof(AssemblyInspector).Assembly.FullName,
                typeof(AssemblyInspector).FullName
            );

            // Checks whether the loadedAssemblies Dictionary contains the FullName
            // before returning inverted result to indicate that the Assembly does not exist. 

            string fullName = inspector.GetFullName(assemblyBytes);

            bool result = !loadedAssemblies.ContainsKey(fullName);

            return result;
        }

        private AppDomain CreateTemporaryAppDomain()
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            return AppDomain.CreateDomain("TemporaryReflectionDomain", null, setup);
        }
    }
}
