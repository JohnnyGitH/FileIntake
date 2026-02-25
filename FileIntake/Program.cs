using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FileIntake.Data;
using System;
using Microsoft.Extensions.Logging;
using FileIntake.Services;
using FileIntake.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using FileIntake.Models.Configuration;
using Microsoft.Extensions.Options;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;

namespace FileIntake;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        var isDev = builder.Environment.IsDevelopment();

        // Register the DbContext (Replace ApplicationDbContext and connection string name)
        if(isDev)
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());
        }
        else
        {
            // Cloud Run: use writable /tmp
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=/tmp/fileintake.db"));
        }

        // Identity Services
        builder.Services.AddDefaultIdentity<IdentityUser>(options => 
                {
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = false;
                }  
            )
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddScoped<IFileIntakeService, FileIntakeService>();
        builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
        builder.Services.AddHttpClient<IAiProcessingService, AiProcessingService>((sp,client) =>
        {
            var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;

            var baseUrl = options.BaseUrl?.TrimEnd('/');
            if(string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("AiService: BaseUrl is not configured");
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");})
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetAiRetryPolicy()) // Retry transient failures (5xx, 408, network errors, 429)
                .AddPolicyHandler(GetAiCircuitBreakerPolicy()); // Stop when there is a failing downstream

        builder.Services.AddControllersWithViews();

        // --- DataProtectionKeys (Cloud Run-safe) ---
        var keysPath = isDev ? "/keys" : "/tmp/keys";

        Directory.CreateDirectory(keysPath);

        // Add Explicit DataProtection config
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("FileIntakeApp");

        builder.Services.Configure<AiServiceOptions>(builder.Configuration.GetSection("AiService"));

        var aiBaseUrl = builder.Configuration["AiService:BaseUrl"];
        Console.WriteLine($"[Startup] AI Service BaseUrl resolved to: {aiBaseUrl}");

        var app = builder.Build();

        // Configure the HTTP request pipeline (Middleware).
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        // Identity Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        // --- DB migrate + optional reset + optional seed ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                var seedDemo = builder.Configuration.GetValue<bool>("SEED_DEMO_DATA");
                var resetDb = builder.Configuration.GetValue<bool>("RESET_DB_ON_START");

                // Optional: reset for demo runs in Cloud Run
                if (!isDev && resetDb)
                {
                    await context.Database.EnsureDeletedAsync();
                }

                var isSqlite = context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

                if (isSqlite)
                {
                    // Cloud Run
                    await context.Database.EnsureCreatedAsync();
                }
                else
                {
                    // Local development
                    await context.Database.MigrateAsync();
                }

                // Seed in dev OR when explicitly enabled
                if (isDev || seedDemo)
                {
                    await DbInitializer.Initialize(context, services);
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        app.Run();
    }

    static IAsyncPolicy<HttpResponseMessage> GetAiRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode == 429)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2,attempt))
            );
    }

    static IAsyncPolicy<HttpResponseMessage> GetAiCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode == 429)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }
}

