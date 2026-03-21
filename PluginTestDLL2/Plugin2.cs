using PluginDependency;
using SharedContracts.Interfaces;

namespace PluginTestDLL
{
    internal class Plugin2 : IPlugin
    {
        private DependencyB dependencyB; 

        public Plugin2()
        {
            dependencyB = new DependencyB("Name2"); 
        }
        public string HelloName(string name)
        {
            return $"Hello! {this.GetType()} has been loaded. The dependencies will now be loaded. " + dependencyB.DependencyCheck();
        }
    }
}
