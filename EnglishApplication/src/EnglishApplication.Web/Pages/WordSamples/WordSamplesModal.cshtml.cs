using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace EnglishApplication.Web.Pages.WordSamples
{
    public class WordSamplesModalModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid WordId { get; set; }

        public void OnGet()
        {
        }
    }
}
