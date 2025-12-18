using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.AddAzureCosmosClient("cosmos-db");

// Cosmos DB-based store for temperature measurements
builder.Services.AddSingleton<TemperatureMeasurementStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Weather Forecast endpoint
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Temperature Measurements CRUD endpoints
var temperatureGroup = app.MapGroup("/api/temperatures");

temperatureGroup.MapGet("/", async (TemperatureMeasurementStore store) =>
{
    var measurements = await store.GetAllAsync();
    return Results.Ok(measurements);
})
.WithName("GetAllTemperatures");

temperatureGroup.MapGet("/{id:guid}", async (Guid id, TemperatureMeasurementStore store) =>
{
    var measurement = await store.GetByIdAsync(id);
    return measurement is not null ? Results.Ok(measurement) : Results.NotFound();
})
.WithName("GetTemperatureById");

temperatureGroup.MapPost("/", async (CreateTemperatureMeasurement request, TemperatureMeasurementStore store) =>
{
    var measurement = new TemperatureMeasurement(
        Guid.NewGuid(),
        request.Location,
        request.TemperatureC,
        request.RecordedAt ?? DateTime.UtcNow
    );
    var created = await store.AddAsync(measurement);
    return Results.Created($"/api/temperatures/{created.Id}", created);
})
.WithName("CreateTemperature");

temperatureGroup.MapPut("/{id:guid}", async (Guid id, UpdateTemperatureMeasurement request, TemperatureMeasurementStore store) =>
{
    var existing = await store.GetByIdAsync(id);
    if (existing is null)
    {
        return Results.NotFound();
    }

    var updated = new TemperatureMeasurement(
        id,
        request.Location ?? existing.Location,
        request.TemperatureC ?? existing.TemperatureC,
        request.RecordedAt ?? existing.RecordedAt
    );
    var result = await store.UpdateAsync(updated);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("UpdateTemperature");

temperatureGroup.MapDelete("/{id:guid}", async (Guid id, TemperatureMeasurementStore store) =>
{
    var existing = await store.GetByIdAsync(id);
    if (existing is null)
    {
        return Results.NotFound();
    }
    await store.DeleteAsync(id, existing.Location);
    return Results.NoContent();
})
.WithName("DeleteTemperature");

app.Run();

// Models
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Cosmos DB document model - using lowercase property names for Cosmos DB compatibility
class TemperatureMeasurementDocument
{
    public string id { get; set; } = default!;
    
    public string location { get; set; } = default!;
    
    public double temperatureC { get; set; }
    
    public DateTime recordedAt { get; set; }
    
    [JsonIgnore]
    public double TemperatureF => 32 + (temperatureC * 9 / 5);
    
    public TemperatureMeasurement ToRecord() => new(Guid.Parse(id), location, temperatureC, recordedAt);
    
    public static TemperatureMeasurementDocument FromRecord(TemperatureMeasurement record) => new()
    {
        id = record.Id.ToString(),
        location = record.Location,
        temperatureC = record.TemperatureC,
        recordedAt = record.RecordedAt
    };
}

record TemperatureMeasurement(Guid Id, string Location, double TemperatureC, DateTime RecordedAt)
{
    public double TemperatureF => 32 + (TemperatureC * 9 / 5);
}

record CreateTemperatureMeasurement(string Location, double TemperatureC, DateTime? RecordedAt);
record UpdateTemperatureMeasurement(string? Location, double? TemperatureC, DateTime? RecordedAt);

// Cosmos DB-based store
class TemperatureMeasurementStore
{
    private readonly Container _container;
    private readonly ILogger<TemperatureMeasurementStore> _logger;
    private const string DatabaseName = "TemperatureDb";
    private const string ContainerName = "Temperatures";

    public TemperatureMeasurementStore(CosmosClient cosmosClient, ILogger<TemperatureMeasurementStore> logger)
    {
        _container = cosmosClient.GetContainer(DatabaseName, ContainerName);
        _logger = logger;
    }

    public async Task<IEnumerable<TemperatureMeasurement>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.recordedAt DESC");
        var results = new List<TemperatureMeasurement>();
        
        using var iterator = _container.GetItemQueryIterator<TemperatureMeasurementDocument>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Select(doc => doc.ToRecord()));
        }
        
        return results;
    }

    public async Task<TemperatureMeasurement?> GetByIdAsync(Guid id)
    {
        // Since we don't know the partition key (location), we need to query
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id.ToString());
        
        using var iterator = _container.GetItemQueryIterator<TemperatureMeasurementDocument>(query);
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var doc = response.FirstOrDefault();
            return doc?.ToRecord();
        }
        
        return null;
    }

    public async Task<TemperatureMeasurement> AddAsync(TemperatureMeasurement measurement)
    {
        var document = TemperatureMeasurementDocument.FromRecord(measurement);
        var response = await _container.CreateItemAsync(document, new PartitionKey(document.location));
        _logger.LogInformation("Created temperature measurement {Id} in Cosmos DB", measurement.Id);
        return response.Resource.ToRecord();
    }

    public async Task<TemperatureMeasurement?> UpdateAsync(TemperatureMeasurement measurement)
    {
        var document = TemperatureMeasurementDocument.FromRecord(measurement);
        try
        {
            var response = await _container.ReplaceItemAsync(
                document, 
                document.id, 
                new PartitionKey(document.location));
            _logger.LogInformation("Updated temperature measurement {Id} in Cosmos DB", measurement.Id);
            return response.Resource.ToRecord();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, string location)
    {
        try
        {
            await _container.DeleteItemAsync<TemperatureMeasurementDocument>(
                id.ToString(), 
                new PartitionKey(location));
            _logger.LogInformation("Deleted temperature measurement {Id} from Cosmos DB", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
