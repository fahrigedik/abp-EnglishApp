using System;
using System.Threading.Tasks;
using EnglishApplication.QuizAttempts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.QuizAttempts;

public class QuizAttemptRepository : EfCoreRepository<EnglishApplicationDbContext, QuizAttempt, Guid>, IQuizAttemptRepository
{
    public QuizAttemptRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }

    public async Task<int> GetQuestionResolveCountByUserIdAsync(Guid userId)
    {
        return await DbSet.CountAsync(x => x.UserId == userId);
    }

    public async Task<int> GetTrueQuestionResolveCountByUserId(Guid userId)
    {
        return await DbSet.CountAsync(x => x.UserId == userId && x.IsCorrect);
    }

    public async Task<int> GetFalseQuestionResolveCountByUserId(Guid userId)
    {
        return await DbSet.CountAsync(x => x.UserId == userId && !x.IsCorrect);
    }
}