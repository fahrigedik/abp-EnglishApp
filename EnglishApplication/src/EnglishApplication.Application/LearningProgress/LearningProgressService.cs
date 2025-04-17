// src/EnglishApplication.Application/Stats/LearningProgressService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.QuizAttempts;
using EnglishApplication.Words;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace EnglishApplication.LearningProgress
{

    [Authorize(Roles = "student, admin")]
    public class LearningProgressService : ApplicationService, ILearningProgressService
    {
        private readonly IWordRepository _wordRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly ICurrentUser _currentUser;

        public LearningProgressService(
            IWordRepository wordRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ICurrentUser currentUser)
        {
            _wordRepository = wordRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _currentUser = currentUser;
        }

        public async Task<List<LearningProgressDto>> GetLearningProgressAsync(int days = 7)
        {
            if (!_currentUser.IsAuthenticated)
            {
                return new List<LearningProgressDto>();
            }

            var userId = _currentUser.GetId();

            // Get the daily learned words count
            var learnedWordsData = await _wordRepository.GetDailyLearnedWordsCountByUserId(userId, days);

            // Get the daily correct answers count
            var correctAnswersData = await _quizAttemptRepository.GetDailyCorrectAnswersCountByUserId(userId, days);

            // Combine the data into a single DTO list
            var result = new List<LearningProgressDto>();

            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Now.Date.AddDays(-(days - 1 - i));
                var learnedCount = learnedWordsData.FirstOrDefault(d => d.Date.Date == date.Date).LearnedCount;
                var correctCount = correctAnswersData.FirstOrDefault(d => d.Date.Date == date.Date).CorrectCount;

                result.Add(new LearningProgressDto
                {
                    Date = date,
                    LearnedWordsCount = learnedCount,
                    CorrectAnswersCount = correctCount
                });
            }

            return result;
        }
    }
}
