using StackExchange.Redis;

namespace Valuator.Services;

public class ShardRouter
{
    private readonly IDatabase _main;
    private readonly Dictionary<string, IDatabase> _shards;

    public ShardRouter(IConfiguration config)
    {
       var mainConn = Environment.GetEnvironmentVariable("DB_MAIN") ?? "localhost:6000";
        _main = ConnectionMultiplexer.Connect(mainConn).GetDatabase();

        _shards = new Dictionary<string, IDatabase>
        {
            ["RU"]   = ConnectionMultiplexer.Connect(config["DB_RU"]   ?? "localhost:6001").GetDatabase(),
            ["EU"]   = ConnectionMultiplexer.Connect(config["DB_EU"]   ?? "localhost:6002").GetDatabase(),
            ["ASIA"] = ConnectionMultiplexer.Connect(config["DB_ASIA"] ?? "localhost:6003").GetDatabase(),
        };
    }

    public async Task SaveShardMapAsync(string id, string region)
        => await _main.StringSetAsync($"shard:{id}", region);

    /// <summary>
    /// Находит регион для id и логирует LOOKUP: {id}, {region}
    /// </summary>
    public async Task<string?> LookupRegionAsync(string id)
    {
        var value = await _main.StringGetAsync($"shard:{id}");
        if (value.IsNull) return null;

        var region = value.ToString();
        Console.WriteLine($"LOOKUP: {id}, {region}");
        return region;
    }

    public IDatabase GetShard(string region) => _shards[region];
}