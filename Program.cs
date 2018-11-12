using System;
using Newtonsoft.Json;

namespace homeagent
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1 && args[0] == "run")
            {
                string configPath = args[1];
                Config config = Config.Load(configPath);
                Logger.WriteLine($"running with config {JsonConvert.SerializeObject(config, Formatting.Indented)}");

                HomeAgent homeAgent = new HomeAgent(config);
                homeAgent.Test().Wait();
            }
            else
            {
                Logger.WriteLine("didn't understand the command");
            }
        }
    }
}
