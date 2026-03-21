using Plugin.Interfaces;
using Plugin.Logging;
using System;
using System.Reflection;

namespace Plugin.PluginHost
{
    public class MethodInvoker : IMethodInvoker
    {
        private static readonly ILogger logger = new Logger(); 

        // Use a private static readonly logger. 
        public object InvokeMethod(object pluginInstance, string methodName, params object[] args)
        {
            if (pluginInstance == null)
            {
                logger.Warn("Plugin instance is null.");
                return null;
            }

            var type = pluginInstance.GetType();

            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);

            if (method == null)
            {
                logger.Error($"Method '{methodName}' not found in plugin instance of type '{type.FullName}'.");
                return null;
            }

            try
            {
                return method.Invoke(pluginInstance, args);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to invoke method '{methodName}': {ex.Message}");
                return null;
            }
        }
    }
}
