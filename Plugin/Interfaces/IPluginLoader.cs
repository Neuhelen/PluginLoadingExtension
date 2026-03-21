using System;

namespace Plugin.Interfaces
{
    public interface IPluginLoader
    {
        bool LoadPluginFromDLL(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint);

        bool LoadPluginFromZip(Type pluginType, string fileName, string pluginIdentifier, string pinnedThumbprint); 

        object InstantiatePlugin(Type pluginType, string pluginName, params object[] args);

        object GetPlugin(Type pluginType, string pluginIdentifier); 
    }
}
