using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.Words;

public interface IWordService : ICrudAppService<WordDto, 
    Guid, 
    PagedAndSortedResultRequestDto,CreateUpdateWordDto>
{
    public Task<PagedResultDto<WordDetailsDto>> GetWordDetailsByUserId(PagedAndSortedResultRequestDto input, Guid userId);

    public Task<bool> AddWordSetByUserId(Guid UserId);
}