using SharedContracts.Interfaces;

namespace Plugin.Test
{
    public class MockPlugin : IPlugin
    {
        public string HelloName(string name)
        {
            return name;
        }

        public string MockNewFunction(string parameter)
        {
            return parameter;
        }
    }
}
