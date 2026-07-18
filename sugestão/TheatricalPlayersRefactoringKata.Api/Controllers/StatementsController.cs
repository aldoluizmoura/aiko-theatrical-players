using Microsoft.AspNetCore.Mvc;
using TheatricalPlayersRefactoringKata.Api.Contracts;
using TheatricalPlayersRefactoringKata.Api.Processing;

namespace TheatricalPlayersRefactoringKata.Api.Controllers;

[ApiController]
[Route("api/statements")]
public class StatementsController : ControllerBase
{
    private readonly IStatementJobQueue _queue;
    private readonly IStatementJobStore _store;

    public StatementsController(IStatementJobQueue queue, IStatementJobStore store)
    {
        _queue = queue;
        _store = store;
    }

    /// <summary>
    /// Enqueues a statement generation job. Processing happens asynchronously and writes an XML file to disk.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StatementJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStatementJobRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Customer))
        {
            return BadRequest("Customer is required.");
        }

        if (request.Plays is null || request.Plays.Count == 0)
        {
            return BadRequest("At least one play is required.");
        }

        if (request.Performances is null || request.Performances.Count == 0)
        {
            return BadRequest("At least one performance is required.");
        }

        var plays = request.Plays.ToDictionary(
            pair => pair.Key,
            pair => new Play(pair.Value.Name, pair.Value.Lines, pair.Value.Type));

        foreach (var performance in request.Performances)
        {
            if (!plays.ContainsKey(performance.PlayId))
            {
                return BadRequest($"Unknown playId: {performance.PlayId}");
            }
        }

        var invoice = new Invoice(
            request.Customer,
            request.Performances
                .Select(p => new Performance(p.PlayId, p.Audience))
                .ToList());

        var job = new StatementJob
        {
            Invoice = invoice,
            Plays = plays
        };

        _store.Add(job);
        await _queue.EnqueueAsync(job, cancellationToken);

        return AcceptedAtAction(nameof(GetById), new { id = job.Id }, StatementJobResponse.FromJob(job));
    }

    /// <summary>
    /// Lists all statement jobs known to this process.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StatementJobResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<StatementJobResponse>> GetAll()
        => Ok(_store.GetAll().Select(StatementJobResponse.FromJob));

    /// <summary>
    /// Gets the current status of a statement job.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StatementJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<StatementJobResponse> GetById(Guid id)
    {
        var job = _store.Get(id);
        if (job is null)
        {
            return NotFound();
        }

        return Ok(StatementJobResponse.FromJob(job));
    }

    /// <summary>
    /// Downloads the generated XML when the job is completed.
    /// </summary>
    [HttpGet("{id:guid}/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DownloadXml(Guid id, CancellationToken cancellationToken)
    {
        var job = _store.Get(id);
        if (job is null)
        {
            return NotFound();
        }

        if (job.Status != StatementJobStatus.Completed || string.IsNullOrWhiteSpace(job.OutputFilePath))
        {
            return Conflict(new { message = $"Job is {job.Status}. XML is not available yet." });
        }

        if (!System.IO.File.Exists(job.OutputFilePath))
        {
            return NotFound(new { message = "XML file was not found on disk." });
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(job.OutputFilePath, cancellationToken);
        return File(bytes, "application/xml", Path.GetFileName(job.OutputFilePath));
    }
}
