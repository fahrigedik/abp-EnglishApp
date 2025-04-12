using System;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.QuizAttempts;

public interface IQuizAttemptRepository : IRepository<QuizAttempt, Guid>
{
    
}