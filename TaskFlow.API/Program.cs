using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using TaskFlow.API.Data;
using TaskFlow.API.Hubs;
using TaskFlow.API.Jobs;
using TaskFlow.API.Services;

// ============ SERILOG - Structured logging ============
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // ============ DATABASE - Entity Framework Core ============
    // Default: SQLite (taskflow.db) - no setup required, runs anywhere
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=taskflow.db";

    var useSqlite = connectionString.Contains("Data Source=") || connectionString.EndsWith(".db");

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        if (useSqlite)
            options.UseSqlite(connectionString);
        else if (connectionString.Contains("Host="))
            options.UseNpgsql(connectionString);
        else
            options.UseSqlServer(connectionString);
    });

    // ============ JWT AUTHENTICATION ============
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskFlow",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TaskFlow",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // ============ CACHING - Redis or In-Memory ============
    var redisConnection = builder.Configuration["Redis:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "TaskFlow_";
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }

    // ============ HANGFIRE - Background jobs ============
    builder.Services.AddHangfire(config =>
    {
        if (useSqlite)
            config.UseMemoryStorage();
        else if (connectionString.Contains("Host="))
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
        else
            config.UseSqlServerStorage(connectionString);
    });
    builder.Services.AddHangfireServer();

    // ============ APPLICATION SERVICES ============
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped<ITaskNotificationService, TaskNotificationService>();

    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSignalR();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHangfireDashboard("/hangfire");

    var webRoot = Path.Combine(builder.Environment.ContentRootPath, "..", "taskflow-frontend", "dist");
    if (Directory.Exists(webRoot))
    {
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot) });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot) });
        app.MapFallback(async context =>
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(Path.Combine(webRoot, "index.html"));
        });
    }

    RecurringJob.AddOrUpdate<DailyReportJob>(
        "daily-productivity-report",
        job => job.ExecuteAsync(),
        Cron.Daily(9));

    // Initialize database - apply migrations and seed demo data
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await db.Database.MigrateAsync();
            await DataSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database init failed - ensure connection string is correct");
        }
    }

    Log.Information("TaskFlow API started at http://localhost:5000");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
