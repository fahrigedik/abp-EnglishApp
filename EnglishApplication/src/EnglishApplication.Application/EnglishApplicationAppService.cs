using EnglishApplication.Localization;
using Volo.Abp.Application.Services;

namespace EnglishApplication;

/* Inherit your application services from this class.
 */
public abstract class EnglishApplicationAppService : ApplicationService
{
    protected EnglishApplicationAppService()
    {
        LocalizationResource = typeof(EnglishApplicationResource);
    }
}
