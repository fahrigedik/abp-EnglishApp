using System;
using EnglishApplication.WordSamples;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.WordSamples;

public class WordSampleRepository : EfCoreRepository<EnglishApplicationDbContext, WordSample, Guid>, IWordSampleRepository
{
    public WordSampleRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }
}