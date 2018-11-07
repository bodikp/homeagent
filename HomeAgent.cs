using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static homeagent.NestClient;
using static homeagent.WeatherGovClient;

namespace homeagent
{
    public class HomeAgent
    {
        private NestClient NestClient;
        private WeatherGovClient WeatherGovClient;

        public HomeAgent(Config config)
        {
            this.NestClient = new NestClient(config.NestConfig);
            this.WeatherGovClient = new WeatherGovClient(config.WeatherGovConfig);
        }

        public async Task Test()
        {
            if (this.NestClient != null)
            {
                StreamWriter file = new StreamWriter($"data_{DateTime.Now.Ticks}.tab");
                file.WriteLine(string.Join("\t", NestThermostatMeasurementEvent.GetHeader()));

                while (true)
                {
                    try
                    {
                        List<NestThermostatMeasurementEvent> events = await this.NestClient.Measure();

                        foreach (var e in events)
                        {
                            file.WriteLine(string.Join("\t", e.GetStringValues()));
                        }
                        file.Flush();

                        Console.WriteLine(JsonConvert.SerializeObject(events, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    WeatherObservation weather = await this.WeatherGovClient.GetCurrentWeather();
                    Console.WriteLine(JsonConvert.SerializeObject(weather, Formatting.Indented));
                    await Task.Delay(60 * 1000);
                }
            }
        }
    }
}