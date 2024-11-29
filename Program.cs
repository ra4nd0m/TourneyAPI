using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TourneyAPI.Data;
using TourneyAPI.Endpoints;
using TourneyAPI.Interfaces;
using TourneyAPI.Services;
using TourneyAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TournamentContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TournamentContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
    options.AddPolicy("AdminOrUser", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.IsInRole("User")));
    options.AddPolicy("TournamentEditor", policy => policy.RequireAssertion(context =>
    context.User.IsInRole("Admin") ||
    context.User.FindFirstValue(ClaimTypes.NameIdentifier) == context.Resource?.ToString()
    ));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<User>>();
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || await userManager.FindByIdAsync(userId) == null)
            {
                context.Fail("Invalid user");
            }
        }
    };
});

builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<ITournamentService, TournamentService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

await AdminInitializer.InitializeAsync(app.Services);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("/index.html");

app.MapGet("/", () => "Hello World!");
app.MapTournamentEndpoints();
app.MapUserEndpoints();

app.Run();
