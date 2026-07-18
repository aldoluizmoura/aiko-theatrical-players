using System.Collections.Concurrent;

namespace TheatricalPlayersRefactoringKata.Api.Processing;

public interface IStatementJobStore
{
    void Add(StatementJob job);
    StatementJob? Get(Guid id);
    IReadOnlyCollection<StatementJob> GetAll();
    void Update(StatementJob job);
}

public class InMemoryStatementJobStore : IStatementJobStore
{
    private readonly ConcurrentDictionary<Guid, StatementJob> _jobs = new();

    public void Add(StatementJob job) => _jobs[job.Id] = job;

    public StatementJob? Get(Guid id)
        => _jobs.TryGetValue(id, out var job) ? job : null;

    public IReadOnlyCollection<StatementJob> GetAll()
        => _jobs.Values.OrderByDescending(j => j.CreatedAt).ToArray();

    public void Update(StatementJob job) => _jobs[job.Id] = job;
}
