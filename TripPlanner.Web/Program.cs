using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.Implementation;
using TripPlanner.Infrastructure.Database;
using TripPlanner.Infrastructure.Identity;

namespace TripPlanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration.GetConnectionString("MySQL")
                ?? throw new InvalidOperationException("Connection string 'MySQL' was not found.");
            ServerVersion serverVersion = new MySqlServerVersion("8.0.41");

            builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder => optionsBuilder.UseMySql(connectionString, serverVersion));

            builder.Services
                .AddDefaultIdentity<User>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 1;

                    options.User.RequireUniqueEmail = true;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Security/Account/Login";
                options.LogoutPath = "/Security/Account/Logout";
                options.SlidingExpiration = true;
            });

            builder.Services.AddHttpClient<IGoogleAPIService, GoogleAPIService>();

            builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["OpenMeteo:BaseUrl"]);
            });

            builder.Services.AddScoped<IAccountIdentityService, AccountIdentityService>();
            builder.Services.AddScoped<ITripService, TripService>();
            builder.Services.AddScoped<ITripDetailService, TripDetailService>();
            builder.Services.AddScoped<IAccomodationService, AccomodationService>();
            builder.Services.AddScoped<IPlaceService, PlaceService>();
            builder.Services.AddScoped<ICollaboratorService, CollaboratorService>();
            builder.Services.AddScoped<IBudgetService, BudgetService>();
            builder.Services.AddScoped<IChecklistService, ChecklistService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
