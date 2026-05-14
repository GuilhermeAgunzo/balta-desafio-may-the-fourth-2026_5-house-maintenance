using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Infra.Persistence;
using HouseMaintenance.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseMaintenance.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("housemaintenance")
            ?? "Data Source=housemaintenance.db";

        services.AddDbContext<HouseMaintenanceDbContext>(
            options => options.UseSqlite(connectionString));

        services.AddScoped<IMaintenanceMissionRepository, SqliteMaintenanceMissionRepository>();

        return services;
    }
}
