using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EnglishApplication.Data;

/* This is used if database provider does't define
 * IEnglishApplicationDbSchemaMigrator implementation.
 */
public class NullEnglishApplicationDbSchemaMigrator : IEnglishApplicationDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
