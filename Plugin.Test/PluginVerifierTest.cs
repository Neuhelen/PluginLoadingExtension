using Plugin.Interfaces;
using Plugin.PluginHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Plugin.Test
{

    [TestClass]
    public class PluginVerifierTest
    {
        private IPluginVerifier verifier;

        // This is the test plugin's name and used function
        private string[] pluginFileNames = { "PluginTestDLL.zip", "PluginTestDLL2.dll" };
        private string pluginBaseType = "IPlugin"; 
        private string pluginFilePath;
        private string signatureFolderPath; 
        private string signatureFilePath;
        private string pinnedThumbprint = "6A03C66626CEADA31B698463CC6E804CBF66782FA27EBC9C3AD86A7F2A18E9DC";
        private string baseDir; 

        [TestInitialize]
        public void Setup()
        {
            verifier = new PluginVerifier();

            baseDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        [TestMethod]
        public void VerifyDigitalSignatureSuccessTest()
        {
            // This sets up the file paths. 
            pluginFilePath = Path.Combine(baseDir, "PluginFolder", pluginBaseType, pluginFileNames[0]);
            signatureFolderPath = Path.Combine(baseDir, "SignatureFolder");

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pluginFilePath);

            signatureFilePath = Path.Combine(signatureFolderPath, pluginBaseType, fileNameWithoutExtension + ".p7");

            // This tests the plugin verification. 
            bool verificationResult = verifier.VerifyDigitalSignature(signatureFilePath, pluginFilePath, pinnedThumbprint);
            Assert.IsTrue(verificationResult, "Plugin could not be verified.");
        }

        [TestMethod]
        public void VerifyÍnvalidDigitalSignatureFailingTest()
        {
            // This sets up the file paths. The plugin file path is correct, but the signature file path is for another plugin. 
            pluginFilePath = Path.Combine(baseDir, "PluginFolder", pluginBaseType, pluginFileNames[1]);
            signatureFolderPath = Path.Combine(baseDir, "SignatureFolder");

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pluginFileNames[0]);

            signatureFilePath = Path.Combine(signatureFolderPath, pluginBaseType, fileNameWithoutExtension + ".p7");

            // This tests the plugin verification. 
            bool verificationResult = verifier.VerifyDigitalSignature(signatureFilePath, pluginFilePath, pinnedThumbprint);
            Assert.IsFalse(verificationResult, "Plugin was verified when it shouldn't be.");
        }
    }
}
