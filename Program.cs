using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.NetCore.Data;
using WebApi.NetCore.Data.Database;
using WebApi.NetCore.Middleware;
using WebApi.NetCore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret)
    || Encoding.UTF8.GetByteCount(jwtSettings.Secret) < 32
    || jwtSettings.Secret.Contains("replace-with", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT Secret must be configured and at least 32 bytes.");
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddApplicationServices();
builder.Services.AddScoped<DatabaseInitializer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
    var allowStartupWithoutInitialization = builder.Configuration.GetValue<bool>("Database:AllowStartupWithoutInitialization");

    try
    {
        await initializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        if (allowStartupWithoutInitialization)
        {
            logger.LogWarning(ex, "Database initialization failed and startup fallback is enabled.");
        }
        else
        {
            throw;
        }
    }
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("ApiCors");
app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
