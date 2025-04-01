using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Users;

namespace EnglishApplication.Web.Pages.Words
{
    public class CreateModalModel : PageModel
    {


        [BindProperty]
        public CreateUpdateWordDto Word { get; set; }

        private readonly IWordService _wordService;
        private readonly IWordDetailService _wordDetailService;
        private readonly ICurrentUser currentUser;

        public CreateModalModel(
            IWordService wordService,
            IWordDetailService wordDetailService, 
            ICurrentUser currentUser
            )
        {
            _wordService = wordService;
            _wordDetailService = wordDetailService;
            this.currentUser = currentUser;

        }


        public void OnGet()
        {
            Word = new CreateUpdateWordDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
           var createdWord = await _wordService.CreateAsync(Word);
           var createdWordDetails = await _wordDetailService.CreateAsync(new CreateUpdateWordDetailDto()
               { 
                   IsLearn = false,
                   NextDate = null,
                   TrueCount = 0,
                   WordId = createdWord.Id
               });
            return null;
        }
    }
}
