using EnglishApplication.WordSamples;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System;

namespace EnglishApplication.Web.Pages.WordSamples
{
    public class EditWordSampleModalModel : PageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdateWordSampleDto WordSample { get; set; }

        private readonly IWordSampleService _wordSampleService;

        public EditWordSampleModalModel(IWordSampleService wordSampleService)
        {
            _wordSampleService = wordSampleService;
            WordSample = new CreateUpdateWordSampleDto();
        }

        public async Task OnGetAsync()
        {
            var wordSample = await _wordSampleService.GetAsync(Id);

            WordSample = new CreateUpdateWordSampleDto
            {
                WordId = wordSample.WordId,
                Sample = wordSample.Sample
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _wordSampleService.UpdateAsync(Id, WordSample);
            return null;
        }
    }
}
