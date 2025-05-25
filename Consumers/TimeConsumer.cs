namespace Playground.Consumers;

using Microsoft.Extensions.Caching.Hybrid;

public class TimeConsumer : IConsumer<TimeContract>
{
    private readonly ConnectionMultiplexer connectionMultiplexer;
    private readonly HybridCache hybridCache;

    public TimeConsumer(ConnectionMultiplexer connectionMultiplexer, UserContext context, HybridCache hybridCache)
    {
        var test = context.Users.Find("Christian");

        this.connectionMultiplexer = connectionMultiplexer;

        this.hybridCache = hybridCache;
    }

    public async Task Consume(ConsumeContext<TimeContract> context)
    {
        await hybridCache.SetAsync("Time2", DateTime.Now);

        await context.RespondAsync(new TimeContract()
        {
            Time = await hybridCache.GetOrCreateAsync("Time2", cancellationToken => ValueTask.FromResult(DateTime.Now.AddYears(-1))),
        });
    }
}