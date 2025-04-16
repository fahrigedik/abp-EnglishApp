using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.QuizAttempts;

public interface IQuizAttemptRepository : IRepository<QuizAttempt, Guid>
{
    Task<int> GetQuestionResolveCountByUserIdAsync(Guid userId);

    Task<int> GetTrueQuestionResolveCountByUserId(Guid userId);

    Task<int> GetFalseQuestionResolveCountByUserId(Guid userId);
}