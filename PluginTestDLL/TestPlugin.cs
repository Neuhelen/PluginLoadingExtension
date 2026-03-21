using SharedContracts.Interfaces;

namespace PluginTestDLL
{
    public class TestPlugin : IPlugin
    {
        public string HelloName(string name)
        {
            return $"Hello, {name}! " + new InternalDependencyPlugin().DependencyCall();
        }
    }
}
