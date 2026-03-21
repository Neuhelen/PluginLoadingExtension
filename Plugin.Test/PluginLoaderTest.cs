using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plugin.Interfaces;
using System;
using System.IO;
using Plugin.PluginHost;
using SharedContracts.Interfaces;

namespace Plugin.Test
{
    [TestClass]
    public class PluginLoaderTest
    {
        private IPluginLoader loader;
        private string pluginFolderPath;
        
        // This is the interface implemented by the test plugin
        private Type pluginBaseType = typeof(IPlugin); 

        // This is the test plugin's name and used function
        private string[] pluginFileNames = { "PluginTestDLL2.dll", "PluginTestDLL.zip" }; 
        private string[] pluginIdentifiers = { "30B1E09E157C473B79AAE07B2B1C21FE59B5B0D52A8668881278C1C613738157",
            "B799D1847A865C3319F8DE2EF3CA3D40D38E9FAF4705E844F94F91422BE42773" }; 
        private string pinnedThumbprint = "6A03C66626CEADA31B698463CC6E804CBF66782FA27EBC9C3AD86A7F2A18E9DC";
        string pluginFilePath; 

        [TestInitialize]
        public void Setup()
        {
            loader = new PluginLoader();

            // The pluginFolder is "PluginFolder" + the pluginBaseType.Name variable
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            pluginFolderPath = Path.Combine(baseDir, "PluginFolder", pluginBaseType.Name);
        }

        [TestMethod]
        public void LoadPluginTest()
        {
            // This tests the plugin loading 

            bool loadResult;
            for (int i = 0; i<pluginFileNames.Length; i++) 
            {
                loadResult = false;
                pluginFilePath = Path.Combine(pluginFolderPath, pluginFileNames[i]);

                if (pluginFilePath.EndsWith(".dll"))
                {
                    loadResult = loader.LoadPluginFromDLL(pluginBaseType, pluginFilePath, pluginIdentifiers[i],
                    pinnedThumbprint);
                }
                else if (pluginFilePath.EndsWith(".zip"))
                {
                    loadResult = loader.LoadPluginFromZip(pluginBaseType, pluginFilePath, pluginIdentifiers[i],
                    pinnedThumbprint);
                }

                Assert.IsTrue(loadResult, "Plugin failed to load.");
            }
        }

        [TestMethod]
        public void InstantiatePluginTest()
        {
            bool loadResult;
            for (int i = 0; i < pluginFileNames.Length; i++)
            {
                loadResult = false;
                pluginFilePath = Path.Combine(pluginFolderPath, pluginFileNames[i]);

                if (pluginFilePath.EndsWith(".dll"))
                {
                    loadResult = loader.LoadPluginFromDLL(pluginBaseType, pluginFilePath, pluginIdentifiers[i],
                    pinnedThumbprint);
                }
                else if (pluginFilePath.EndsWith(".zip"))
                {
                    loadResult = loader.LoadPluginFromZip(pluginBaseType, pluginFilePath, pluginIdentifiers[i],
                    pinnedThumbprint);
                }

                Assert.IsTrue(loadResult, "Plugin failed to load.");


                var pluginInstance = loader.InstantiatePlugin(pluginBaseType, pluginIdentifiers[i], null);
                Assert.IsNotNull(pluginInstance, "Failed to instantiate plugin.");
            }
        }
    }
}
