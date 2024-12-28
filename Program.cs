using Microsoft.AspNetCore.Session;

namespace EthioHomes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add session services
            builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for session data
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout (optional)
                options.Cookie.HttpOnly = true; // Make session cookie HttpOnly for security
                options.Cookie.IsEssential = true; // Mark cookie as essential for the app
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
           
            app.UseStaticFiles(); //for using static files located inside WWWroot folder

            // Enable session middleware

            builder.Services.AddSession();

            app.UseSession();

           

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
