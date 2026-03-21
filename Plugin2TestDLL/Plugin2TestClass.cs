using PluginDependency;
using SharedContracts.Interfaces;

namespace Plugin2TestDLL
{
    public class Plugin2TestClass : IPlugin2
    {
        private DependencyB dependencyB;

        public Plugin2TestClass()
        {
            dependencyB = new DependencyB("Stranger");
        }

        public string HelloStranger(string name)
        {
            return $"Hello, Stranger. Your name is {name}! The dependencies will now be loaded. " + dependencyB.DependencyCheck();
        }
    }
}
