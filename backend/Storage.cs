using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using static homeagent.NestClient;
using static homeagent.WeatherGovClient;

namespace homeagent
{
    public interface IStorage
    {

    }

    public class MongoDbStorage : IStorage
    {
        public class MongoDbStorageConfig
        {
            public string ConnectionString { get; set; } = "mongodb://localhost:27017";
            public string DatabaseName { get; set; } = "homeagent-data-prod";
            public string ThermostatEventsColName { get; set; } = "thermostatEvents";
            public string TemperatureColName { get; set; } = "temperatures";
            public string WeatherColName { get; set; } = "weather";
        }

        private MongoDbStorageConfig config = new MongoDbStorageConfig();

        private MongoClient client;
        private IMongoDatabase database;

        // private IMongoCollection<NestThermostatMeasurementEvent> temperaturesCollection;
        private IMongoCollection<NestThermostatMeasurementEvent> thermostatEventsCollection;
        private IMongoCollection<WeatherObservation> weatherCollection;

        public MongoDbStorage(MongoDbStorageConfig config)
        {
            this.config = config;

            this.client = new MongoClient(this.config.ConnectionString);
            this.database = this.client.GetDatabase(this.config.DatabaseName);

            this.thermostatEventsCollection = this.database.GetCollection<NestThermostatMeasurementEvent>(this.config.ThermostatEventsColName);
            this.weatherCollection = this.database.GetCollection<WeatherObservation>(this.config.WeatherColName);
        }

        public async Task AddThermostatEvent(NestThermostatMeasurementEvent data)
        {
            await this.thermostatEventsCollection.InsertOneAsync(data);
        }

        public async Task AddWeatherObservation(WeatherObservation weatherObservation)
        {
            await this.weatherCollection.InsertOneAsync(weatherObservation);
        }

        public Task<List<NestThermostatMeasurementEvent>> GetThermostatEvents(DateTime t0, DateTime t1)
        {
            //this.thermostatEventsCollection.
            return null;
        }

        public async Task Test()
        {
            var events = (await this.thermostatEventsCollection.FindAsync(e => e.MeasurementTime > DateTime.MinValue)).ToList();

            foreach (var e in events.OrderBy(x => x.MeasurementTime))
            {
                Console.WriteLine($"{e.MeasurementTime.ToLocalTime()}: {e.AmbientTemperatureF}, {e.HvacState}");
            }
        }
    }
}