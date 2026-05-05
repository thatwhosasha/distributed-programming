using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Globalization;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    // Добавляем IConnectionMultiplexer в конструктор
    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text)
    {
        _logger.LogDebug(text);

        if (string.IsNullOrWhiteSpace(text))
            return Page();

        string id = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();

        // 1. Сохраняем исходный текст по ключу TEXT-{id}
        string textKey = $"TEXT-{id}";
        await db.StringSetAsync(textKey, text);

        // 2. Вычисляем rank = доля неалфавитных символов
        double rank = CalculateRank(text);
        string rankKey = $"RANK-{id}";
        await db.StringSetAsync(rankKey, rank.ToString(CultureInfo.InvariantCulture));

        // 3. Проверяем дубликат: ищем такой же текст во всех записях?
        // По заданию нужно проверить, был ли такой текст ранее среди всех обработанных.
        // Проще всего хранить сам текст как ключ с признаком "exists", но тогда не привязано к id.
        // В текущем шаблоне предполагается, что similarity вычисляется по факту существования такого же текста в Redis.
        // Поэтому заведём отдельное множество или просто ключ по тексту.
        // Сделаем так: если ключ с таким текстом уже существует, значит similarity=1, иначе 0, и затем сохраняем ключ.
        string duplicateKey = $"DUPLICATE-{text}"; // ключ на основе текста, а не id
        bool exists = await db.KeyExistsAsync(duplicateKey);
        int similarity = exists ? 1 : 0;
        if (!exists)
        {
            await db.StringSetAsync(duplicateKey, "1"); // помечаем, что текст уже был
        }

        string similarityKey = $"SIMILARITY-{id}";
        await db.StringSetAsync(similarityKey, similarity.ToString());

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int nonLetterCount = text.Count(c => !char.IsLetter(c));
        return (double)nonLetterCount / text.Length;
    }
}