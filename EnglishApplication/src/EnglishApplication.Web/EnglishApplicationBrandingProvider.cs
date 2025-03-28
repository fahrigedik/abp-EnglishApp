using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using EnglishApplication.Localization;

namespace EnglishApplication.Web;

[Dependency(ReplaceServices = true)]
public class EnglishApplicationBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EnglishApplicationResource> _localizer;

    public EnglishApplicationBrandingProvider(IStringLocalizer<EnglishApplicationResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
