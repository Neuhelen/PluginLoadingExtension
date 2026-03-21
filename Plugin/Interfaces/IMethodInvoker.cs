namespace Plugin.Interfaces
{
    public interface IMethodInvoker
    {
        object InvokeMethod(object pluginInstance, string methodName, params object[] args);
    }
}
