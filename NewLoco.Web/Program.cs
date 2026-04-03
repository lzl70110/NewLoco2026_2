using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;
using NewLoco.Service.Core.Services;
using NewLoco.Web.Auth;
using NewLoco.Web.Infrastructure;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace NewLoco.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------------------------------
            // 🔥 FIX: ASP.NET Core must use dot (.) as decimal separator
            // -------------------------------------------------------
            var culture = new CultureInfo("en-US");
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = "";

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture);
                options.SupportedCultures = new[] { culture };
                options.SupportedUICultures = new[] { culture };
            });
            // -------------------------------------------------------

            // --- DbContext ---
            var connectionString = builder.Configuration.GetConnectionString("DevConnection")
                ?? throw new InvalidOperationException("Connection string 'DevConnection' not found.");

            builder.Services.AddDbContext<LocoDbContext>(options =>
            {
                options.UseSqlServer(connectionString);

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services
                .AddControllersWithViews()
#if DEBUG
                .AddRazorRuntimeCompilation()
#endif
                ;

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
            });

            // --- Identity ---
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
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
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<LocoDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Home/Forbidden";
            });

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                                       AppClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, ConventionalAuthorizationPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        ctx.User.HasClaim(c => c.Type == Perm.ClaimType))
                    .Build());

            builder.Services.AddScoped<IFuelService, FuelService>();
            builder.Services.AddScoped<ILocomotiveService, LocomotiveService>();
            builder.Services.AddScoped<IShiftWorkService, ShiftWorkService>();
            builder.Services.AddScoped<IFuelEstimator, FuelEstimator>();

            builder.Services.Configure<BootstrapAdminOptions>(builder.Configuration.GetSection("BootstrapAdmin"));

            builder.Services
                .AddOptions<FuelPoliciesOptions>()
                .Bind(builder.Configuration.GetSection("FuelPolicies"))
                .Validate(o => o is not null, "FuelPolicies section missing")
                .Validate(o => o.DepotStepLiters > 0, "DepotStepLiters must be > 0")
                .Validate(o => o.PerClassSafety is not null && o.PerClassSafety.Count > 0,
                          "PerClassSafety must contain at least one class")
                .ValidateOnStart();

            builder.Services.AddScoped<IAxleMeasurementService, AxleMeasurementService>();

            var app = builder.Build();

            // -------------------------------------------------------
            // 🔥 apply en-US culture to the pipeline
            // -------------------------------------------------------
            app.UseRequestLocalization();
            // -------------------------------------------------------

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePages(async context =>
            {
                var statusCode = context.HttpContext.Response.StatusCode;

                if (statusCode == 401)
                {
                    context.HttpContext.Response.Redirect("/Home/UnauthorizedPage");
                }
                else if (statusCode == 403)
                {
                    context.HttpContext.Response.Redirect("/Home/Forbidden");
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                await AppSeeder.SeedAsync(scope.ServiceProvider);
            }

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "admin-roles-alias",
                pattern: "Admin/Roles/{action=Roles}/{roleName?}",
                defaults: new { area = "Admin", controller = "Rbac" });

            app.MapControllerRoute(
                    name: "public-locomotives-index",
                    pattern: "PublicLocomotives/Index",
                    defaults: new { controller = "PublicLocomotives", action = "Index" })
               .WithMetadata(new AllowAnonymousAttribute());

            app.MapControllerRoute(
                    name: "calendar-index",
                    pattern: "Calendar/Index",
                    defaults: new { controller = "Calendar", action = "Index" })
               .WithMetadata(new AllowAnonymousAttribute());

            app.MapControllerRoute(
                    name: "calculator-index",
                    pattern: "Calculator/Index",
                    defaults: new { controller = "Calculator", action = "Index" })
               .WithMetadata(new AllowAnonymousAttribute());

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}