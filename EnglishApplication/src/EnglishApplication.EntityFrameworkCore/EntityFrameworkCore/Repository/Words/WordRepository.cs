using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.Words;

public class WordRepository : EfCoreRepository<EnglishApplicationDbContext, Word, Guid>, IWordRepository
{
    private readonly IDbContextProvider<EnglishApplicationDbContext> _dbContextProvider;

    public WordRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }


    public async Task<List<Word>> GetWordsByUserId(Guid userId)
    {

        var words = await (await _dbContextProvider.GetDbContextAsync()).Words.Where(x => x.UserId == userId).ToListAsync();
        return words;
    }

    public async Task<List<Word>> GetWordsByIds(List<Guid> Ids)
    {
        var words = await (await _dbContextProvider.GetDbContextAsync()).Words.Where(x => Ids.Contains(x.Id)).ToListAsync();
        return words;
    }

    public async Task<Word> GetWordById(Guid id)
    {
        var word = await (await _dbContextProvider.GetDbContextAsync()).Words.FirstOrDefaultAsync(x => x.Id == id);
        return word;
    }

    public async Task<int> GetLearnedWordCountByUserId(Guid userId)
    {
        var words = await (await _dbContextProvider.GetDbContextAsync()).Words.Where(x => x.UserId == userId).ToListAsync();
        var wordDetails = await (await _dbContextProvider.GetDbContextAsync()).WordDetails.ToListAsync();
        var learnedWords = words.Where(x => wordDetails.Any(y => y.WordId == x.Id && y.IsLearn == true)).ToList();
        return learnedWords.Count;
    }

    public async Task<List<Word>> GetLearnedWordsByUserId(Guid userId)
    {
        var words = await (await _dbContextProvider.GetDbContextAsync()).Words.Where(x => x.UserId == userId).ToListAsync();
        var wordDetails = await (await _dbContextProvider.GetDbContextAsync()).WordDetails.ToListAsync();
        var learnedWords = words.Where(x => wordDetails.Any(y => y.WordId == x.Id && y.IsLearn == true)).ToList();
        return learnedWords;
    }

    public async Task<List<(DateTime Date, int LearnedCount)>> GetDailyLearnedWordsCountByUserId(Guid userId, int days)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var startDate = DateTime.Now.Date.AddDays(-(days - 1));

        // Get words for this user
        var userWords = await dbContext.Words
            .Where(x => x.UserId == userId)
            .Select(w => w.Id)
            .ToListAsync();

        // Get word details that mark the words as learned
        // Fix for Error CS0019 - Handle nullable boolean comparison
        var learnedDetails = await dbContext.WordDetails
            .Where(wd => userWords.Contains(wd.WordId) && wd.IsLearn == true)
            .ToListAsync();

        // Group by the date when they were learned
        var queryResult = learnedDetails
            .GroupBy(wd => wd.CreationTime.Date)
            .Where(g => g.Key >= startDate)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToList();

        var results = new List<(DateTime Date, int LearnedCount)>();

        // Fill in all days in the range, including those with no data
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var entry = queryResult.FirstOrDefault(x => x.Date == date);

            // Fix for Error CS1503 - Explicitly specify tuple element names
            var count = entry != null ? entry.Count : 0;
            results.Add((Date: date, LearnedCount: count));
        }

        return results;
    }
}