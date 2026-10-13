using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Services;

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

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (string.IsNullOrWhiteSpace(Text)) return Page();

        var id = Guid.NewGuid().ToString();

        await _redis.StringSetAsync($"text:{id}", Text);
        await _publisher.PublishAsync(id, Text);

        return RedirectToPage("Summary", new { id });
    }
}