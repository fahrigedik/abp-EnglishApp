using EnglishApplication.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace EnglishApplication.Web.Pages;

public abstract class EnglishApplicationPageModel : AbpPageModel
{
    protected EnglishApplicationPageModel()
    {
        LocalizationResourceType = typeof(EnglishApplicationResource);
    }
}
