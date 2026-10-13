using StackExchange.Redis;
using Valuator.Services;        
using Microsoft.AspNetCore.Authentication.Cookies;

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

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/Login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });

        builder.Services.AddAuthorization();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication(); 

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
