using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnglishApplication.Words;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.WordSamples;

public interface IWordSampleService : ICrudAppService<WordSampleDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateWordSampleDto>
{
    public Task<PagedResultDto<WordSampleDto>> GetListByWordIdWithPaging(PagedAndSortedResultRequestDto input, Guid wordId);

    public Task<List<WordSampleDto>> GetListByWordId(Guid wordId);
}