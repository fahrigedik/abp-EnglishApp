using EnglishApplication.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace EnglishApplication.Permissions;

public class EnglishApplicationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EnglishApplicationPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(EnglishApplicationPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(EnglishApplicationPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(EnglishApplicationPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(EnglishApplicationPermissions.Books.Delete, L("Permission:Books.Delete"));

        var wordsPermission = myGroup.AddPermission(EnglishApplicationPermissions.Words.Default, L("Permission:Words"));
        wordsPermission.AddChild(EnglishApplicationPermissions.Words.Create, L("Permission:Words.Create"));
        wordsPermission.AddChild(EnglishApplicationPermissions.Words.Edit, L("Permission:Words.Edit"));
        wordsPermission.AddChild(EnglishApplicationPermissions.Words.Delete, L("Permission:Words.Delete"));

        var userSettingsPermission = myGroup.AddPermission(EnglishApplicationPermissions.UserSettings.Default, L("Permission:UserSettings"));
        userSettingsPermission.AddChild(EnglishApplicationPermissions.UserSettings.Create, L("Permission:UserSettings.Create"));
        userSettingsPermission.AddChild(EnglishApplicationPermissions.UserSettings.Edit, L("Permission:UserSettings.Edit"));
        userSettingsPermission.AddChild(EnglishApplicationPermissions.UserSettings.Delete, L("Permission:UserSettings.Delete"));
       
        var wordSamplesPermission = myGroup.AddPermission(EnglishApplicationPermissions.WordSamples.Default, L("Permission:WordSamples"));
        wordSamplesPermission.AddChild(EnglishApplicationPermissions.WordSamples.Create, L("Permission:WordSamples.Create"));
        wordSamplesPermission.AddChild(EnglishApplicationPermissions.WordSamples.Edit, L("Permission:WordSamples.Edit"));
        wordSamplesPermission.AddChild(EnglishApplicationPermissions.WordSamples.Delete, L("Permission:WordSamples.Delete"));
     
        var quizAttemptsPermission = myGroup.AddPermission(EnglishApplicationPermissions.QuizAttempts.Default, L("Permission:QuizAttempts"));
        quizAttemptsPermission.AddChild(EnglishApplicationPermissions.QuizAttempts.Create, L("Permission:QuizAttempts.Create"));
        quizAttemptsPermission.AddChild(EnglishApplicationPermissions.QuizAttempts.Edit, L("Permission:QuizAttempts.Edit"));
        quizAttemptsPermission.AddChild(EnglishApplicationPermissions.QuizAttempts.Delete, L("Permission:QuizAttempts.Delete"));

        var statsPermission = myGroup.AddPermission(EnglishApplicationPermissions.Stats.Default, L("Permission:Stats"));


        //Define the permissions for the application modules here.


        //Define your own permissions here. Example:
        //myGroup.AddPermission(EnglishApplicationPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EnglishApplicationResource>(name);
    }
}
