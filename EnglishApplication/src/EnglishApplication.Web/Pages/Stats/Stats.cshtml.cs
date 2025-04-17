using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnglishApplication.LearningProgress;
using EnglishApplication.QuizAttempts;
using EnglishApplication.Stats;
using EnglishApplication.Words;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Users;

namespace EnglishApplication.Web.Pages.Stats
{
    public class StatsModel : PageModel
    {
        private readonly IWordRepository _wordRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ILearningProgressService _learningProgressService;
        private readonly PdfGenerationService _pdfGenerationService;
        private readonly IWordService _wordService;

        public List<LearningProgressDto> LearningProgress { get; set; } = new List<LearningProgressDto>();

        public StatDto Stat { get; set; } = new StatDto();


        public StatsModel(
            IWordRepository wordRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ICurrentUser currentUser, 
            ILearningProgressService learningProgressService,
            PdfGenerationService pdfGenerationService,
            IWordService wordService)
        {
            _wordRepository = wordRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _currentUser = currentUser;
            _learningProgressService = learningProgressService;
            _pdfGenerationService = pdfGenerationService;
            _wordService = wordService;
        }

        public async Task OnGetAsync()
        {
            if (_currentUser.IsAuthenticated)
            {
                var userId = _currentUser.GetId();
                Stat.LearnedWordCount = await _wordRepository.GetLearnedWordCountByUserId(userId);
                Stat.QuestionCount = await _quizAttemptRepository.GetQuestionResolveCountByUserIdAsync(userId);
                Stat.TrueCount = await _quizAttemptRepository.GetTrueQuestionResolveCountByUserId(userId);
                Stat.FalseCount = await _quizAttemptRepository.GetFalseQuestionResolveCountByUserId(userId);

                LearningProgress = await _learningProgressService.GetLearningProgressAsync(7);

            }
        }

        public async Task<IActionResult> OnGetDownloadPdfAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userId = _currentUser.GetId();
            var userName = _currentUser.UserName ?? _currentUser.Email ?? "User";

            // Get statistics data
            var stat = new StatDto
            {
                LearnedWordCount = await _wordRepository.GetLearnedWordCountByUserId(userId),
                QuestionCount = await _quizAttemptRepository.GetQuestionResolveCountByUserIdAsync(userId),
                TrueCount = await _quizAttemptRepository.GetTrueQuestionResolveCountByUserId(userId),
                FalseCount = await _quizAttemptRepository.GetFalseQuestionResolveCountByUserId(userId),
                LearnedWords = await _wordService.GetLearnedWordByUserId(userId)
            };

            // Get learning progress data
            var learningProgress = await _learningProgressService.GetLearningProgressAsync(7);

            // Generate PDF
            var pdfBytes = _pdfGenerationService.GenerateStatisticsPdf(stat, learningProgress, userName);

            // Return the PDF as a file
            return File(pdfBytes, "application/pdf", $"stats-report-{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
