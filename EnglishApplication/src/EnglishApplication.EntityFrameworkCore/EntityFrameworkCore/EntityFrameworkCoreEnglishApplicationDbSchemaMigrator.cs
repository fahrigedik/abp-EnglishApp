using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EnglishApplication.Data;
using Volo.Abp.DependencyInjection;

namespace EnglishApplication.EntityFrameworkCore;

public class EntityFrameworkCoreEnglishApplicationDbSchemaMigrator
    : IEnglishApplicationDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreEnglishApplicationDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the EnglishApplicationDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<EnglishApplicationDbContext>()
            .Database
            .MigrateAsync();
    }
}
