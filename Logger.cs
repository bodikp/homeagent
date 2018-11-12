using System;
using System.IO;

namespace homeagent
{
    public class Logger
    {
        static string directory = "_logs";
        static StreamWriter log;

        static Logger()
        {
            Directory.CreateDirectory(directory );
            log = new StreamWriter($"{directory}/log_{DateTime.Now:o}.txt");
        }

        public static void WriteLine(string message)
        {
            string msg = $"{DateTime.Now}: {message}";
            log.WriteLine(msg);
            log.Flush();
            Console.WriteLine(msg);
        }

        public static void WriteLine(Exception e) {
            WriteLine(e.ToString());
        }
    }
}