using Infrastructure.Persistance;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SZRSTContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SZRST"),
                b => b.MigrationsAssembly("SZRST.Infrastructure")), ServiceLifetime.Transient);
                //   b => b.MigrationsAssembly(typeof(SZRSTContext).Assembly.FullName)), ServiceLifetime.Transient);

            services.AddScoped<ISZRSTContext>(provider => provider.GetService<SZRSTContext>());
            return services;
        }
    }
}