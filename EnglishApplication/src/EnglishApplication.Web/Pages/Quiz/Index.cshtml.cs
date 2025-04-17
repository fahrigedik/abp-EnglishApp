using EnglishApplication.QuizAttempts.Dtos;
using EnglishApplication.QuizAttempts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace EnglishApplication.Web.Pages.Quiz
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public QuizAnswerDto Answer { get; set; }

        public QuizQuestionDto CurrentQuestion { get; set; }
        public QuizResultDto LastResult { get; set; }

        private readonly IQuizAppService _quizAppService;

        public IndexModel(IQuizAppService quizAppService)
        {
            _quizAppService = quizAppService;
            Answer = new QuizAnswerDto();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            LastResult = null; // Sonucu sýfýrla
            CurrentQuestion = await _quizAppService.GetNextQuizQuestionAsync();

            if (CurrentQuestion == null)
            {
                // Çalýþýlacak kelime kalmadýysa veya QuestionCount'a ulaþýldýysa
                return RedirectToPage("/Quiz/Complete");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentQuestion = await _quizAppService.GetNextQuizQuestionAsync();
                return Page();
            }

            // Cevabý deðerlendir ve sonucu al
            var result = await _quizAppService.SubmitQuizAnswerAsync(Answer);

            // Sonuç sayfasýna yönlendir (doðru veya yanlýþ cevap olmasýna bakýlmaksýzýn)
            return RedirectToPage("/Quiz/Result", new { id = result.QuizAttemptId });
        }
    }
}