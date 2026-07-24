using Timebash.API.Services;
using Timebash.Application.Services;
using Timebash.Application.Services.Access;
using Timebash.Core.Services.Access;

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
        services.AddScoped<IActivityAccessService, ActivityAccessService>();
        services.AddScoped<IJournalAccessService, JournalAccessService>();
        services.AddScoped<ICategoryAccessService, CategoryAccessService>();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
