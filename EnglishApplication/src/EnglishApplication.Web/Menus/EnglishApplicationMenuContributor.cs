using System.Threading.Tasks;
using EnglishApplication.Localization;
using EnglishApplication.Permissions;
using EnglishApplication.MultiTenancy;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace EnglishApplication.Web.Menus;

public class EnglishApplicationMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<EnglishApplicationResource>();

        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                EnglishApplicationMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);
        
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 7);


        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Words",
                l["Menu:Words"],
                icon: "fa fa-pencil"
            ).AddItem(
                new ApplicationMenuItem(
                    "Words.Words",
                    l["Menu:Words"],
                    url: "/Words"
                ).RequirePermissions(EnglishApplicationPermissions.Words.Default)
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                "UserSettings",
                l["Menu:UserSettings"],
                url: "/UserSettings"
            ).RequirePermissions(EnglishApplicationPermissions.UserSettings.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Quiz!",
                l["Menu:Quiz"],
                url: "/Quiz"
            ).RequirePermissions(EnglishApplicationPermissions.QuizAttempts.Default)
        );

        return Task.CompletedTask;
    }
}
