using Volo.Abp.Modularity;

namespace EnglishApplication;

/* Inherit from this class for your domain layer tests. */
public abstract class EnglishApplicationDomainTestBase<TStartupModule> : EnglishApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
