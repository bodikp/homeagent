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
        public class Config
        {
            public string ConnectionString { get; set; } = "mongodb://localhost:27017";
            public string DatabaseName { get; set; } = "homeagent-data-test";
            public string TemperatureColName { get; set; } = "temperatures";
            public string WeatherColName { get; set; } = "weather";
        }

        private Config config = new Config();

        private MongoClient client;
        private IMongoDatabase database;

        private IMongoCollection<NestThermostatMeasurementEvent> temperaturesCollection;
        private IMongoCollection<WeatherObservation> weatherCollection;

        public MongoDbStorage()
        {
            this.client = new MongoClient(this.config.ConnectionString);
            this.database = this.client.GetDatabase(this.config.DatabaseName);

            this.temperaturesCollection = this.database.GetCollection<NestThermostatMeasurementEvent>(this.config.TemperatureColName);
            this.weatherCollection = this.database.GetCollection<WeatherObservation>(this.config.WeatherColName);
        }

        public async Task AddTemperatureData(NestThermostatMeasurementEvent data)
        {
            await this.temperaturesCollection.InsertOneAsync(data);
        }

        public async Task AddWeatherObservation(WeatherObservation weatherObservation) {
            await this.weatherCollection.InsertOneAsync(weatherObservation);
        }
    }
}