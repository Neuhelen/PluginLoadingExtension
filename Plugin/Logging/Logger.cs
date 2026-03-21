using Plugin.Interfaces;

namespace Plugin.Logging
{
    public class Logger : ILogger
    {
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Warn(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        public void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
