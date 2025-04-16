using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<List<(DateTime Date, int CorrectCount)>> GetDailyCorrectAnswersCountByUserId(Guid userId, int days)
    {
        var dbContext = await GetDbContextAsync();
        var startDate = DateTime.Now.Date.AddDays(-(days - 1));

        var queryResult = await dbContext.QuizAttempts
            .Where(x => x.UserId == userId && x.CreationTime >= startDate && x.IsCorrect)
            .GroupBy(x => x.CreationTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var results = new List<(DateTime Date, int CorrectCount)>();

        // Fill in all days in the range, including those with no data
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var entry = queryResult.FirstOrDefault(x => x.Date == date);
            var count = entry != null ? entry.Count : 0;
            results.Add((date, count));
        }

        return results;
    }
}