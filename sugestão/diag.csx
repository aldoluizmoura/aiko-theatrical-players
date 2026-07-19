using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using TheatricalPlayersRefactoringKata.Api.Contracts;

var dir = Path.Combine(Path.GetTempPath(), "diag-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(dir);
var db = Path.Combine(dir, "t.db");
await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
{
    b.UseSetting("StatementOutput:Directory", Path.Combine(dir, "out"));
    b.UseSetting("ConnectionStrings:TheatricalPlayers", $"Data Source={db};Cache=Shared");
    b.UseSetting("Serilog:FilePath", Path.Combine(dir, "logs", "api-.log"));
});
var client = factory.CreateClient();
var req = new CreateStatementJobRequest {
  Customer = "BigCo",
  Plays = new Dictionary<string, PlayRequest> { ["hamlet"] = new() { Name = "Hamlet", Lines = 4024, Type = "tragedy" } },
  Performances = [ new() { PlayId = "hamlet", Audience = 55 } ]
};
var post = await client.PostAsJsonAsync("/api/statements", req);
Console.WriteLine("POST " + (int)post.StatusCode + " " + await post.Content.ReadAsStringAsync());
var get = await client.GetAsync("/api/statements");
Console.WriteLine("GET " + (int)get.StatusCode + " " + await get.Content.ReadAsStringAsync());
