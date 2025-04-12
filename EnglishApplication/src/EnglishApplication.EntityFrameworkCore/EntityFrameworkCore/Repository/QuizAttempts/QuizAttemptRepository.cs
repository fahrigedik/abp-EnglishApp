using System;
using EnglishApplication.QuizAttempts;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.QuizAttempts;

public class QuizAttemptRepository : EfCoreRepository<EnglishApplicationDbContext, QuizAttempt, Guid>, IQuizAttemptRepository
{
    public QuizAttemptRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }
}