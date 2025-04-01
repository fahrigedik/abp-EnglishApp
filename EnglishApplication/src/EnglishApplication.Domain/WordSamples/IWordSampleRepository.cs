using System;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.WordSamples;

public interface IWordSampleRepository : IRepository<WordSample, Guid>
{

}