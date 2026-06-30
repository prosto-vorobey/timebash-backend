using Microsoft.EntityFrameworkCore;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Infrastructure;
using Timebash.Infrastructure.Repositories;
using Timebash.Infrastructure.Services;

namespace Timebash.API.Extensions;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TimebashDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IActivityRepository, PostgresActivityRepository>();
        services.AddScoped<ICategoryRepository, PostgresCategoryRepository>();
        services.AddScoped<IJournalRepository, PostgresJournalRepository>();
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IUserSettingsRepository, PostgresUserSettingsRepository>();
        services.AddScoped<IActivityQueryService, PostgresActivityQueryService>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TimebashDbContext>());

        return services;
    }
}