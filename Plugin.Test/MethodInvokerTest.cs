using Plugin.Interfaces;
using Plugin.PluginHost;
using SharedContracts.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Plugin.Test
{
    [TestClass]
    public class MethodInvokerTest
    {
        private IMethodInvoker methodInvoker;
        private IPlugin plugin; 

        [TestInitialize]
        public void Setup()
        {
            methodInvoker = new MethodInvoker();
            plugin = new MockPlugin();
        }

        [TestMethod]
        public void InvokeMethodTest()
        {
            System.Diagnostics.Debug.WriteLine("This test should write 'Failed to invoke method 'MockNewFunction': Parameter count mismatch.' to the console."); 
            var result = methodInvoker.InvokeMethod(plugin, "MockNewFunction", null);
            Assert.IsNull(result);

            string expectedResult = "0"; 
            result = methodInvoker.InvokeMethod(plugin, "MockNewFunction", expectedResult);
            Assert.IsNotNull(result);
            Assert.IsTrue(typeof(string).IsAssignableFrom(result.GetType()));
            Assert.AreEqual(expectedResult, (string)result);
        }
    }
}
