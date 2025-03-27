using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace EnglishApp.Data;

public class EnglishAppDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EnglishAppDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the EnglishAppDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<EnglishAppDbContext>()
            .Database
            .MigrateAsync();

    }
}
