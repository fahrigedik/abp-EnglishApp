using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.Words;

public interface IWordRepository : IRepository<Word, Guid>
{
    public Task<List<Word>> GetWordsByUserId(Guid userId);

    public Task<List<Word>> GetWordsByIds(List<Guid> Ids);

    public Task<Word> GetWordById(Guid id);

}