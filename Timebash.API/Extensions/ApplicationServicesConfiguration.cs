using Timebash.API.Services;
using Timebash.Application.Services;

namespace Timebash.API.Extensions;

public static class ApplicationServicesConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMeService, MeService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
