using System.Threading.Tasks;
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

        public StatDto Stat { get; set; } = new StatDto();


        public StatsModel(
            IWordRepository wordRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ICurrentUser currentUser)
        {
            _wordRepository = wordRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _currentUser = currentUser;
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
            }
        }
    }
}
