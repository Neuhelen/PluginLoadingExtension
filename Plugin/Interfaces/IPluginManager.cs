using System;
using System.Collections.Generic;

namespace Plugin.Interfaces
{
    public interface IPluginManager
    {
        List<object> CreatePlugins(Type pluginType, string pluginId, params object[] parameters);

        object InvokeMethod(object pluginInstance, string methodName, params object[] args);

        object GetPluginFromFileName(Type pluginType, string fileName);

        object GetPluginFromPluginIdentifier(Type pluginType, string pluginIdentifier); 
    }
}
