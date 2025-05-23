﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordSamples;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.WordSamples;

public class WordSampleRepository : EfCoreRepository<EnglishApplicationDbContext, WordSample, Guid>, IWordSampleRepository
{
    public WordSampleRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }

    public async Task<IQueryable<WordSample>> GetQueryableByWordId(Guid wordId)
    {
        return (await GetQueryableAsync()).Where(x => x.WordId == wordId);
    }

    public async Task<List<WordSample>> GetListByWordId(Guid wordId)
    {
        return await DbSet.Where(x => x.WordId == wordId)
            .ToListAsync();
    }
}