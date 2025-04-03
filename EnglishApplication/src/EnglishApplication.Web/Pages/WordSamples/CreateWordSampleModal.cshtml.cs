using System;
using System.Threading.Tasks;
using EnglishApplication.WordSamples;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishApplication.Web.Pages.WordSamples
{
    public class CreateWordSampleModalModel : PageModel
    {
        [BindProperty]
        public CreateUpdateWordSampleDto WordSample { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid WordId { get; set; }

        private readonly IWordSampleService _wordSampleService;

        public CreateWordSampleModalModel(IWordSampleService wordSampleService)
        {
            _wordSampleService = wordSampleService;
        }

        public void OnGet()
        {
            WordSample = new CreateUpdateWordSampleDto
            {
                WordId = WordId // Set the WordId from the query parameter
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var createdWordSample = await _wordSampleService.CreateAsync(WordSample);
            return new NoContentResult(); // This tells ABP UI that the operation was successful
        }
    }
}
