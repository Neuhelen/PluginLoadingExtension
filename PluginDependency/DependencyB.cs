using SecondPluginDependency;

namespace PluginDependency
{
    public class DependencyB
    {
        private NameObject nameObject;
        public DependencyB(string name)
        {
            nameObject = new NameObject();
            nameObject.Name = name;
        }
        public string DependencyCheck()
        {
            return this.GetType() + " has been loaded. The Name is: " + nameObject.Name + ".";
        }
    }
}
