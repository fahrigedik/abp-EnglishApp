using EnglishApplication.UserSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;

namespace EnglishApplication.Web.Pages.UserSettings
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public UserSettingsViewModel UserSettings { get; set; }

        private readonly IUserSettingService _userSettingService;

        public IndexModel(IUserSettingService userSettingService)
        {
            _userSettingService = userSettingService;
            UserSettings = new UserSettingsViewModel();
        }

        public async Task OnGetAsync()
        {
            var userSetting = await _userSettingService.GetByCurrentUserAsync();
            UserSettings = new UserSettingsViewModel
            {
                Id = userSetting.Id,
                QuestionCount = userSetting.QuestionCount,
                UserId = userSetting.UserId
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var updateDto = new CreateUpdateUserSettingDto
            {
                QuestionCount = UserSettings.QuestionCount,
                UserId = UserSettings.UserId
            };

            await _userSettingService.UpdateCurrentUserSettingsAsync(updateDto);
            return RedirectToPage("/UserSettings/Index");
        }

        public class UserSettingsViewModel
        {
            public Guid Id { get; set; }

            [Required]
            [Display(Name = "Question Count")]
            [Range(1, 100, ErrorMessage = "Question count must be between 1 and 100")]
            public int QuestionCount { get; set; } = 10;

            [HiddenInput]
            public Guid UserId { get; set; }
        }

    }
}
