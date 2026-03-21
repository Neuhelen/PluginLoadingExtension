namespace Plugin.Interfaces
{
    internal interface IAssemblyInspector
    {
        string GetFullName(byte[] assemblyBytes); 
    }
}
