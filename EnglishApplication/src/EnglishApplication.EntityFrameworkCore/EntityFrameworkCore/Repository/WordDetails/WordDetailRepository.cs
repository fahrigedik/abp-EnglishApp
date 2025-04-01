using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.WordDetails;

public class WordDetailRepository : EfCoreRepository<EnglishApplicationDbContext, WordDetail, Guid>, IWordDetailRepository
{
    public WordDetailRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public Task<WordDetail> GetWordDetailByWordId(Guid wordId)
    {
        var wordDetail = DbSet.Where(x => x.WordId == wordId).FirstOrDefaultAsync();
        return wordDetail;
    }
}