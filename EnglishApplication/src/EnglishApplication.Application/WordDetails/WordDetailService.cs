using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.WordDetails;


[Authorize(Roles = "student, admin")]
public class WordDetailService : ApplicationService, IWordDetailService
{
    private readonly IWordDetailRepository _wordDetailRepository;
    public WordDetailService(IWordDetailRepository wordDetailRepository)
    {
        _wordDetailRepository = wordDetailRepository;
    }

    public Task<WordDetailDto> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResultDto<WordDetailDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        throw new NotImplementedException();
    }

    public async Task<WordDetailDto> CreateAsync(CreateUpdateWordDetailDto input)
    {
        var wordDetail = ObjectMapper.Map<CreateUpdateWordDetailDto, WordDetail>(input);
        await _wordDetailRepository.InsertAsync(wordDetail);
        return ObjectMapper.Map<WordDetail, WordDetailDto>(wordDetail);
    }

    public Task<WordDetailDto> UpdateAsync(Guid id, CreateUpdateWordDetailDto input)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}