using StackExchange.Redis;
using Valuator.Services;        

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = builder.Configuration["REDIS_CONNECTION"] ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(config);
        });

        builder.Services.AddSingleton<RabbitPublisher>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}