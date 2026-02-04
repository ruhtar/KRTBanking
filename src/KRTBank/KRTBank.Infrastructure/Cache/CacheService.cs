using System.Text.Json;
using KRTBank.Application.Interfaces;
using StackExchange.Redis;

namespace KRTBank.Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(1);


    public CacheService(string redisEndpoint)
    {
        var options = new ConfigurationOptions
        {
            EndPoints = { redisEndpoint },
            Ssl = redisEndpoint.Contains("amazonaws") // TODO: fazer isso mesmo? SSL só para AWS
        };

        var redis = ConnectionMultiplexer.Connect(options);
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, Ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}