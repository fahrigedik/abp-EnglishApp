using Volo.Abp.Application.Services;
using EnglishApp.Localization;

namespace EnglishApp.Services;

/* Inherit your application services from this class. */
public abstract class EnglishAppAppService : ApplicationService
{
    protected EnglishAppAppService()
    {
        LocalizationResource = typeof(EnglishAppResource);
    }
}