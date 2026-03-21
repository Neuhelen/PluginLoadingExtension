using Plugin.Interfaces;
using Plugin.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace Plugin.PluginHost
{
    public class PluginVerifier: IPluginVerifier
    {
        private static readonly ILogger logger = new Logger();
        public bool VerifyDigitalSignature(string signaturePath, string filePath, string pinnedThumbprint)
        {
            // Loads the matching signature and file as bytes without executing them if they are not null. 
            if (!File.Exists(signaturePath) || !File.Exists(filePath)) 
            {
                logger.Error($"One or both of the file paths for the digital signature and the signed file were invalid. " +
                    $"The file paths were '{signaturePath}' and '{filePath}'");

                return false;
            }
            byte[] signatureData = File.ReadAllBytes(signaturePath);
            byte[] assemblyData = File.ReadAllBytes(filePath);

            // Loads the digital signature as a "SignedCms" object. 
            ContentInfo contentInfo = new ContentInfo(assemblyData);
            SignedCms signedCms = new SignedCms(contentInfo, detached: true);

            try
            {

                signedCms.Decode(signatureData);
            }
            catch (Exception ex)
            {
                logger.Error($"The CMS object '{signaturePath}' was invalid. " +
                    $"The returned Exception from the verification function was {ex}");
                return false;
            }

            // Attempts to verify signature and certificate chain, as indicated by the "false" value.
            // If the function fails, an Exception is thrown. 
            try
            {

                signedCms.CheckSignature(false);
            }
            catch (Exception ex)
            {
                logger.Error($"The digital signature of the file '{filePath}' was invalid. " +
                    $"The returned Exception from the verification function was {ex}");
                return false;
            }

            if (pinnedThumbprint == null)
            {
                logger.Error("The pinned thumbprint was null.");
                return false;
            }

            // Checks whether or not the thumbprint of the certificate used to make the digital signature matches the pinned thumbprint. 
            var signerCert = signedCms.SignerInfos[0].Certificate;

            if (signerCert == null)
            {
                logger.Error("The digital certificate was not included. The digital signature cannot be verified.");
                return false;
            }

            byte[] hashBytes = signerCert.GetCertHash(HashAlgorithmName.SHA256);

            string certificateThumbprint = BitConverter.ToString(hashBytes).Replace("-", "");

            if (!certificateThumbprint.Equals(pinnedThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                logger.Error("The thumbprint in the license and the thumbprint of the digital certificate were not a match. " +
                    "As such, the digital signature verification failed.");
                return false;
            }

            // If all digital signature verification checks passed, log it and return "true". 
            logger.Info($"The digital signature of the file {filePath} was successfully verified.");
            return true;
        }
    }
}
