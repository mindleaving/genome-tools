using System;

namespace Commons
{
    public static class Log
    {
        public static void LogError(string message)
        {
            Console.WriteLine(@"Error: " + message);
        }
    }
}
