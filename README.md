# How to manage AWS DynamoDb Tables and Items from Blazor Web App

Note: this tutorial is based on the official AWS DynamoDB Samples

https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/csharp_dynamodb_code_examples.html

## 1. Create Blazor Web App



## 2. Load Nuget package

Load the AWSSDK.DynamoDBv2 package, see the figure for more details:

![image](https://github.com/user-attachments/assets/4b999c1a-95c1-4e13-88f5-1faf1e3a2e2e)

## 3. Create Data Models

This code defines two classes, **Movie** and **MovieInfo**, within the **DynamoDbBlazor.Data** namespace

![image](https://github.com/user-attachments/assets/973fcc81-7199-47ea-ac3a-833cf76d1e03)

These classes are used in a Blazor application that interacts with **DynamoDB**, an **AWS NoSQL** database service

**Namespace DynamoDbBlazor.Data**: The namespace groups these classes logically, part of a Blazor project that deals with **DynamoDB** data operations

**Class Movie**: The Movie class stores basic movie information like the year and title

See the source code: 

**Movie.cs**

```csharp
namespace DynamoDbBlazor.Data
{
    public class Movie
    {
        public int Year { get; set; }
        public string? Title { get; set; }
    }
}
```

**Contains two properties**:

**Year (an integer)**: Represents the year of the movie

**Title (a nullable string string?)**: Represents the title of the movie. The ? indicates that the Title property can hold a null value

**Class MovieInfo**: MovieInfo class holds additional details such as the plot and ranking of the movie

See the source code:

**MovieInfo.cs**

```csharp
namespace DynamoDbBlazor.Data
{
    public class MovieInfo
    {
        public string? Plot { get; set; }
        public int Rank { get; set; }
    }
}
```

**Contains two properties**:

**Plot (a nullable string string?**: Represents the plot description of the movie. Again, ? indicates that this property can be null

**Rank (an integer)**: Represents the ranking of the movie

These models are used when interacting with a **DynamoDB** table, storing or retrieving movie-related data within the Blazor framework

## 4. Create Data Repository

This code defines a static class **DynamoDbMethods** within the **DynamoDbBlazor.Repository** namespace, providing various methods to interact with an Amazon DynamoDB database

It uses the AWS SDK for .NET to perform common database operations like listing tables, creating a table, inserting, updating, retrieving, and deleting items:

**ListTablesAsync**: Retrieves a list of all DynamoDB table names using the ListTablesAsync() method from the AmazonDynamoDBClient

**CreateMovieTableAsync**: Creates a DynamoDB table for storing movie data

Defines attributes (Year, Title), key schema (partition key Year and sort key Title), and provisioned throughput for read/write operations

Returns a boolean indicating whether the table was created successfully

**PutItemAsync**: Inserts a new movie item into the specified DynamoDB table

Checks if the movie already exists using a condition expression before adding it to avoid duplicates

Returns a boolean indicating whether the operation succeeded

**UpdateItemAsync**: Updates an existing movie item in the table by changing its Plot and Rank attributes

Uses UpdateItemAsync() to perform the update operation

Returns a boolean based on the success of the operation

**BatchWriteItemsAsync**: A placeholder method that reads movies from a JSON file and writes multiple movie items in batch to the DynamoDB table using the PutItemAsync method

**GetItemAsyn**: Retrieves a specific movie item based on the partition key (Year) and sort key (Title) from the specified table

Returns the retrieved item as a dictionary of AttributeValue

**DeleteItemAsync**: Deletes a movie item from the DynamoDB table based on the specified keys (Year and Title)

Returns a boolean indicating whether the delete operation was successful

**QueryMoviesAsync**: Queries movies from the DynamoDB table based on the Year using a key condition expression

Returns the number of matching items

**ScanTableAsync**: Scans the entire table and retrieves all movie items where the Year is between a specified range (startYear and endYear)

Returns the count of matching items

**DeleteTableAsync**: Deletes the specified DynamoDB table

Returns a boolean indicating if the operation was successful

**ReadMoviesFromJson**: A placeholder helper method for reading movie data from a JSON file (not implemented in this snippet)

**QueryMoviesFromTableAsync**: Scans the DynamoDB table and retrieves all movie items, returning a list of Movie objects

**Additional Concepts**:

**AmazonDynamoDBClient**: A client object provided by the AWS SDK that handles requests to the DynamoDB service

**Attribute Definitions**: Specifies the attributes and their data types for the table (e.g., N for numbers, S for strings)

**Key Schema**: Defines the primary key structure, consisting of a partition key (HASH) and an optional sort key (RANGE)

**Condition Expression**: Used in PutItemAsync to ensure that the movie item does not already exist before insertion

**Provisioned Throughput**: Specifies the read and write capacity units for the table

**Exception Handling**: Catches specific exceptions like ConditionalCheckFailedException for conditional write failures and logs other exceptions for debugging

This code provides basic **CRUD (Create, Read, Update, Delete)** operations and scanning/querying functionality for a DynamoDB table that stores movie data

**DynamoDbMethods**

```csharp
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
```

## 5. Create Service

This code defines a class DynamoDbService within the **DynamoDbBlazor.Services** namespace, which acts as a service layer to interact with Amazon DynamoDB through a set of predefined methods

This service encapsulates the logic for working with a DynamoDB table, specifically handling **CRUD operations** and other DynamoDB actions

**Key Components**:

**AmazonDynamoDBClient**: This is the client object from the AWS SDK that facilitates communication with DynamoDB. The class creates an instance of this client (_client) for all its interactions with DynamoDB

**_tableName**: The default table name is hardcoded as "movie_table", which is used for most of the operations involving movies

**Methods**: 

**ListTablesAsync**: Calls the DynamoDbMethods.ListTablesAsync method to retrieve a list of all DynamoDB tables using the provided client (_client)

**CreateTableAsync**: Calls DynamoDbMethods.CreateMovieTableAsync to create a new DynamoDB table with the specified name

**AddMovieAsync**: Adds a new movie to the DynamoDB table (movie_table) by calling the PutItemAsync method from DynamoDbMethods

**UpdateMovieAsync**: Updates an existing movie entry in the table by calling UpdateItemAsync. It takes a Movie and MovieInfo object to update specific fields.

**BatchAddMoviesAsync**: Performs a batch insertion of movie records from a file. This method calls BatchWriteItemsAsync to read movies from a JSON file and add them to the table

**GetMovieAsync**: Retrieves a movie record from the DynamoDB table based on the Year and Title. It uses the GetItemAsync method from DynamoDbMethods

**DeleteMovieAsync**: Deletes a movie entry from the default movie_table by calling DeleteItemAsync

**QueryMoviesAsync**: Queries the specified table (tablename) to find all movies from a given year by calling QueryMoviesAsync

**ScanMoviesAsync**: Scans the entire movie_table for movies within a given year range (startYear to endYear) using the ScanTableAsync method

**DeleteTableAsync**: Deletes a specified DynamoDB table by calling the DeleteTableAsync method

**AddMovieToTableAsync**: Adds a movie to a specific DynamoDB table (tableName) by calling the PutItemAsync method. It differs from AddMovieAsync by allowing the user to specify a table name

**GetMoviesFromTableAsync**: Scans the specified table (tableName) and retrieves all movie items. It uses the ScanRequest to get all items and processes each one to construct a list of Movie objects

**DeleteMovieAsync (overloaded)**: Deletes a movie from a specified table (tableName) by calling DeleteItemAsync. This method is a more flexible version of the earlier DeleteMovieAsync, allowing for the table name to be passed in

**Key Concepts**:

**Encapsulation of DynamoDbMethods**: The DynamoDbService class serves as an intermediary, simplifying the interaction with DynamoDbMethods. This way, the logic of working with the DynamoDB client is abstracted, making it easier for other parts of the application to perform DynamoDB operations

**Async Operations**: All methods are asynchronous (Task<T>) to ensure that DynamoDB operations do not block the main application thread, which is essential for scalability and performance, especially in cloud-based applications

**Exception Handling**: The code includes basic error handling (e.g., in DeleteMovieAsync and GetMoviesFromTableAsync) to catch exceptions and log errors, ensuring the application is more robust

See the source code for more details:

**DynamoDbService.cs**

```csharp
namespace DynamoDbBlazor.Services
{
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using DynamoDbBlazor.Data;
    using DynamoDbBlazor.Repository;
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

        public async Task<int> QueryMoviesAsync(int year, string tablename)
        {
            return await DynamoDbMethods.QueryMoviesAsync(_client, tablename, year);
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
```

## 6. Modify the middleware (Program.cs)

Add this line to register the DynamoDb Service:

```csharp
builder.Services.AddSingleton<DynamoDbService>();
```

This is the whole code in the **Program.cs** file:

```csharp
using DynamoDbBlazor.Components;
using DynamoDbBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DynamoDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

## 7. Create a Component for Creating and deleting AWS DynamoDb Tables

![image](https://github.com/user-attachments/assets/dd6b45e3-7694-4eab-85fe-f21b5368b89a)

The following code creates and deletes AWS DynamoDB Tables: 

**AddItem.razor**

```razor
@page "/add-item"
@using DynamoDbBlazor.Data
@using DynamoDbBlazor.Services

@inject DynamoDbService DynamoDbService

<div class="container mt-4">
    <h3 class="text-primary mb-4">Add Movie</h3>

    <!-- Dropdown for selecting a DynamoDB table -->
    <div class="mb-3">
        <label for="tableSelect" class="form-label">Select Table:</label>
        @if (tables == null)
        {
            <div>Loading tables...</div>
        }
        else if (tables.Count == 0)
        {
            <div>No tables found in AWS DynamoDB.</div>
        }
        else
        {
            <select id="tableSelect" class="form-select" @bind="SelectedTable">
                <option value="">-- Select a table --</option>
                @foreach (var table in tables)
                {
                    <option value="@table">@table</option>
                }
            </select>
        }
    </div>

    <!-- Movie entry form -->
    @if (!string.IsNullOrEmpty(SelectedTable))
    {
        <EditForm Model="@newMovie" OnValidSubmit="@SubmitMovie" class="needs-validation">
            <DataAnnotationsValidator />
            <ValidationSummary class="alert alert-danger" />

            <div class="mb-3">
                <label for="year" class="form-label">Year:</label>
                <InputNumber id="year" @bind-Value="newMovie.Year" class="form-control" />
            </div>

            <div class="mb-3">
                <label for="title" class="form-label">Title:</label>
                <InputText id="title" @bind-Value="newMovie.Title" class="form-control" />
            </div>

            <button type="submit" class="btn btn-success">Add Movie</button>
        </EditForm>
    }

    <!-- Success or error messages -->
    @if (isSuccess)
    {
        <div class="alert alert-success mt-3">Movie added successfully!</div>
    }
    @if (isError && !movieAlreadyExists)
    {
        <div class="alert alert-danger mt-3">Failed to add movie. Please try again later.</div>
        <div class="alert alert-danger mt-3">@errorMessage</div>
    }
    @if (movieAlreadyExists)
    {
        <div class="alert alert-warning mt-3">This movie already exists in the database.</div>
    }

    <!-- Display items from the selected table -->
    @if (itemsInTable != null && itemsInTable.Count > 0)
    {
        <h4 class="mt-4">Items in '@SelectedTable' Table:</h4>
        <ul class="list-group">
            @foreach (var item in itemsInTable)
            {
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <span>Year: @item.Year, Title: @item.Title</span>
                    <button class="btn btn-danger btn-sm" @onclick="(() => DeleteMovie(item))">Delete</button>
                </li>
            }
        </ul>
    }
    else if (!string.IsNullOrEmpty(SelectedTable))
    {
        <div class="alert alert-info mt-3">No items found in the selected table.</div>
    }
</div>

@code {
    private Movie newMovie = new Movie();
    private bool isSuccess = false;
    private bool isError = false;
    private bool movieAlreadyExists = false;
    private string errorMessage = string.Empty;

    // Variables for listing and selecting tables
    private List<string> tables = new List<string>();
    private string selectedTable = "";

    // List to store items (movies) from the selected table
    private List<Movie> itemsInTable = new List<Movie>();

    protected override async Task OnInitializedAsync()
    {
        await LoadExistingTablesAsync();
    }

    private async Task LoadExistingTablesAsync()
    {
        try
        {
            // Load the list of DynamoDB tables from AWS
            tables = await DynamoDbService.ListTablesAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error loading tables: {ex.Message}";
            isError = true;
        }
    }

    // Load table items automatically when the SelectedTable value changes
    private string SelectedTable
    {
        get => selectedTable;
        set
        {
            if (selectedTable != value)
            {
                selectedTable = value;
                LoadTableItems().ConfigureAwait(false);
            }
        }
    }

    private async Task LoadTableItems()
    {
        Console.WriteLine($"Loading items from table: {selectedTable}");
        itemsInTable.Clear();
        if (!string.IsNullOrEmpty(selectedTable))
        {
            try
            {
                // Load the items (movies) from the selected table
                itemsInTable = await DynamoDbService.GetMoviesFromTableAsync(selectedTable);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading items from table '{selectedTable}': {ex.Message}";
                isError = true;
            }
        }
    }

    private async Task SubmitMovie()
    {
        // Clear previous flags and messages
        isSuccess = false;
        isError = false;
        movieAlreadyExists = false;
        errorMessage = string.Empty;

        // Ensure a table is selected
        if (string.IsNullOrEmpty(selectedTable))
        {
            isError = true;
            errorMessage = "Please select a DynamoDB table.";
            return;
        }

        try
        {
            // Try to add the movie using the selected DynamoDB table
            isSuccess = await DynamoDbService.AddMovieToTableAsync(newMovie, selectedTable);

            // If the movie was added successfully, refresh the items list
            if (isSuccess)
            {
                await LoadTableItems();
            }
            else
            {
                isError = true;
            }
        }
        catch (Amazon.DynamoDBv2.Model.ConditionalCheckFailedException)
        {
            // Handle the case where the movie already exists
            movieAlreadyExists = true;
        }
        catch (Exception ex)
        {
            // Capture the specific error message and display it
            errorMessage = ex.Message;
            isError = true;
        }
    }

    private async Task DeleteMovie(Movie movie)
    {
        try
        {
            bool success = await DynamoDbService.DeleteMovieAsync(movie, selectedTable);
            if (success)
            {
                // Remove the movie from the list if deleted successfully
                itemsInTable.Remove(movie);
                StateHasChanged();
            }
            else
            {
                errorMessage = $"Failed to delete movie: {movie.Title}.";
                isError = true;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error deleting movie '{movie.Title}': {ex.Message}";
            isError = true;
        }
    }
}
```

## 8. Create a Component for Adding and deleting Items inside the Tables

**QueryItems.razor**

```razor
@page "/query-item"

@using DynamoDbBlazor.Data
@using DynamoDbBlazor.Services
@inject DynamoDbService DynamoDbService

<div class="container mt-4">
    <h3 class="text-primary mb-4">Query Movies by Year</h3>

    <!-- Dropdown list for selecting a DynamoDB table -->
    <div class="mb-3">
        <label for="tableSelect" class="form-label">Select Table:</label>
        @if (tables == null)
        {
            <div>Loading tables...</div>
        }
        else if (tables.Count == 0)
        {
            <div>No tables found in AWS DynamoDB.</div>
        }
        else
        {
            <select id="tableSelect" class="form-select" @bind="tableName">
                <option value="">-- Select a table --</option>
                @foreach (var table in tables)
                {
                    <option value="@table">@table</option>
                }
            </select>
        }
    </div>

    <EditForm Model="@searchYear" OnValidSubmit="@QueryMoviesFunction" class="needs-validation">
        <div class="mb-3">
            <label for="year" class="form-label">Year:</label>
            <InputNumber id="year" @bind-Value="searchYear" class="form-control" />
        </div>
        <button type="submit" class="btn btn-primary" disabled="@string.IsNullOrEmpty(tableName)">Search</button>
    </EditForm>

    @if (moviesFound >= 0)
    {
        <div class="alert alert-info mt-3">Found @moviesFound movies from @searchYear in table @tableName.</div>
    }
</div>

@code {
    private int searchYear = 0;
    private int moviesFound = -1;
    private string tableName = ""; // Variable to store the selected table name
    private List<string> tables = new List<string>(); // To store the list of tables from AWS

    protected override async Task OnInitializedAsync()
    {
        await LoadExistingTablesAsync(); // Load tables when the component initializes
    }

    // Method to load existing DynamoDB tables
    private async Task LoadExistingTablesAsync()
    {
        try
        {
            tables = await DynamoDbService.ListTablesAsync(); // Get the list of tables from DynamoDB
        }
        catch (Exception ex)
        {
            // Handle error in loading tables
            Console.WriteLine($"Error loading tables: {ex.Message}");
        }
    }

    private async Task QueryMoviesFunction()
    {
        // Ensure the table name is provided
        if (string.IsNullOrEmpty(tableName))
        {
            moviesFound = -1;
            return;
        }

        // Pass the table name and search year to query the DynamoDB table
        moviesFound = await DynamoDbService.QueryMoviesAsync(searchYear, tableName);
    }
}
```

## 9. Create a Component for Searching Items inside a Table

**QueryMovies.razor**

```razor

```

## 10. Add the MenuItems for accessing the above new components

