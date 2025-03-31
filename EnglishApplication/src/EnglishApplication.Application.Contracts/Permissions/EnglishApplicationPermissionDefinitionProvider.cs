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

        //Define your own permissions here. Example:
        //myGroup.AddPermission(EnglishApplicationPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EnglishApplicationResource>(name);
    }
}
