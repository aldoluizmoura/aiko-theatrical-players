using System.Threading.Channels;

namespace TheatricalPlayersRefactoringKata.Api.Processing;

public interface IStatementJobQueue
{
    ValueTask EnqueueAsync(StatementJob job, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StatementJob> ReadAllAsync(CancellationToken cancellationToken);
}

public class StatementJobQueue : IStatementJobQueue
{
    private readonly Channel<StatementJob> _channel = Channel.CreateUnbounded<StatementJob>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(StatementJob job, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(job, cancellationToken);

    public IAsyncEnumerable<StatementJob> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
