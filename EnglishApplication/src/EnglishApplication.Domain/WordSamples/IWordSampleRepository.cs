using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.WordSamples;

public interface IWordSampleRepository : IRepository<WordSample, Guid>
{
    public Task<IQueryable<WordSample>> GetQueryableByWordId(Guid wordId);
}