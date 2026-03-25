// File: NewLoco.Web/Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;                 // ApplicationUser, ApplicationRole
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;     // FuelPoliciesOptions + service contracts
using NewLoco.Service.Core.Services;      // FuelService, LocomotiveService, ShiftWorkService, FuelEstimator
using NewLoco.Web.Auth;                   // Perm, PermissionRequirement, PermissionHandler, AppClaimsPrincipalFactory
using NewLoco.Web.Infrastructure;         // AppSeeder, BootstrapAdminOptions

namespace NewLoco.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- DbContext ---
            var connectionString = builder.Configuration.GetConnectionString("DevConnection")
                ?? throw new InvalidOperationException("Connection string 'DevConnection' not found.");

            builder.Services.AddDbContext<LocoDbContext>(options =>
            {
                options.UseSqlServer(connectionString);

                // DEV-only diagnostics for EF (safe to keep behind env check)
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // --- MVC + Razor Pages (Identity UI needs Razor Pages) ---
            builder.Services
                .AddControllersWithViews()
#if DEBUG
                .AddRazorRuntimeCompilation() // optional: speeds up .cshtml edits in dev
#endif
                ;

            // Allow anonymous ONLY to Identity Login;
            // everything else requires authorization (via FallbackPolicy below)
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
               
                // options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied");
            });

            // --- Identity (custom TUser/TRole + EF stores) ---
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    // Dev-friendly defaults; tune for production
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
                .AddRoles<ApplicationRole>()              // roles enabled
                .AddEntityFrameworkStores<LocoDbContext>() // use EF stores
                .AddDefaultTokenProviders();               // tokens for 2FA/reset, etc.

            // Cookie paths for Identity UI
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";

                // redirect any 403 (forbidden) straight to the public list
                options.AccessDeniedPath = "/PublicLocomotives/Index";
            });

            // Claims factory: materialize role permission-claims into user principal on sign-in
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                                       AppClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();

            // --- Authorization ---
            // Dynamic policy provider + handler (Perm.* built on the fly; checks "permission" claims)
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, ConventionalAuthorizationPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // FallbackPolicy: authenticated + must have at least 1 permission claim.
            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        // IMPORTANT: Perm.ClaimType must be "permission"
                        ctx.User.HasClaim(c => c.Type == Perm.ClaimType)
                    )
                    .Build());

            // --- DI for app services ---
            builder.Services.AddScoped<IFuelService, FuelService>();
            builder.Services.AddScoped<ILocomotiveService, Service.Core.LocomotiveService>();
            builder.Services.AddScoped<IShiftWorkService, ShiftWorkService>();
            builder.Services.AddScoped<IFuelEstimator, FuelEstimator>(); // + default fuel suggestion per hours/type

            // --- Options binding ---
            builder.Services.Configure<BootstrapAdminOptions>(builder.Configuration.GetSection("BootstrapAdmin"));

            // Fail-fast validation for FuelPolicies (bind + validate on start)
            builder.Services
                .AddOptions<FuelPoliciesOptions>()
                .Bind(builder.Configuration.GetSection("FuelPolicies"))
                .Validate(o => o is not null, "FuelPolicies section missing")
                .Validate(o => o.DepotStepLiters > 0, "DepotStepLiters must be > 0")
                .Validate(o => o.PerClassSafety is not null && o.PerClassSafety.Count > 0,
                          "PerClassSafety must contain at least one class")
                // If you want to enforce specific classes, uncomment below:
                //.Validate(o => new[] { "52", "55", "06" }.All(k => o.PerClassSafety.ContainsKey(k)),
                //          "PerClassSafety must define thresholds for classes 52, 55, and 06")
                .ValidateOnStart();

            var app = builder.Build();

            // --- Pipeline ---
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint(); // dev errors + migrations endpoint
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // must be before UseAuthorization
            app.UseAuthorization();

            // --- Idempotent seed: roles/permissions always; admin only if Enabled=true in secrets ---
            using (var scope = app.Services.CreateScope())
            {
                await AppSeeder.SeedAsync(scope.ServiceProvider);
            }

            // --- Routes ---
            // Areas
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Optional: alias so /Admin/Roles/Create maps to RbacController
            app.MapControllerRoute(
                name: "admin-roles-alias",
                pattern: "Admin/Roles/{action=Roles}/{roleName?}",
                defaults: new { area = "Admin", controller = "Rbac" });

            // PUBLIC WHITELIST (visible even for logged-in users without permissions)
            // We attach AllowAnonymous metadata to these specific endpoints to bypass FallbackPolicy.
            app.MapControllerRoute(
                    name: "public-locomotives-index",
                    pattern: "PublicLocomotives/Index",
                    defaults: new { controller = "PublicLocomotives", action = "Index" })
               .WithMetadata(new AllowAnonymousAttribute()); // acts like [AllowAnonymous] on the action

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

            // Default
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // Identity UI

            await app.RunAsync();
        }
    }
}