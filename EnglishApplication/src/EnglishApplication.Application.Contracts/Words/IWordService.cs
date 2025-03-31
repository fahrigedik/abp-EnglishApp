using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.Words;

public interface IWordService : ICrudAppService<WordDto, 
    Guid, 
    PagedAndSortedResultRequestDto,CreateUpdateWordDto>
{
    
}