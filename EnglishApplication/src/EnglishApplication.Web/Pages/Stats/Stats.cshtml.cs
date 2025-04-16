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

        public List<LearningProgressDto> LearningProgress { get; set; } = new List<LearningProgressDto>();

        public StatDto Stat { get; set; } = new StatDto();


        public StatsModel(
            IWordRepository wordRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ICurrentUser currentUser, 
            ILearningProgressService learningProgressService)
        {
            _wordRepository = wordRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _currentUser = currentUser;
            _learningProgressService = learningProgressService;
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
    }
}
