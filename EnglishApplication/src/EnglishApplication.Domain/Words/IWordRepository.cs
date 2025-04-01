using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.Words;

public interface IWordRepository : IRepository<Word, Guid>
{
    public Task<List<Word>> GetWordsByUserId(Guid userId);

}