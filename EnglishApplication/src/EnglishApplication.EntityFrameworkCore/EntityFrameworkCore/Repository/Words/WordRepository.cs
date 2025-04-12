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
}