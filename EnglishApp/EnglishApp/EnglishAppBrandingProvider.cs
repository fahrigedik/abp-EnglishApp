using Microsoft.Extensions.Localization;
using EnglishApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace EnglishApp;

[Dependency(ReplaceServices = true)]
public class EnglishAppBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EnglishAppResource> _localizer;

    public EnglishAppBrandingProvider(IStringLocalizer<EnglishAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}