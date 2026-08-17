using System.Text;
using System.Text.Json.Serialization;
using EmployeeHub.Api.Data;
using EmployeeHub.Api.Models;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<EmployeeHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register our services
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<PositionService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<LeaveRequestService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// JWT authentication
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
});

// CORS for the React dev client
const string CorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

// CORS must run before anything that can short-circuit the request (auth) so that
// preflight OPTIONS requests always get the CORS headers.
app.UseCors(CorsPolicy);

// HTTPS redirection is skipped in Development: the SPA calls the API over HTTP, and a
// redirect on a CORS preflight request is rejected by browsers.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// The integration tests swap in SQLite and build their own schema and fixtures, so the
// SQL Server migrate-and-seed step does not apply to them.
if (!app.Environment.IsEnvironment("Testing"))
{
    await DbSeeder.SeedAsync(app.Services);
}

app.Run();

/// <summary>
/// Exposed so the test project's <c>WebApplicationFactory&lt;Program&gt;</c> can boot the real
/// pipeline — attributes, policies and all — rather than testing services in isolation.
/// </summary>
public partial class Program;
