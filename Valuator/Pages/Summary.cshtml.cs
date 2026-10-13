using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public string RankText { get; set; } = "";
    public string SimilarityText { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(string id)
    {
    if (string.IsNullOrEmpty(id)) return Page();

    var region = await _shards.LookupRegionAsync(id);
    if (region is null)
    {
        RankText = "Оценка содержания не завершена";
        SimilarityText = "Оценка схожести не завершена";
        return Page();
    }

    var shardDb = _shards.GetShard(region);

    var rank = await shardDb.StringGetAsync($"rank:{id}");
    RankText = rank.IsNull ? "Оценка содержания не завершена" : rank.ToString();

    var sim = await shardDb.StringGetAsync($"similarity:{id}");
    SimilarityText = sim.IsNull ? "Оценка схожести не завершена" : sim.ToString();

    return Page();
    }
}
