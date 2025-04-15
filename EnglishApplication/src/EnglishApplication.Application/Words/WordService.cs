using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishApplication.UserSettings;
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
    private readonly IUserSettingRepository _userSettingService;

    public WordService(
        IWordRepository wordRepository,
        IWordDetailRepository wordDetailRepository,
        ICurrentUser currentUser, IUserSettingRepository userSettingService)
    {
        _wordRepository = wordRepository;
        _wordDetailRepository = wordDetailRepository;
        _currentUser = currentUser;
        _userSettingService = userSettingService;
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

    public async Task<bool> AddWordSetByUserId(Guid UserId)
    {
        var isWordSetLoad = await _userSettingService.GetIsWordSetLoad(UserId);
        if (isWordSetLoad == false)
        {
            var defaultWords = GetDefaultWordList().Select(item =>
                new Word
                {
                    EnglishWordName = item.EnglishName,
                    TurkishWordName = item.TurkishName,
                    Picture = item.PictureUrl,
                    UserId = UserId
                }).ToList();

            await _wordRepository.InsertManyAsync(defaultWords);

            // Update user setting to indicate word set has been loaded
            var userSetting = await _userSettingService.FirstOrDefaultAsync(us => us.UserId == UserId);
            if (userSetting != null)
            {
                userSetting.IsWordSetLoad = true;
                await _userSettingService.UpdateAsync(userSetting);
            }

            return true;
        }
        return false;
    }




    /// <summary>
    /// Gets the default list of English-Turkish word pairs with picture URLs
    /// </summary>
    /// <returns>List of word objects containing English name, Turkish name, and picture URL</returns>
    private List<(string EnglishName, string TurkishName, string PictureUrl)> GetDefaultWordList()
    {
        return new List<(string EnglishName, string TurkishName, string PictureUrl)>
        {
        ("apple", "elma", "https://images.unsplash.com/photo-1568702846914-96b305d2aaeb"),
        ("book", "kitap", "https://images.unsplash.com/photo-1544947950-fa07a98d237f"),
        ("computer", "bilgisayar", "https://images.unsplash.com/photo-1517694712202-14dd9538aa97"),
        ("house", "ev", "https://images.unsplash.com/photo-1570129477492-45c003edd2be"),
        ("car", "araba", "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf"),
        ("water", "su", "https://images.unsplash.com/photo-1523362628725-0c100150b8ea"),
        ("sun", "güneş", "https://images.unsplash.com/photo-1506864845143-f4e481be9616"),
        ("moon", "ay", "https://images.unsplash.com/photo-1532693322450-2cb5c511067d"),
        ("phone", "telefon", "https://images.unsplash.com/photo-1585060544812-6b45742d762f"),
        ("friend", "arkadaş", "https://images.unsplash.com/photo-1529156069898-49953e39b3ac"),
        // Add more words here (continue to 100)
        
        // Nature and Environment
        ("tree", "ağaç", "https://images.unsplash.com/photo-1502082553048-f009c37129b9"),
        ("flower", "çiçek", "https://images.unsplash.com/photo-1490750967868-88aa4486c946"),
        ("river", "nehir", "https://images.unsplash.com/photo-1558299530-a7d3b4a3a7ff"),
        ("mountain", "dağ", "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b"),
        ("sea", "deniz", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e"),
        
        // Food and Drinks
        ("bread", "ekmek", "https://images.unsplash.com/photo-1549931319-a545dcf3bc7c"),
        ("cheese", "peynir", "https://images.unsplash.com/photo-1589881133595-a3c085cb731d"),
        ("milk", "süt", "https://images.unsplash.com/photo-1563636619-e9143da7973b"),
        ("coffee", "kahve", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd"),
        ("tea", "çay", "https://images.unsplash.com/photo-1571934811356-5cc061b6821f"),
        
        // Add more categories and words to reach 100 total words
        };
    }
}