namespace DynamoDbBlazor.Services
{
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using DynamoDbBlazor.Data;
    using DynamoDbBlazor.DynamoDB_Actions;
    using System.Threading.Tasks;

    public class DynamoDbService
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly string _tableName = "movie_table";

        public DynamoDbService()
        {
            _client = new AmazonDynamoDBClient();
        }

        public async Task<bool> CreateTableAsync()
        {
            return await DynamoDbMethods.CreateMovieTableAsync(_client, _tableName);
        }

        public async Task<bool> AddMovieAsync(Movie movie)
        {
            return await DynamoDbMethods.PutItemAsync(_client, movie, _tableName);
        }

        public async Task<bool> UpdateMovieAsync(Movie movie, MovieInfo info)
        {
            return await DynamoDbMethods.UpdateItemAsync(_client, movie, info, _tableName);
        }

        public async Task<int> BatchAddMoviesAsync(string filePath)
        {
            return await DynamoDbMethods.BatchWriteItemsAsync(_client, filePath);
        }

        public async Task<Dictionary<string, AttributeValue>> GetMovieAsync(Movie movie)
        {
            return await DynamoDbMethods.GetItemAsync(_client, movie, _tableName);
        }

        public async Task<bool> DeleteMovieAsync(Movie movie)
        {
            return await DynamoDbMethods.DeleteItemAsync(_client, _tableName, movie);
        }

        public async Task<int> QueryMoviesAsync(int year)
        {
            return await DynamoDbMethods.QueryMoviesAsync(_client, _tableName, year);
        }

        public async Task<int> ScanMoviesAsync(int startYear, int endYear)
        {
            return await DynamoDbMethods.ScanTableAsync(_client, _tableName, startYear, endYear);
        }

        public async Task<bool> DeleteTableAsync()
        {
            return await DynamoDbMethods.DeleteTableAsync(_client, _tableName);
        }
    }
}
