using Model.Licenses;
using Plugin.Interfaces;
using Plugin.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.PluginHost
{
    public class PluginManager : IPluginManager
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IMethodInvoker methodInvoker;
        private readonly List<LicensePluginLoader> whitelistedPlugins;

        private static readonly ILogger logger = new Logger(); 

        private readonly License license; 


        public PluginManager()
        {
            pluginLoader = new PluginLoader();
            methodInvoker = new MethodInvoker();
            // This pluginRegistry and its constructor should use the actual, loaded license through a "License" parameter, 
            // along with state.CurrentLicense.License.
            // But the DataLoaderCollection is loaded before the license is loaded, so state.CurrentLicense is null. 
            // The correct version would this: 
            // this.license = license;

            // This can be used because the information is hardcoded in the LicensePluginLoader, but it will need to be fixed
            // in order to provide a replaceable whitelist:
            license = new License();
            whitelistedPlugins = license.Plugins;
        }

        public List<object> CreatePlugins(Type pluginType, string pluginId, params object[] parameters)
        {
            List<object> plugins = new List<object>();

            foreach (var whitelistedPlugin in whitelistedPlugins)
            {
                if (pluginId != null && !pluginId.Equals(whitelistedPlugin.Identifier))
                    continue;

                if (!whitelistedPlugin.FileName.Contains("PluginFolder\\" + pluginType.Name + "\\"))
                        continue; 

                if (!LoadDLLOrZip(pluginType, whitelistedPlugin.FileName, whitelistedPlugin.Identifier,
                    whitelistedPlugin.Thumbprint))
                    continue;

                object plugin = pluginLoader.InstantiatePlugin(pluginType, whitelistedPlugin.Identifier, parameters);

                if (plugin != null)
                    plugins.Add(plugin);
            }

            return plugins;
        }

        public object InvokeMethod(object pluginInstance, string methodName, params object[] args)
        {
            return methodInvoker.InvokeMethod(pluginInstance, methodName, args);
        }

        private bool LoadDLLOrZip(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint)
        { 
            if (fileName.EndsWith(".dll")) 
            {
                if (pluginLoader.LoadPluginFromDLL(pluginType, fileName, pluginIdentifier, pinnedThumbprint))
                {
                    return true;
                }
            }
            else if (fileName.EndsWith(".zip"))
            {
                if (pluginLoader.LoadPluginFromZip(pluginType, fileName, pluginIdentifier, pinnedThumbprint))
                {
                    return true;
                }
            }

            logger.Error($"An error occurred during the loading of the plugin '{fileName}' of Type {pluginType}.");

            return false; 
        }

        public object GetPluginFromFileName(Type pluginType, string fileName)
        {
            foreach (var whitelistedPlugin in whitelistedPlugins)
            {
                if (fileName.Equals(whitelistedPlugin.FileName))
                    return pluginLoader.GetPlugin(pluginType, whitelistedPlugin.Identifier);
            }

            return null;
        }

        public object GetPluginFromPluginIdentifier(Type pluginType, string pluginIdentifier)
        {
            return pluginLoader.GetPlugin(pluginType, pluginIdentifier);
        }
    }
}
