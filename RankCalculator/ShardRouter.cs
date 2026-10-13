using StackExchange.Redis;

namespace Valuator.Services;

public class ShardRouter
{
    private readonly IDatabase _main;
    private readonly Dictionary<string, IDatabase> _shards;

    public ShardRouter(IConfiguration config)
    {
        var mainConn = config.GetConnectionString("DB_MAIN")
            ?? throw new InvalidOperationException("DB_MAIN not set");
        _main = ConnectionMultiplexer.Connect(mainConn).GetDatabase();

        _shards = new Dictionary<string, IDatabase>
        {
            ["RU"]   = ConnectionMultiplexer.Connect(config.GetConnectionString("DB_RU")!).GetDatabase(),
            ["EU"]   = ConnectionMultiplexer.Connect(config.GetConnectionString("DB_EU")!).GetDatabase(),
            ["ASIA"] = ConnectionMultiplexer.Connect(config.GetConnectionString("DB_ASIA")!).GetDatabase(),
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