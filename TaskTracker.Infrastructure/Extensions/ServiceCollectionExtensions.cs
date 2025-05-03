using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Infrastructure.Repositories;
using TaskTracker.Application.Interfaces;
using TaskTracker.Infrastructure.BackgroundServices;

namespace TaskTracker.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddHostedService<OverdueTaskUpdater>();

        return services;
    }
}
