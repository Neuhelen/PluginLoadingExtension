using Plugin.Interfaces;
using Plugin.PluginHost;
using SharedContracts.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Plugin.Test
{
    [TestClass]
    public class PluginManagerTest
    {
        private IPluginManager pluginManager;

        // This is the interface implemented by the test plugin. 
        private Type pluginBaseType = typeof(IPlugin);

        private string fileName = @"C:\Users\patri\source\repos\PluginLoadingExtension\Plugin.Test\bin\Debug\PluginFolder\IPlugin\PluginTestDLL2.dll";

        // These are the test plugin's name and used function. 
        // Update the extension of the plugin file names if they are updated in the "LicensePluginLoader". 
        private string[] pluginIdentifiers = { "30B1E09E157C473B79AAE07B2B1C21FE59B5B0D52A8668881278C1C613738157",
            "B799D1847A865C3319F8DE2EF3CA3D40D38E9FAF4705E844F94F91422BE42773" };
        private string pluginMethodParameter = "Random Name";

        [TestInitialize]
        public void Setup()
        {
            pluginManager = new PluginManager();
        }

        [TestMethod]
        public void CreateAllPluginsTest()
        {
            // Disregard the DataLoader Exception.
            // It is not a part of the automatic test, but a part of the manual test, which the automatic test cannot understand. 
            List<object> loadResult = pluginManager.CreatePlugins(pluginBaseType, null, null);
            string[] expectedResults = { "Hello! PluginTestDLL.Plugin2 has been loaded. The dependencies will now be loaded. " + 
                "PluginDependency.DependencyB has been loaded. The Name is: Name2.",
                "Hello, Random Name! The dependency, InternalDependencyPlugin, is called.PluginDependency.DependencyB has been loaded. " +
                "The Name is: Name." };

            Assert.AreEqual(expectedResults.Length, loadResult.Count);

            for (int i = 0; i < loadResult.Count; i++)
            {
                var pluginObject = loadResult[i];

                Assert.IsNotNull(pluginObject);

                Assert.IsTrue(pluginBaseType.IsAssignableFrom(pluginObject.GetType()),
                    $"Loaded object {pluginObject.GetType().Name} does not implement {pluginBaseType.Name}.");

                var plugin = (IPlugin)pluginObject;
                var result = plugin.HelloName(pluginMethodParameter);

                Assert.AreEqual(expectedResults[i], result,
                    $"Plugin {pluginObject.GetType().Name} returned an unexpected result.");
            }
        }

        [TestMethod]
        public void CreatePluginTest()
        {
            object pluginObject = pluginManager.CreatePlugins(pluginBaseType, pluginIdentifiers[0], null)[0];
            string expectedResult = "Hello! PluginTestDLL.Plugin2 has been loaded. The dependencies will now be loaded. " +
                "PluginDependency.DependencyB has been loaded. The Name is: Name2.";

            Assert.IsNotNull(pluginObject);
            Assert.IsTrue(pluginBaseType.IsAssignableFrom(pluginObject.GetType()),
                    $"Loaded object {pluginObject.GetType().Name} does not implement {pluginBaseType.Name}.");

            var plugin = (IPlugin)pluginObject;
            var result = plugin.HelloName(pluginMethodParameter);
            Assert.AreEqual(expectedResult, result,
                $"Plugin {pluginObject.GetType().Name} returned an unexpected result.");
        }

        [TestMethod]
        public void InvokeMethodTest()
        {
            IPlugin plugin = new MockPlugin();

            System.Diagnostics.Debug.WriteLine("This test should write 'Failed to invoke method 'MockNewFunction': Parameter count mismatch.' to the console.");
            var result = pluginManager.InvokeMethod(plugin, "MockNewFunction", null);
            Assert.IsNull(result);

            string expectedResult = "0";
            result = pluginManager.InvokeMethod(plugin, "MockNewFunction", expectedResult);
            Assert.IsNotNull(result);
            Assert.IsTrue(typeof(string).IsAssignableFrom(result.GetType()));
            Assert.AreEqual(expectedResult, (string)result);
        }

        [TestMethod]
        public void GetPluginFromFileNameTest()
        {
            object expectedObject = pluginManager.CreatePlugins(pluginBaseType, pluginIdentifiers[0], null)[0];

            object actualObject = pluginManager.GetPluginFromFileName(pluginBaseType, fileName);
            
            Assert.IsNotNull(actualObject);

            Assert.AreEqual(expectedObject, actualObject);
        }

        [TestMethod]
        public void GetPluginFromPluginIdentifierTest()
        {
            object expectedObject = pluginManager.CreatePlugins(pluginBaseType, pluginIdentifiers[0], null)[0];

            object actualObject = pluginManager.GetPluginFromPluginIdentifier(pluginBaseType, pluginIdentifiers[0]);

            Assert.IsNotNull(actualObject);

            Assert.AreEqual(expectedObject, actualObject);
        }
    }
}
