using Volo.Abp.Settings;

namespace EnglishApplication.Settings;

public class EnglishApplicationSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(EnglishApplicationSettings.MySetting1));
    }
}
