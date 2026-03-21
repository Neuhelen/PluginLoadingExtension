using PluginDependency;

namespace PluginTestDLL
{
    internal class InternalDependencyPlugin
    {
        private DependencyB dependencyB; 

        public InternalDependencyPlugin()
        {
            dependencyB = new DependencyB("Name");
        }
        public string DependencyCall()
        {
            return $"The dependency, {this.GetType().Name}, is called." + dependencyB.DependencyCheck(); 
        }
    }
}
