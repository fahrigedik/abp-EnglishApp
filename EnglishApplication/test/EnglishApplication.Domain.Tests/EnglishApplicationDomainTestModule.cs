using Volo.Abp.Modularity;

namespace EnglishApplication;

[DependsOn(
    typeof(EnglishApplicationDomainModule),
    typeof(EnglishApplicationTestBaseModule)
)]
public class EnglishApplicationDomainTestModule : AbpModule
{

}
