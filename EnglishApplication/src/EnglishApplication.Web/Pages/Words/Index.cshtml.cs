using System.Threading.Tasks;
using EnglishApplication.UserSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Volo.Abp.Users;

namespace EnglishApplication.Web.Pages.Words
{
    public class IndexModel : PageModel
    {
        private readonly IUserSettingService _userSettingService;
        private readonly ICurrentUser _currentUser;
        public bool IsWordSetLoad { get; set; } = false;
        public IndexModel(IUserSettingService userSettingService, ICurrentUser currentUser)
        {
            _userSettingService = userSettingService;
            _currentUser = currentUser;
        }
        public async Task OnGetAsync()
        {
            var userId = _currentUser.GetId();
            IsWordSetLoad = await _userSettingService.GetIsWordSetLoad(userId);
        }
    }
}
