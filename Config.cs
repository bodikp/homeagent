using System.IO;
using Newtonsoft.Json;
using static homeagent.WeatherGovClient;

namespace homeagent
{
    public class Config
    {
        public NestConfig NestConfig { get; set; }
        public WeatherGovConfig WeatherGovConfig { get; set; }

        public static Config Load(string path) => JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

        public void Store(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class NestConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}