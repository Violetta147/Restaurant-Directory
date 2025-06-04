using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.IdentityPolicy;
using PBL3.CustomPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using PBL3.Repositories.Interfaces;
using PBL3.Repositories;
using PBL3.Services.Interfaces;
using PBL3.Services;
using NetTopologySuite;

var builder = WebApplication.CreateBuilder(args);

// ADD SERVICES TO THE CONTAINER
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("PBL3");
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
            sqlOptions.UseNetTopologySuite(); //thêm dòng này để dbcontext hiểu được geometry của NetTopologySuite
        }
    ));
builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// REGISTER REPOSITORY
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

// REGISTER SERVICE
builder.Services.AddScoped<IRestaurantService, RestaurantService>();

// CONFIGURE IDENTITY
builder.Services.Configure<IdentityOptions>(options =>
{


    options.Password.RequiredLength = 8;            // Minimum password length.
    options.Password.RequireLowercase = true;       // Require at least one lowercase ('a'-'z').
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";


    // Lockout settings (optional but recommended)
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 3;
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(10);
});

builder.Services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordPolicy>();
builder.Services.AddTransient<IUserValidator<AppUser>, CustomUsernameEmailPolicy>();
builder.Services.AddTransient<IAuthorizationHandler, AllowUsersHandler>();
builder.Services.AddTransient<IAuthorizationHandler, AllowPrivateHandler>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/LoginModal";
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthentication()
        .AddGoogle(opts =>
        {
            IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
            opts.ClientId = googleAuthNSection["ClientId"];
            opts.ClientSecret = googleAuthNSection["ClientSecret"];
            opts.SignInScheme = IdentityConstants.ExternalScheme;
            opts.CallbackPath = new PathString("/signin-google");
        });
builder.Services.AddAuthorization(opts => {
    opts.AddPolicy("AspManager", policy => {
        policy.RequireRole("Manager");
        policy.RequireClaim("Coding-Skill", "ASP.NET Core MVC");
    });
    opts.AddPolicy("AllowTom", policy => {
        policy.AddRequirements(new AllowUserPolicy("tom"));
    });
    opts.AddPolicy("PrivateAccess", policy =>
    {
        policy.AddRequirements(new AllowPrivatePolicy());
    });
});
builder.Services.AddTransient<PBL3.Ultilities.EmailHelper>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add HttpClient factory for API calls
// builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages();
builder.Services.AddTransient<PBL3.Ultilities.IEmailSender, PBL3.Ultilities.EmailHelper>();
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

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// app.MapRazorPages()
//    .WithStaticAssets();

app.Run();
