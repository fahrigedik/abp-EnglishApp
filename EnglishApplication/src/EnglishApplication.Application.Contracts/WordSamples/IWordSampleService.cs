using System;
using EnglishApplication.Words;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.WordSamples;

public interface IWordSampleService : ICrudAppService<WordSampleDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateWordSampleDto>
{
    
}