using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using TheatricalPlayersRefactoringKata.Api.Contracts;
using TheatricalPlayersRefactoringKata.Api.Processing;

namespace TheatricalPlayersRefactoringKata.Api.Tests;

public class StatementsApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _testDirectory = null!;

    public Task InitializeAsync()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "theatrical-api-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);

        var dbPath = Path.Combine(_testDirectory, "test.db");
        var outputDirectory = Path.Combine(_testDirectory, "statements-output");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("StatementOutput:Directory", outputDirectory);
            builder.UseSetting("ConnectionStrings:TheatricalPlayers", $"Data Source={dbPath};Cache=Shared");
            builder.UseSetting("Serilog:FilePath", Path.Combine(_testDirectory, "logs", "api-.log"));
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();

        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // SQLite may briefly lock the file on Windows during disposal.
            }
        }
    }

    [Fact]
    public async Task PostStatements_WithValidPayload_ReturnsAcceptedJob()
    {
        var response = await _client.PostAsJsonAsync("/api/statements", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var job = await response.Content.ReadFromJsonAsync<StatementJobResponse>(JsonOptions);
        Assert.NotNull(job);
        Assert.NotEqual(Guid.Empty, job.Id);
        Assert.Equal("BigCo", job.Customer);
        Assert.NotEmpty(job.Plays);
        Assert.True(job.Status is StatementJobStatus.Queued or StatementJobStatus.Processing or StatementJobStatus.Completed);
    }

    [Fact]
    public async Task PostStatements_WithUnknownPlayId_ReturnsBadRequest()
    {
        var request = CreateValidRequest();
        request.Performances.Add(new PerformanceRequest { PlayId = "missing-play", Audience = 10 });

        var response = await _client.PostAsJsonAsync("/api/statements", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStatement_WhenJobDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/statements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StatementJob_IsPersistedProcessedAndXmlCanBeDownloaded()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/statements", CreateValidRequest());
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<StatementJobResponse>(JsonOptions);
        Assert.NotNull(created);

        var completed = await WaitForCompletionAsync(created.Id);
        Assert.Equal(StatementJobStatus.Completed, completed.Status);
        Assert.Equal("BigCo", completed.Customer);
        Assert.Equal(399540, completed.TotalAmountInCents);
        Assert.Equal(56, completed.TotalCredits);
        Assert.NotEmpty(completed.Plays);
        Assert.Equal(6, completed.Lines.Count);
        Assert.False(string.IsNullOrWhiteSpace(completed.OutputFilePath));
        Assert.True(File.Exists(completed.OutputFilePath));

        var xmlResponse = await _client.GetAsync($"/api/statements/{created.Id}/xml");
        Assert.Equal(HttpStatusCode.OK, xmlResponse.StatusCode);
        Assert.Equal("application/xml", xmlResponse.Content.Headers.ContentType?.MediaType);

        var xml = await xmlResponse.Content.ReadAsStringAsync();
        Assert.Contains("<Customer>BigCo</Customer>", xml);
        Assert.Contains("<AmountOwed>3995.4</AmountOwed>", xml);
        Assert.Contains("<EarnedCredits>56</EarnedCredits>", xml);
    }

    [Fact]
    public async Task GetStatements_ReturnsPersistedJobs()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/statements", CreateValidRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<StatementJobResponse>(JsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync("/api/statements");
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {body}");

        var list = JsonSerializer.Deserialize<List<StatementJobResponse>>(body, JsonOptions);
        Assert.NotNull(list);
        Assert.Contains(list, job => job.Id == created.Id && job.Customer == "BigCo");
    }

    private async Task<StatementJobResponse> WaitForCompletionAsync(Guid jobId)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var response = await _client.GetAsync($"/api/statements/{jobId}");
            response.EnsureSuccessStatusCode();

            var job = await response.Content.ReadFromJsonAsync<StatementJobResponse>(JsonOptions);
            Assert.NotNull(job);

            if (job.Status is StatementJobStatus.Completed or StatementJobStatus.Failed)
            {
                return job;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"Job {jobId} did not complete in time.");
    }

    private static CreateStatementJobRequest CreateValidRequest() => new()
    {
        Customer = "BigCo",
        Plays = new Dictionary<string, PlayRequest>
        {
            ["hamlet"] = new() { Name = "Hamlet", Lines = 4024, Type = "tragedy" },
            ["as-like"] = new() { Name = "As You Like It", Lines = 2670, Type = "comedy" },
            ["othello"] = new() { Name = "Othello", Lines = 3560, Type = "tragedy" },
            ["henry-v"] = new() { Name = "Henry V", Lines = 3227, Type = "history" },
            ["john"] = new() { Name = "King John", Lines = 2648, Type = "history" },
            ["richard-iii"] = new() { Name = "Richard III", Lines = 3718, Type = "history" }
        },
        Performances =
        [
            new() { PlayId = "hamlet", Audience = 55 },
            new() { PlayId = "as-like", Audience = 35 },
            new() { PlayId = "othello", Audience = 40 },
            new() { PlayId = "henry-v", Audience = 20 },
            new() { PlayId = "john", Audience = 39 },
            new() { PlayId = "henry-v", Audience = 20 }
        ]
    };
}
