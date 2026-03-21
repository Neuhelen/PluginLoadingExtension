namespace Plugin.Interfaces
{
    public interface IPluginVerifier
    {
        bool VerifyDigitalSignature(string signaturePath, string assemblyPath, string pinnedThumbprint); 
    }
}
