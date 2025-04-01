using EnglishApplication.Words;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using IObjectMapper = Volo.Abp.ObjectMapping.IObjectMapper;

namespace EnglishApplication.Web.Pages.Words
{
    public class EditModalModel : PageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdateWordDto Word { get; set; }


        private readonly IWordService _wordService;
        private readonly IObjectMapper _objectMapper;
        public EditModalModel(IWordService wordService, IObjectMapper objectMapper)
        {
            _wordService = wordService;
            _objectMapper = objectMapper;
        }


        public async Task OnGetAsync()
        {
            var wordDto = await _wordService.GetAsync(Id);
            Word = _objectMapper.Map<WordDto, CreateUpdateWordDto>(wordDto);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _wordService.UpdateAsync(Id, Word);
            return null;
        }
    }
}
