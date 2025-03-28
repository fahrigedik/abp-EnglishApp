using Volo.Abp.Modularity;

namespace EnglishApplication;

public abstract class EnglishApplicationApplicationTestBase<TStartupModule> : EnglishApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
