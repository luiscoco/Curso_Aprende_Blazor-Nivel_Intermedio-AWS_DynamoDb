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

        public async Task<List<string>> ListTablesAsync()
        {
            return await DynamoDbMethods.ListTablesAsync(_client);
        }

        public async Task<bool> CreateTableAsync(string tablename)
        {
            return await DynamoDbMethods.CreateMovieTableAsync(_client, tablename);
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

        public async Task<bool> DeleteTableAsync(string tablename)
        {
            return await DynamoDbMethods.DeleteTableAsync(_client, tablename);
        }

        public async Task<bool> AddMovieToTableAsync(Movie movie, string tableName)
        {
            return await DynamoDbMethods.PutItemAsync(_client, movie, tableName);
        }

        public async Task<List<Movie>> GetMoviesFromTableAsync(string tableName)
        {
            var request = new ScanRequest
            {
                TableName = tableName
            };

            var response = await _client.ScanAsync(request);

            List<Movie> movies = new List<Movie>();

            // Process each item in the response
            foreach (var item in response.Items)
            {
                try
                {
                    // Ensure 'Year' and 'Title' fields exist in the item
                    if (item.ContainsKey("Year") && item.ContainsKey("Title"))
                    {
                        Movie movie = new Movie
                        {
                            Year = int.Parse(item["Year"].N),  // Convert DynamoDB 'Number' to int
                            Title = item["Title"].S             // Extract 'String' directly
                        };

                        movies.Add(movie);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing movie data: {ex.Message}");
                }
            }

            return movies;
        }

        public async Task<bool> DeleteMovieAsync(Movie movie, string tableName)
        {
            try
            {
                return await DynamoDbMethods.DeleteItemAsync(_client, tableName, movie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting movie: {ex.Message}");
                return false;
            }
        }


    }
}
