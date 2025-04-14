using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.WordDetails;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
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

    public async Task DeleteAsync(Guid id)
    {
        await _wordRepository.DeleteAsync(id);
    }

    public async Task<PagedResultDto<WordDetailsDto>> GetWordDetailsByUserId(PagedAndSortedResultRequestDto input, Guid userId)
    {
        if (userId == Guid.Empty)
        {
            Logger.LogWarning("User ID cannot be empty", nameof(userId));
            return new PagedResultDto<WordDetailsDto>(0, new List<WordDetailsDto>());
        }

        // Get all words by user ID
        var words = await _wordRepository.GetWordsByUserId(userId);

        if (words == null || !words.Any())
        {
            return new PagedResultDto<WordDetailsDto>(0, new List<WordDetailsDto>());
        }

        // Create a list of word details DTOs
        var wordDetailDtos = new List<WordDetailsDto>();
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

        // Apply sorting (default to EnglishWordName if no sorting is specified)
        var sortProperty = input.Sorting.IsNullOrWhiteSpace() ? "EnglishWordName" : input.Sorting;
        bool isDescending = sortProperty.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);

        // Remove the " DESC" suffix if present
        if (isDescending)
        {
            sortProperty = sortProperty.Substring(0, sortProperty.Length - 5).Trim();
        }

        // Apply ordering
        IEnumerable<WordDetailsDto> query = sortProperty.ToLower() switch
        {
            "englishwordname" => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.EnglishWordName)
                : wordDetailDtos.OrderBy(w => w.EnglishWordName),
            "turkishwordname" => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.TurkishWordName)
                : wordDetailDtos.OrderBy(w => w.TurkishWordName),
            "truecount" => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.TrueCount)
                : wordDetailDtos.OrderBy(w => w.TrueCount),
            "nextdate" => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.NextDate)
                : wordDetailDtos.OrderBy(w => w.NextDate),
            "islearn" => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.IsLearn)
                : wordDetailDtos.OrderBy(w => w.IsLearn),
            _ => isDescending
                ? wordDetailDtos.OrderByDescending(w => w.EnglishWordName)
                : wordDetailDtos.OrderBy(w => w.EnglishWordName) // Default sort
        };

        // Get total count before pagination
        int totalCount = wordDetailDtos.Count;

        // Apply pagination
        var pagedResults = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        // Return paged result
        return new PagedResultDto<WordDetailsDto>(
            totalCount,
            pagedResults
        );
    }
}