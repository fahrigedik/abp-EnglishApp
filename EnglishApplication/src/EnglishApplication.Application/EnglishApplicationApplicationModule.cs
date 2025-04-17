using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using EnglishApplication.QuizAttempts;
using Microsoft.Extensions.DependencyInjection;

using QuestPDF.Infrastructure;

namespace EnglishApplication;

[DependsOn(
    typeof(EnglishApplicationDomainModule),
    typeof(EnglishApplicationApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class EnglishApplicationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<EnglishApplicationApplicationModule>();
        });
        context.Services.AddTransient<IQuizAppService, QuizAppService>();

        QuestPDF.Settings.License = LicenseType.Community;


    }
}
