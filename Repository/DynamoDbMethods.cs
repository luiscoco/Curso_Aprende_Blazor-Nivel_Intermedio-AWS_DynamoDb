namespace DynamoDbBlazor.Repository
{
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DynamoDbBlazor.Data;

    public static class DynamoDbMethods
    {
        public static async Task<List<string>> ListTablesAsync(AmazonDynamoDBClient client)
        {
            var response = await client.ListTablesAsync();
            return response.TableNames;
        }

        public static async Task<bool> CreateMovieTableAsync(AmazonDynamoDBClient client, string tableName)
        {
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "Year", AttributeType = "N" },
                    new AttributeDefinition { AttributeName = "Title", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "Year", KeyType = "HASH" },  // Partition key
                    new KeySchemaElement { AttributeName = "Title", KeyType = "RANGE" } // Sort key
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            var response = await client.CreateTableAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public static async Task<bool> PutItemAsync(AmazonDynamoDBClient client, Movie movie, string tableName)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
        {
            { "Year", new AttributeValue { N = movie.Year.ToString() } },
            { "Title", new AttributeValue { S = movie.Title } }
        },
                // Alias 'Year' since it's a reserved keyword
                ConditionExpression = "attribute_not_exists(#yr) AND attribute_not_exists(Title)",
                ExpressionAttributeNames = new Dictionary<string, string>
        {
            { "#yr", "Year" }  // Alias the reserved word 'Year'
        }
            };

            try
            {
                var response = await client.PutItemAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Amazon.DynamoDBv2.Model.ConditionalCheckFailedException)
            {
                // Movie already exists, throw this exception to handle in UI
                throw;
            }
            catch (Exception ex)
            {
                // Log the exact exception message for debugging
                Console.WriteLine($"Error adding movie: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> UpdateItemAsync(AmazonDynamoDBClient client, Movie movie, MovieInfo info, string tableName)
        {
            var request = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Year", new AttributeValue { N = movie.Year.ToString() } },
                    { "Title", new AttributeValue { S = movie.Title } }
                },
                AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                {
                    {
                        "Plot", new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = info.Plot }
                        }
                    },
                    {
                        "Rank", new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { N = info.Rank.ToString() }
                        }
                    }
                }
            };

            var response = await client.UpdateItemAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public static async Task<int> BatchWriteItemsAsync(AmazonDynamoDBClient client, string filePath)
        {
            // Code to read from JSON and batch write items. Example assumes you have a method to read from the file.

            // Example of the batch write logic (pseudo):
            int itemCount = 0;
            // Assume ReadMoviesFromJson(filePath) returns a list of Movie objects
            var movies = ReadMoviesFromJson(filePath);

            foreach (var movie in movies)
            {
                await PutItemAsync(client, movie, "movie_table");
                itemCount++;
            }
            return itemCount;
        }

        public static async Task<Dictionary<string, AttributeValue>> GetItemAsync(AmazonDynamoDBClient client, Movie movie, string tableName)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Year", new AttributeValue { N = movie.Year.ToString() } },
                    { "Title", new AttributeValue { S = movie.Title } }
                }
            };

            var response = await client.GetItemAsync(request);
            return response.Item;
        }

        public static async Task<bool> DeleteItemAsync(AmazonDynamoDBClient client, string tableName, Movie movie)
        {
            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Year", new AttributeValue { N = movie.Year.ToString() } },
                    { "Title", new AttributeValue { S = movie.Title } }
                }
            };

            var response = await client.DeleteItemAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public static async Task<int> QueryMoviesAsync(AmazonDynamoDBClient client, string tableName, int year)
        {
            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "#yr = :v_year",
                        ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#yr", "Year" } // Aliasing the reserved word 'Year'
                },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":v_year", new AttributeValue { N = year.ToString() } }
                }
            };

            var response = await client.QueryAsync(request);
            return response.Count;
        }

        public static async Task<int> ScanTableAsync(AmazonDynamoDBClient client, string tableName, int startYear, int endYear)
        {
            var request = new ScanRequest
            {
                TableName = tableName,
                FilterExpression = "Year between :start_year and :end_year",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":start_year", new AttributeValue { N = startYear.ToString() } },
                    { ":end_year", new AttributeValue { N = endYear.ToString() } }
                }
            };

            var response = await client.ScanAsync(request);
            return response.Count;
        }

        public static async Task<bool> DeleteTableAsync(AmazonDynamoDBClient client, string tableName)
        {
            var response = await client.DeleteTableAsync(new DeleteTableRequest { TableName = tableName });
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        // Helper method to read movies from a JSON file
        private static List<Movie> ReadMoviesFromJson(string filePath)
        {
            // Implement JSON parsing logic here and return list of movies
            return new List<Movie>();
        }

        public static async Task<List<Movie>> QueryMoviesFromTableAsync(AmazonDynamoDBClient client, string tableName)
        {
            var request = new ScanRequest
            {
                TableName = tableName
            };

            var response = await client.ScanAsync(request);

            List<Movie> movies = new List<Movie>();

            foreach (var item in response.Items)
            {
                Movie movie = new Movie
                {
                    Year = int.Parse(item["Year"].N),
                    Title = item["Title"].S
                };
                movies.Add(movie);
            }

            return movies;
        }
    }
}
