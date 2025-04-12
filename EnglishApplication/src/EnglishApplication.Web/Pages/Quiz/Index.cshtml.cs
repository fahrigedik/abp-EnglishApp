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
            LastResult = null; // Sonucu s�f�rla
            CurrentQuestion = await _quizAppService.GetNextQuizQuestionAsync();

            if (CurrentQuestion == null)
            {
                // �al���lacak kelime kalmad�ysa
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

            LastResult = await _quizAppService.SubmitQuizAnswerAsync(Answer);

            return RedirectToPage("/Quiz/Result", new { id = LastResult.QuizAttemptId });
        }
    }
}
