using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Converters;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace homeagent
{
    public class NestClient
    {
        const string pinCachePath = "nest_pin_cache.json";
        const string accessTokenCachePath = "nest_access_token_cache.json";

        private NestConfig Config;

        private string accessToken;

        public NestClient(NestConfig config)
        {
            this.Config = config;

            this.LoadAccessToken();
            if (this.accessToken == null)
            {
                string pin = this.RequestPin();
                Console.WriteLine($"using PIN: {pin}");

                this.accessToken = RequestAccessToken(pin).Result;
                this.CacheAccessToken();
            }
            Console.WriteLine($"using the following access token: {this.accessToken}");
        }

        public async Task<List<NestThermostatMeasurementEvent>> Measure()
        {
            NestApiResponse response = await this.GetAllData();
            List<NestThermostatMeasurementEvent> events = new List<NestThermostatMeasurementEvent>();

            foreach (var thermostat in response.devices.thermostats.Values)
            {
                NestThermostatMeasurementEvent e = new NestThermostatMeasurementEvent
                {
                    MeasurementTime = DateTime.UtcNow,
                    ThermostatId = thermostat.device_id,
                    Name = thermostat.name,
                    WhereId = thermostat.where_id,
                    WhereName = thermostat.where_name,
                    LastContactTime = thermostat.last_connection,
                    Humidity = thermostat.humidity,
                    TemperatureScale = thermostat.temperature_scale,
                    TargetTemperatureF = thermostat.target_temperature_f,
                    AmbientTemperatureF = thermostat.ambient_temperature_f,
                    HvacMode = thermostat.hvac_mode,
                    HvacState = thermostat.hvac_state,
                };

                events.Add(e);
            }

            return events;
        }

        private async Task<NestApiResponse> GetAllData()
        {
            var client = new RestClient("https://developer-api.nest.com");
            client.FollowRedirects = false;

            var request = new RestRequest(Method.GET);
            request.AddHeader("Postman-Token", "390fb86d-70f2-43bf-ac00-6882ad92e9d2");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "Bearer c.CxjcIdeXNmvhHFM1aGKHB0k07bPQMCrAQMdGoDAouDqnRzCR6cF9V6OOZ38s4h8kUYnEnC6wYnQbdJEScy15PhQAQUnVjZJrfNJu1RXfJ6SwbgRyS6Kv9lMuwZ9azCBhG72XiDCmDBRTtP0K");
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = await client.ExecuteTaskAsync(request);

            if (response.StatusCode == HttpStatusCode.RedirectKeepVerb)
            {
                var locationHeader = response.Headers.ToList().Where(h => h.Name == "Location").First();
                client.BaseUrl = new Uri(locationHeader.Value.ToString());
                response = await client.ExecuteTaskAsync(request);
            }

            NestApiResponse apiResponse = JsonConvert.DeserializeObject<NestApiResponse>(response.Content);
            // Console.WriteLine(JsonConvert.SerializeObject(apiResponse, Formatting.Indented));

            return apiResponse;
        }

        private string RequestPin()
        {
            Console.WriteLine(value: $"Navigate to the following URL in the browser: ");
            Console.WriteLine($"{GetAuthenticationUrl()}");
            Console.Write("PIN> ");
            string pin = Console.ReadLine();
            return pin;
        }

        private string GetAuthenticationUrl() => $"https://home.nest.com/login/oauth2?client_id={this.Config.ClientId}&state=STATE";

        private async Task<string> RequestAccessToken(string pin)
        {
            string url = "https://api.home.nest.com/oauth2/access_token";

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string bodyString = $"client_id={this.Config.ClientId}&client_secret={this.Config.ClientSecret}&grant_type=authorization_code&code={pin}";

                byte[] responseBytes = await wc.UploadDataTaskAsync(url, Encoding.ASCII.GetBytes(bodyString));
                string responseJson = Encoding.ASCII.GetString(responseBytes);
                AuthenticationTokenResponse response = JsonConvert.DeserializeObject<AuthenticationTokenResponse>(responseJson);

                return response.access_token;
            }
        }

        private void CacheAccessToken()
        {
            if (accessToken != null)
            {
                File.WriteAllText(accessTokenCachePath, accessToken);
            }
        }

        private void LoadAccessToken()
        {
            if (File.Exists(accessTokenCachePath))
            {
                this.accessToken = File.ReadAllText(accessTokenCachePath);
            }
            else
            {
                this.accessToken = null;
            }
        }

        private class AuthenticationTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }

        public enum HvacMode
        {
            heat,
            cool,
            heatcool,
            eco,
            off
        }

        public enum HvacState
        {
            heating,
            cooling,
            off
        }

        public class NestThermostatMeasurementEvent
        {
            public ObjectId _id { get; set; }

            public DateTime MeasurementTime { get; set; }

            public string ThermostatId { get; set; }
            public string Name { get; set; }
            public string WhereId { get; set; }
            public string WhereName { get; set; }

            public DateTime LastContactTime { get; set; }

            public double Humidity { get; set; }

            public string TemperatureScale { get; set; }
            public double TargetTemperatureF { get; set; }
            public double AmbientTemperatureF { get; set; }

            [BsonRepresentation(BsonType.String)]
            [JsonConverter(typeof(StringEnumConverter))]
            public HvacMode HvacMode { get; set; }

            [BsonRepresentation(BsonType.String)]
            [JsonConverter(typeof(StringEnumConverter))]
            public HvacState HvacState { get; set; }

            public static List<string> GetHeader()
            {
                return new List<string> { "measurement time", "thermostat id", "last contact", "humidity", "target_temp", "ambient_temp", "hvac_mode", "hvac_status" };
            }

            public List<string> GetStringValues()
            {
                List<object> data = new List<object> { MeasurementTime, ThermostatId, LastContactTime, Humidity, TargetTemperatureF, AmbientTemperatureF, HvacMode, HvacState };
                List<string> result = data.Select(d => d.ToString()).ToList();
                return result;
            }
        }

        private class NestApiResponse
        {
            public NestDevicesResponse devices { get; set; }

            public Dictionary<string, NestStructureResponse> structures { get; set; } = new Dictionary<string, NestStructureResponse>();
        }

        private class NestDevicesResponse
        {
            public Dictionary<string, NestThermostatResponse> thermostats { get; set; } = new Dictionary<string, NestThermostatResponse>();
        }

        private class NestThermostatResponse
        {
            public double humidity { get; set; }
            public string temperature_scale { get; set; }
            public string where_id { get; set; }
            public string device_id { get; set; }
            public string name { get; set; }
            public double target_temperature_f { get; set; }
            public double target_tempearture_high_f { get; set; }
            public double target_temperature_low_f { get; set; }
            public double ambient_temperature_f { get; set; }
            public string structure_id { get; set; }
            public string previous_hvac_mode { get; set; }
            public HvacMode hvac_mode { get; set; }
            public string time_to_target { get; set; }
            public string where_name { get; set; }
            public string name_long { get; set; }
            public bool is_online { get; set; }
            public DateTime last_connection { get; set; }
            public HvacState hvac_state { get; set; }
        }

        private class NestStructureResponse
        {
            public string name { get; set; }
            public string country_code { get; set; }

            public string time_zone { get; set; }

            public string away { get; set; }
            public List<string> thermostats { get; set; } = new List<string>();

            public string structure_id { get; set; }

            public List<string> cameras { get; set; } = new List<string>();
        }
    }
}