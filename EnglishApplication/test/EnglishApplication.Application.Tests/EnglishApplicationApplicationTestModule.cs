using Volo.Abp.Modularity;

namespace EnglishApplication;

[DependsOn(
    typeof(EnglishApplicationApplicationModule),
    typeof(EnglishApplicationDomainTestModule)
)]
public class EnglishApplicationApplicationTestModule : AbpModule
{

}
