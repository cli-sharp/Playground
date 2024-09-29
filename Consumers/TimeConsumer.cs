using MassTransit;
using Playground.Contracts;
using StackExchange.Redis;
using Newtonsoft.Json;
using Playground.Contexts;

namespace Playground.Consumers;

public class TimeConsumer : IConsumer<TimeContract>
{
    private readonly ConnectionMultiplexer connectionMultiplexer;

    public TimeConsumer(ConnectionMultiplexer connectionMultiplexer, UserContext context)
    {
        var test = context.Users.Find("Christian");

        this.connectionMultiplexer = connectionMultiplexer;
    }

    public async Task Consume(ConsumeContext<TimeContract> context)
    {
        await connectionMultiplexer.GetDatabase().StringSetAsync(
            "Time", JsonConvert.SerializeObject(DateTime.Now));

        await context.RespondAsync(new TimeContract()
        {
            Time = JsonConvert.DeserializeObject<DateTime>(
                await connectionMultiplexer.GetDatabase().StringGetAsync("Time"))
        });
    }
}