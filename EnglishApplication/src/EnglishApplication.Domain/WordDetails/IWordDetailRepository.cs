using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.WordDetails;

public interface IWordDetailRepository : IRepository<WordDetail, Guid>
{
    public Task<WordDetail> GetWordDetailByWordId(Guid wordId);
}