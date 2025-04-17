using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Layout;

namespace EnglishApplication.Web.Pages
{

    [AllowAnonymous]
    public class HomeModel : EnglishApplicationPageModel
    {
        private readonly IPageLayout _pageLayout;

        public HomeModel(IPageLayout pageLayout)
        {
            _pageLayout = pageLayout;
        }

        public void OnGet()
        {
            _pageLayout.Content.Title = L["Home"];

            // These two lines are important for removing the sidebar
            _pageLayout.Content.BreadCrumb.ShowHome = false;
            _pageLayout.Content.BreadCrumb.ShowCurrent = false;
            _pageLayout.Content.MenuItemName = null;
        }
    }
}
