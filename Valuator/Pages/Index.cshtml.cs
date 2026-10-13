using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Services;
using Microsoft.AspNetCore.Authorization;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;
    private readonly RabbitPublisher _publisher;

    public IndexModel(
        ILogger<IndexModel> logger,
        IConnectionMultiplexer redis,
        RabbitPublisher publisher)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
        _publisher = publisher;
    }

    [BindProperty(Name = "text")]
    public string Text { get; set; } = string.Empty;

    [BindProperty(Name = "country")]
    public string Country { get; set; } = "Russia";

    private static readonly Dictionary<string, string> CountryToRegion = new()
    {
        ["Russia"]  = "RU",
        ["France"]  = "EU",
        ["Germany"] = "EU",
        ["UAE"]     = "ASIA",
        ["India"]   = "ASIA",
    };

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (string.IsNullOrWhiteSpace(Text)) return Page();

        var id = Guid.NewGuid().ToString();
        var region = CountryToRegion.GetValueOrDefault(Country, "RU");

        await _shards.SaveShardMapAsync(id, region);

        var shardDb = _shards.GetShard(region);
        await shardDb.StringSetAsync($"text:{id}", Text);

        double similarity = ComputeSimilarity(Text);
        await shardDb.StringSetAsync($"similarity:{id}", similarity);

        await _publisher.PublishSimilarityCalculatedAsync(id, similarity);

        await _publisher.PublishAsync(id);

        return RedirectToPage("Summary", new { id });
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
    if (!ModelState.IsValid) return Page();
    if (string.IsNullOrWhiteSpace(Text)) return Page();

    var id = Guid.NewGuid().ToString();

    await _redis.StringSetAsync($"text:{id}", Text);

    double similarity = ComputeSimilarity(Text);
    await _redis.StringSetAsync($"similarity:{id}", similarity);

    await _publisher.PublishSimilarityCalculatedAsync(id, similarity);

    await _publisher.PublishAsync(id, Text);

    return RedirectToPage("Summary", new { id });
    }
    
}
