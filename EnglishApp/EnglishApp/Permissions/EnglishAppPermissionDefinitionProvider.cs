using EnglishApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace EnglishApp.Permissions;

public class EnglishAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EnglishAppPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(EnglishAppPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(EnglishAppPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(EnglishAppPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(EnglishAppPermissions.Books.Delete, L("Permission:Books.Delete"));
        
        //Define your own permissions here. Example:
        //myGroup.AddPermission(EnglishAppPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EnglishAppResource>(name);
    }
}
