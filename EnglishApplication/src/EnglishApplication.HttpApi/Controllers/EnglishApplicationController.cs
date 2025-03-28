using EnglishApplication.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EnglishApplication.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class EnglishApplicationController : AbpControllerBase
{
    protected EnglishApplicationController()
    {
        LocalizationResource = typeof(EnglishApplicationResource);
    }
}
