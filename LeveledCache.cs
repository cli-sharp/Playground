namespace Playground;

public class LeveledCache(IMemoryCache memory, ConnectionMultiplexer redis)
{
    public async Task SetAsync(string key, string value, TimeSpan expiration)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(key, nameof(value));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiration, TimeSpan.Zero, nameof(expiration));

        memory.Set(key, value, expiration);
        await redis.GetDatabase().StringSetAsync(key, value, expiration);
    }

    public async Task<string?> GetAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        if (memory.Get<string?>(key) is var memoryValue &&
            memoryValue is not null)
            return memoryValue;

        if (await redis.GetDatabase().StringGetWithExpiryAsync(key) is var redisEntry &&
            redisEntry.Value.HasValue &&
            redisEntry.Expiry > TimeSpan.Zero)
        {
            memory.Set(key, redisEntry.Value, redisEntry.Expiry.Value);
            return redisEntry.Value;
        }

        return null;
    }

    public async Task DeleteAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        memory.Remove(key);
        await redis.GetDatabase().KeyDeleteAsync(key);
    }
}