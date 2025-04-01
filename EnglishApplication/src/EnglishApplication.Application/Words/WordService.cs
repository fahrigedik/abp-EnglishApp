using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;
using static EnglishApplication.Permissions.EnglishApplicationPermissions;

namespace EnglishApplication.Words;

public class WordService : ApplicationService, IWordService
{
    private readonly IWordRepository _wordRepository;
    private readonly IWordDetailRepository _wordDetailRepository;
    private readonly ICurrentUser _currentUser;

    public WordService(
        IWordRepository wordRepository,
        IWordDetailRepository wordDetailRepository,
        ICurrentUser currentUser)
    {
        _wordRepository = wordRepository;
        _wordDetailRepository = wordDetailRepository;
        _currentUser = currentUser;
    }

    public async Task<WordDto> GetAsync(Guid id)
    {
        var word = await _wordRepository.GetAsync(id);
        return ObjectMapper.Map<Word, WordDto>(word);
    }

    public Task<PagedResultDto<WordDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        throw new NotImplementedException();
    }

    public async Task<WordDto> CreateAsync(CreateUpdateWordDto input)
    {
        var word = ObjectMapper.Map<CreateUpdateWordDto, Word>(input);
        word.UserId = _currentUser.Id!.Value;
       var createdWord = await _wordRepository.InsertAsync(word);
        return ObjectMapper.Map<Word, WordDto>(word);
    }

    public async Task<WordDto> UpdateAsync(Guid id, CreateUpdateWordDto input)
    {
        var word = await _wordRepository.GetAsync(id);
        ObjectMapper.Map(input, word);
        await _wordRepository.UpdateAsync(word);
        return ObjectMapper.Map<Word, WordDto>(word);
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<WordDetailsDto>> GetWordDetailsByUserId(Guid userId)
    {

        if (userId == Guid.Empty)
        {
            Logger.LogWarning("User ID cannot be empty", nameof(userId));

        }

        var words = await _wordRepository.GetWordsByUserId(userId);
        var wordDetailDtos = new List<WordDetailsDto>();

        if (words == null || !words.Any())
        {
            return wordDetailDtos;
        }

        foreach (var word in words)
        {
            if (word == null || word.Id == Guid.Empty)
            {
                continue;
            }

            var wordDetails = await _wordDetailRepository.GetWordDetailByWordId(word.Id);

            if (wordDetails == null)
            {
                Logger.LogWarning($"No word details found for word ID: {word.Id}");
                wordDetails = new WordDetail();
            }

            wordDetailDtos.Add(new WordDetailsDto
            {
                Id = word.Id,
                EnglishWordName = word.EnglishWordName ?? string.Empty,
                TurkishWordName = word.TurkishWordName ?? string.Empty,
                Picture = word.Picture ?? string.Empty,
                UserId = word.UserId,
                TrueCount = wordDetails.TrueCount,
                NextDate = wordDetails.NextDate,
                IsLearn = wordDetails.IsLearn,
                WordId = word.Id
            });
        }

        return wordDetailDtos;
    }
}