using EnglishApplication.QuizAttempts.Dtos;
using EnglishApplication.QuizAttempts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System;

namespace EnglishApplication.Web.Pages.Quiz
{
    public class ResultModel : PageModel
    {
        public QuizResultDto QuizResult { get; set; }
        public QuizQuestionDto NextQuestion { get; set; }

        private readonly IQuizAppService _quizAppService;
        private readonly IQuizAttemptRepository _quizAttemptRepository;

        public ResultModel(
            IQuizAppService quizAppService,
            IQuizAttemptRepository quizAttemptRepository)
        {
            _quizAppService = quizAppService;
            _quizAttemptRepository = quizAttemptRepository;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            // Quiz denemesini al
            var quizAttempt = await _quizAttemptRepository.GetAsync(id);
            if (quizAttempt == null)
            {
                return NotFound();
            }

            // Sonuç verilerini hazýrla
            QuizResult = new QuizResultDto
            {
                QuizAttemptId = quizAttempt.Id,
                CorrectOptionIndex = GetCorrectOptionIndex(quizAttempt),
                SelectedOptionIndex = quizAttempt.SelectedOptionIndex,
                IsCorrect = quizAttempt.IsCorrect,
                CorrectTranslation = quizAttempt.CorrectOption,
                SelectedTranslation = GetSelectedOption(quizAttempt)
            };

            // Bir sonraki soruyu yükle
            NextQuestion = await _quizAppService.GetNextQuizQuestionAsync();

            return Page();
        }

        private int GetCorrectOptionIndex(QuizAttempt quizAttempt)
        {
            if (quizAttempt.Option1 == quizAttempt.CorrectOption) return 0;
            if (quizAttempt.Option2 == quizAttempt.CorrectOption) return 1;
            if (quizAttempt.Option3 == quizAttempt.CorrectOption) return 2;
            if (quizAttempt.Option4 == quizAttempt.CorrectOption) return 3;
            return -1;
        }

        private string GetSelectedOption(QuizAttempt quizAttempt)
        {
            switch (quizAttempt.SelectedOptionIndex)
            {
                case 0: return quizAttempt.Option1;
                case 1: return quizAttempt.Option2;
                case 2: return quizAttempt.Option3;
                case 3: return quizAttempt.Option4;
                default: return string.Empty;
            }
        }
    }
}
