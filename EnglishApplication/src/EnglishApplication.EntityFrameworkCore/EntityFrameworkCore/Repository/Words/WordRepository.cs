using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.Words;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.Words;

public class WordRepository : EfCoreRepository<EnglishApplicationDbContext, Word, Guid>, IWordRepository
{
    public WordRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<Word>> GetWordsByUserId(Guid userId)
    {
        var words = await DbSet.Where(x => x.UserId == userId).ToListAsync();
        return words;
    }
}