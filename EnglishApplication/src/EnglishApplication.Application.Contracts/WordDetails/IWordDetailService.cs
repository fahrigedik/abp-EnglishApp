using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.WordDetails;

public interface IWordDetailService : ICrudAppService<WordDetailDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateWordDetailDto>
{
    
}