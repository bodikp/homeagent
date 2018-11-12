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

        private MongoDbStorage Storage;

        public HomeAgent(Config config)
        {
            this.NestClient = new NestClient(config.NestConfig);
            this.WeatherGovClient = new WeatherGovClient(config.WeatherGovConfig);
            this.Storage = new MongoDbStorage(config.MongoDbStorageConfig);
        }

        public async Task Test()
        {
            if (this.NestClient != null)
            {
                StreamWriter file = new StreamWriter($"data.tab", true);
                file.WriteLine(string.Join("\t", NestThermostatMeasurementEvent.GetHeader()));

                while (true)
                {
                    try
                    {
                        Logger.WriteLine("querying thermostat");
                        List<NestThermostatMeasurementEvent> events = await this.NestClient.Measure();

                        foreach (var e in events)
                        {
                            file.WriteLine(string.Join("\t", e.GetStringValues()));
                            await this.Storage.AddThermostatEvent(e);
                        }
                        file.Flush();

                        Logger.WriteLine(JsonConvert.SerializeObject(events, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(e);
                    }

                    try
                    {
                        Logger.WriteLine("querying weather");
                        WeatherObservation weather = await this.WeatherGovClient.GetCurrentWeather();
                        await this.Storage.AddWeatherObservation(weather);
                        Logger.WriteLine(JsonConvert.SerializeObject(weather, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(e);
                    }

                    Logger.WriteLine("sleeping");
                    await Task.Delay(60 * 1000);
                }
            }
        }
    }
}