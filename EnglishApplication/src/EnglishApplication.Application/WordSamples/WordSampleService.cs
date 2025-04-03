using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using System.Linq.Dynamic.Core;

namespace EnglishApplication.WordSamples;

public class WordSampleService : ApplicationService, IWordSampleService
{
    private readonly IWordSampleRepository _wordSampleRepository;

    public WordSampleService(IWordSampleRepository wordSampleRepository)
    {
        _wordSampleRepository = wordSampleRepository;
    }

    public async Task<WordSampleDto> GetAsync(Guid id)
    {
        var wordSample = await _wordSampleRepository.GetAsync(id);
        return ObjectMapper.Map<WordSample, WordSampleDto>(wordSample);
    }

    public async Task<PagedResultDto<WordSampleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _wordSampleRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Sample" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var wordSamples = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<WordSampleDto>(
            totalCount,
            ObjectMapper.Map<List<WordSample>, List<WordSampleDto>>(wordSamples)
        );
    }

    public async Task<WordSampleDto> CreateAsync(CreateUpdateWordSampleDto input)
    {
        var wordSample = ObjectMapper.Map<CreateUpdateWordSampleDto, WordSample>(input);
        var createdWordSample = await _wordSampleRepository.InsertAsync(wordSample);
        return ObjectMapper.Map<WordSample, WordSampleDto>(createdWordSample);
    }

    public async Task<WordSampleDto> UpdateAsync(Guid id, CreateUpdateWordSampleDto input)
    {
        var wordSample = await _wordSampleRepository.GetAsync(id);
        ObjectMapper.Map(input, wordSample);
        await _wordSampleRepository.UpdateAsync(wordSample);
        return ObjectMapper.Map<WordSample, WordSampleDto>(wordSample);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _wordSampleRepository.DeleteAsync(id);
    }

    public async Task<PagedResultDto<WordSampleDto>> GetListByWordId(PagedAndSortedResultRequestDto input, Guid wordId)
    {
        var queryable = await _wordSampleRepository.GetQueryableByWordId(wordId);
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Sample" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var wordSamples = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<WordSampleDto>(
            totalCount,
            ObjectMapper.Map<List<WordSample>, List<WordSampleDto>>(wordSamples)
        );
    }
}

