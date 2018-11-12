using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using frontend.Models;
using System.IO;
using homeagent;
using static homeagent.NestClient;

namespace frontend.Controllers
{
    public class HomeController : Controller
    {
        static Config config;
        static MongoDbStorage storage;

        static HomeController()
        {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configPath = $"{exePath}/config.json";
            config = Config.Load(configPath);
            storage = new MongoDbStorage(config.MongoDbStorageConfig);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/data")]
        public async Task<JsonResult> Data(TimeSpan duration, DateTime? endTime = null)
        {
            if (!endTime.HasValue)
            {
                endTime = DateTime.Now;
            }
            DateTime t0 = endTime.Value - duration;

            List<NestThermostatMeasurementEvent> thermostatData = await storage.GetThermostatEvents(t0, endTime.Value);

            List<ThermostatDatum> thermostatChartData =
            thermostatData.Select(d => new ThermostatDatum
            {
                Time = new DateTimeOffset(d.MeasurementTime).ToUnixTimeSeconds().ToString(),
                AmbientTemperature = d.AmbientTemperatureF,
                TargetTemperature = d.TargetTemperatureF,
            }).OrderBy(x => x.Time).ToList();

            DataResponse response = new DataResponse
            {
                TemperatureUnit = "F",
                ThermostatData = thermostatChartData
            };

            return new JsonResult(response);
        }

        public class ThermostatDatum
        {
            public string Time { get; set; }
            public double AmbientTemperature { get; set; }
            public double TargetTemperature { get; set; }
            public double? HeatingOn { get; set; }
        }

        public class DataResponse
        {
            public string TemperatureUnit { get; set; }

            public List<ThermostatDatum> ThermostatData { get; set; }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
