using Microsoft.AspNetCore.Mvc;
using TheatricalPlayersRefactoringKata.Api.Contracts;
using TheatricalPlayersRefactoringKata.Api.Persistence;
using TheatricalPlayersRefactoringKata.Api.Processing;

namespace TheatricalPlayersRefactoringKata.Api.Controllers;

[ApiController]
[Route("api/statements")]
public class StatementsController : ControllerBase
{
    private readonly IStatementJobQueue _queue;
    private readonly IStatementJobStore _store;
    private readonly ILogger<StatementsController> _logger;

    public StatementsController(
        IStatementJobQueue queue,
        IStatementJobStore store,
        ILogger<StatementsController> logger)
    {
        _queue = queue;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Enqueues a statement generation job. Processing happens asynchronously, persists the statement and writes XML to disk.
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

        await _store.AddAsync(job, cancellationToken);
        await _queue.EnqueueAsync(job, cancellationToken);

        _logger.LogInformation(
            "Enqueued statement job {JobId} for customer {Customer} with {PerformanceCount} performances",
            job.Id,
            job.Invoice.Customer,
            job.Invoice.Performances.Count);

        return AcceptedAtAction(nameof(GetById), new { id = job.Id }, StatementJobResponse.FromJob(job));
    }

    /// <summary>
    /// Lists persisted statement jobs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StatementJobResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StatementJobResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var jobs = await _store.GetAllAsync(cancellationToken);
        return Ok(jobs.Select(StatementJobResponse.FromJob));
    }

    /// <summary>
    /// Gets a persisted statement job, including plays and calculated lines when available.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StatementJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatementJobResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await _store.GetAsync(id, cancellationToken);
        if (job is null)
        {
            _logger.LogDebug("Statement job {JobId} was not found", id);
            return NotFound();
        }

        return Ok(StatementJobResponse.FromJob(job));
    }

    /// <summary>
    /// Downloads the generated XML when the job is completed (from disk or persisted content).
    /// </summary>
    [HttpGet("{id:guid}/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DownloadXml(Guid id, CancellationToken cancellationToken)
    {
        var job = await _store.GetAsync(id, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        if (job.Status != StatementJobStatus.Completed)
        {
            return Conflict(new { message = $"Job is {job.Status}. XML is not available yet." });
        }

        if (!string.IsNullOrWhiteSpace(job.OutputFilePath) && System.IO.File.Exists(job.OutputFilePath))
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(job.OutputFilePath, cancellationToken);
            return File(bytes, "application/xml", Path.GetFileName(job.OutputFilePath));
        }

        if (!string.IsNullOrWhiteSpace(job.XmlContent))
        {
            _logger.LogWarning("XML file missing for job {JobId}; serving persisted XmlContent instead", id);
            return File(
                System.Text.Encoding.UTF8.GetBytes(job.XmlContent),
                "application/xml",
                $"{id}.xml");
        }

        return NotFound(new { message = "XML content was not found." });
    }
}
