using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Globalization;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public string InputText { get; set; }
    public double Rank { get; set; }
    public int Similarity { get; set; }   // было double, исправлено на int

    public async Task OnGet(string id)
    {
        _logger.LogDebug(id);
        var db = _redis.GetDatabase();

        string textKey = $"TEXT-{id}";
        string rankKey = $"RANK-{id}";
        string similarityKey = $"SIMILARITY-{id}";

        InputText = await db.StringGetAsync(textKey);
        var rankValue = await db.StringGetAsync(rankKey);
        var similarityValue = await db.StringGetAsync(similarityKey);

        if (rankValue.HasValue)
            Rank = double.Parse(rankValue.ToString(), CultureInfo.InvariantCulture);
        if (similarityValue.HasValue)
            Similarity = int.Parse(similarityValue.ToString());
    }
}