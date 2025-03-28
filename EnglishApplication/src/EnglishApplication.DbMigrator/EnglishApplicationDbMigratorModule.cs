using EnglishApplication.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EnglishApplication.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EnglishApplicationEntityFrameworkCoreModule),
    typeof(EnglishApplicationApplicationContractsModule)
)]
public class EnglishApplicationDbMigratorModule : AbpModule
{
}
