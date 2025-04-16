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
                // �al���lacak kelime kalmad�ysa veya QuestionCount'a ula��ld�ysa
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


            // Cevab� de�erlendir ve sonucu al
            var result = await _quizAppService.SubmitQuizAnswerAsync(Answer);

            if (result.IsCorrect)
            {
                // Do�ru cevap verilmi�se sonu� sayfas�na git
                return RedirectToPage("/Quiz/Result", new { id = result.QuizAttemptId });
            }
            else
            {

                CurrentQuestion = await _quizAppService.GetNextQuizQuestionAsync();

                if (CurrentQuestion == null)
                {
                    // T�m sorular tamamland�ysa veya QuestionCount'a ula��ld�ysa
                    return RedirectToPage("/Quiz/Complete");
                }

                return RedirectToPage("/Quiz/Result", new { id = result.QuizAttemptId });
            }
        }
    }
}
