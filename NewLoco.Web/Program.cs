using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;
using NewLoco.Service.Core.Services;

namespace NewLoco.Web // препоръчително, вместо .Data
    {
    public class Program
        {
        public static void Main(string[] args)
            {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext
            var connectionString = builder.Configuration.GetConnectionString("DevConnection")
                ?? throw new InvalidOperationException("Connection string 'DevConnection' not found.");

            builder.Services.AddDbContext<LocoDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // MVC
            builder.Services.AddControllersWithViews();

            // Identity (всички логнати = "админ" за сега)
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 4;
                    options.Password.RequiredUniqueChars = 0;

                    options.User.RequireUniqueEmail = true;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                    options.Lockout.MaxFailedAccessAttempts = 255;
                })
                .AddEntityFrameworkStores<LocoDbContext>();

            // (Опционално) фиксиране на пътищата на cookie-то
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // Authorization – без роли/политики засега
            builder.Services.AddAuthorization();

            // DI за нашите услуги
            builder.Services.AddScoped<IFuelService, FuelService>();
            builder.Services.AddScoped<ILocomotiveService, LocomotiveService>();

            var app = builder.Build();

            // Pipeline
            if (app.Environment.IsDevelopment())
                {
                app.UseMigrationsEndPoint();
                }
            else
                {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Routes
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
            }
        }
    }