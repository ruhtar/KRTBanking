using System.Text.Json;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Entities;
using KRTBank.Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace KRTBank.Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly TimeSpan _ttl;

    public CacheService(IOptions<RedisOptions> redisOptions)
    {
        var optionsValue = redisOptions.Value;

        var options = new ConfigurationOptions
        {
            EndPoints = { optionsValue.Endpoint }
        };

        var redis = ConnectionMultiplexer.Connect(options);
        _database = redis.GetDatabase();

        _ttl = TimeSpan.FromDays(optionsValue.TtlInDays);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, _ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}