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
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<TokenService>();
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

builder.Services.AddAuthorization();

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

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DbSeeder.SeedAsync(app.Services);

app.Run();
