using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Timebash.API.Extensions;
using Timebash.API.Middleware;
using Timebash.Application;
using Timebash.Core.Entities;
using Timebash.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: LogEventLevel.Information);

var seqUrl = builder.Configuration["Seq:ServerUrl"];
if (!string.IsNullOrEmpty(seqUrl))
{
    var minLevel = builder.Configuration.GetValue<LogEventLevel?>("Seq:MinimumLevel") ?? LogEventLevel.Error;
    loggerConfig.WriteTo.Seq(seqUrl, restrictedToMinimumLevel: minLevel);
}

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();

try
{
    Log.Information("Starting Timebash API");

    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddValidatorsFromAssemblyContaining<IApplicationMarker>();
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("LoginPolicy", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 7,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                }
            )
        );

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = "Too many requests. Please try again later.",
                Instance = context.HttpContext.Request.Path
            };

            await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        };
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5017",
                "https://localhost:7174"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    builder.Services.AddSwagger(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TimebashDbContext>();
        await db.Database.MigrateAsync();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
