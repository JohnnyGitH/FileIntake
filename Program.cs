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

namespace FileIntake;

public class Program
{
    public static async Task Main(string[] args)
    {

        // --- STEP 1: Builder Configuration ---
        var builder = WebApplication.CreateBuilder(args);

        // Register the DbContext (Replace ApplicationDbContext and connection string name)
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());

        // Add Identity Services
        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        // .AddRoles<IdentityRole>() // Uncomment when I implement roles

        builder.Services.AddScoped<IFileIntakeService, FileIntakeService>();

        // Add MVC/View Services
        builder.Services.AddControllersWithViews();


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

        app.MapRazorPages(); // Identity UI pages

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var serviceProvider = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                await DbInitializer.Initialize(context, serviceProvider);
            }
            catch (Exception ex)
            {
                // Log errors or handle them as needed
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        app.Run();
    }
}

