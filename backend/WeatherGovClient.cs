using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace homeagent
{
    public class WeatherGovClient
    {
        public WeatherGovConfig Config { get; set; }

        public WeatherGovClient(WeatherGovConfig config)
        {
            this.Config = config;
        }

        public async Task<WeatherObservation> GetCurrentWeather()
        {
            string url = $"https://api.weather.gov/stations/{this.Config.ObservationStationId}/observations/latest";

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
                string json = await wc.DownloadStringTaskAsync(url);
                ObservationLatestResponse response = JsonConvert.DeserializeObject<ObservationLatestResponse>(json);

                WeatherObservation result = new WeatherObservation
                {
                    ObservationStation = this.Config.ObservationStationId,
                    Time = response.properties.timestamp,
                    TemperatureF = response.properties.temperatureF,
                    RelativeHumidity = response.properties.relativeHumidity.value,
                    Description = response.properties.textDescription
                };

                return result;
            }
        }

        public class WeatherObservation
        {
            public string ObservationStation { get; set; }
            public DateTime Time { get; set; }

            public double? TemperatureF { get; set; }
            public double? RelativeHumidity { get; set; }
            public string Description { get; set; }
        }

        public class WeatherGovConfig
        {
            public string ObservationStationId { get; set; }
        }

        private class ObservationLatestResponse
        {
            public ObservationLatestProperties properties { get; set; }
        }

        private class ObservationLatestProperties
        {
            public DateTime timestamp { get; set; }
            public string textDescription { get; set; }
            public ObservationLatestValue temperature { get; set; }
            public ObservationLatestValue relativeHumidity { get; set; }

            public double? temperatureF
            {
                get
                {
                    if (!temperature.value.HasValue)
                    {
                        return null;
                    }
                    else if (temperature.unitCode == ObservationLatestValue.degFUnitCode)
                    {
                        return temperature.value;
                    }
                    else if (temperature.unitCode == ObservationLatestValue.degCUnitCode)
                    {
                        return temperature.value * 9.0 / 5 + 32;
                    }
                    else
                    {
                        throw new Exception($"unexpected unit code '{temperature.unitCode}'");
                    }
                }
            }
        }

        private class ObservationLatestValue
        {
            public static string degCUnitCode = "unit:degC";
            public static string degFUnitCode = "unit:degF";

            public double? value { get; set; }
            public string unitCode { get; set; }
        }
    }
}