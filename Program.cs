using EthioHomes.services.EthioHomes.Services;
using EthioHomes.Services;
using Microsoft.AspNetCore.Session;

namespace EthioHomes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            ConfigureServices(builder.Services);

            builder.Configuration.AddJsonFile("appsettings.json");



            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add controllers with views
            services.AddControllersWithViews();

            services.AddTransient<EmailService>();  //adding email service /Registering it

            services.AddHostedService<RentalReminderService>();   //reminder for renter to pay on time



            // Add session services
            services.AddDistributedMemoryCache(); // In-memory cache for session data
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true; // Enhance security by making cookies HttpOnly
                options.Cookie.IsEssential = true; // Ensure cookies are essential for the application
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Error handling for production
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // Enforce HTTPS for secure connections
            }

            // Enable middleware
            app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
            app.UseStaticFiles();      // Serve static files (e.g., CSS, JS, images)
            app.UseSession();          // Enable session management
            app.UseRouting();          // Route requests to the appropriate controller/action
            app.UseAuthorization();    // Enable authorization middleware (if applicable)

            // Configure the default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
