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
    /// Gets the default list of English-Turkish word pairs with picture URLs, organized by categories
    /// </summary>
    /// <returns>List of word objects containing English name, Turkish name, and picture URL</returns>
    private List<(string EnglishName, string TurkishName, string PictureUrl)> GetDefaultWordList()
    {
        return new List<(string EnglishName, string TurkishName, string PictureUrl)>
    {
        // 1. TEMEL GÜNLÜK KELİMELER (10)
        ("hello", "merhaba", "https://images.unsplash.com/photo-1540331547168-8b63109225b7"),
        ("goodbye", "hoşçakal", "https://images.unsplash.com/photo-1504277040523-430f7e1c984a"),
        ("please", "lütfen", "https://images.unsplash.com/photo-1545366244-0dbf40c14c4f"),
        ("thank you", "teşekkür ederim", "https://images.unsplash.com/photo-1553531889-e6cf4d692b1b"),
        ("yes", "evet", "https://images.unsplash.com/photo-1566413369430-09d505262f02"),
        ("no", "hayır", "https://images.unsplash.com/photo-1603475207421-3a3fdf3b5ac7"),
        ("sorry", "özür dilerim", "https://images.unsplash.com/photo-1523997596732-56d3099b3e18"),
        ("good", "iyi", "https://images.unsplash.com/photo-1591035897819-f4bdf739f446"),
        ("bad", "kötü", "https://images.unsplash.com/photo-1563013544-08f7b086893e"),
        ("time", "zaman", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade"),

        // 2. YEMEK ve İÇECEKLER (15)
        ("apple", "elma", "https://images.unsplash.com/photo-1568702846914-96b305d2aaeb"),
        ("bread", "ekmek", "https://images.unsplash.com/photo-1549931319-a545dcf3bc7c"),
        ("cheese", "peynir", "https://images.unsplash.com/photo-1589881133595-a3c085cb731d"),
        ("milk", "süt", "https://images.unsplash.com/photo-1563636619-e9143da7973b"),
        ("water", "su", "https://images.unsplash.com/photo-1523362628725-0c100150b8ea"),
        ("coffee", "kahve", "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd"),
        ("tea", "çay", "https://images.unsplash.com/photo-1571934811356-5cc061b6821f"),
        ("meat", "et", "https://images.unsplash.com/photo-1603048896760-d3c9c5a7a5ae"),
        ("chicken", "tavuk", "https://images.unsplash.com/photo-1610057099431-d73a1c9d2f2f"),
        ("fish", "balık", "https://images.unsplash.com/photo-1524704654690-b56c05c78a00"),
        ("egg", "yumurta", "https://images.unsplash.com/photo-1489571571749-78d037a8456c"),
        ("rice", "pirinç", "https://images.unsplash.com/photo-1536304929831-ee1ca9d44906"),
        ("vegetable", "sebze", "https://images.unsplash.com/photo-1557844352-761f2ddf6e39"),
        ("fruit", "meyve", "https://images.unsplash.com/photo-1619566636858-adf3ef46400b"),
        ("juice", "meyve suyu", "https://images.unsplash.com/photo-1613478223719-2ab802602423"),

        // 3. DOĞA ve ÇEVRE (10)
        ("tree", "ağaç", "https://images.unsplash.com/photo-1502082553048-f009c37129b9"),
        ("flower", "çiçek", "https://images.unsplash.com/photo-1490750967868-88aa4486c946"),
        ("river", "nehir", "https://images.unsplash.com/photo-1558299530-a7d3b4a3a7ff"),
        ("mountain", "dağ", "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b"),
        ("sea", "deniz", "https://images.unsplash.com/photo-1507525428034-b723cf961d3e"),
        ("sun", "güneş", "https://images.unsplash.com/photo-1506864845143-f4e481be9616"),
        ("moon", "ay", "https://images.unsplash.com/photo-1532693322450-2cb5c511067d"),
        ("star", "yıldız", "https://images.unsplash.com/photo-1519681393784-d120267933ba"),
        ("forest", "orman", "https://images.unsplash.com/photo-1448375240586-882707db888b"),
        ("weather", "hava durumu", "https://images.unsplash.com/photo-1592210454359-9043f067919b"),

        // 4. HAYVANLAR (10)
        ("dog", "köpek", "https://images.unsplash.com/photo-1587300003388-59208cc962cb"),
        ("cat", "kedi", "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba"),
        ("bird", "kuş", "https://images.unsplash.com/photo-1522926193341-e9ffd686c60f"),
        ("horse", "at", "https://images.unsplash.com/photo-1553284965-83fd3e82fa5a"),
        ("elephant", "fil", "https://images.unsplash.com/photo-1557050543-4d5f4e07ef46"),
        ("lion", "aslan", "https://images.unsplash.com/photo-1546182990-dffeafbe841d"),
        ("rabbit", "tavşan", "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308"),
        ("monkey", "maymun", "https://images.unsplash.com/photo-1540573133985-87b6da6d54a9"),
        ("snake", "yılan", "https://images.unsplash.com/photo-1531386151447-fd76ad50012f"),
        ("butterfly", "kelebek", "https://images.unsplash.com/photo-1452570053594-1b985d6ea890"),

        // 5. VÜCUT ve SAĞLIK (10)
        ("head", "baş", "https://images.unsplash.com/photo-1541710430735-5fca14c95b00"),
        ("hand", "el", "https://images.unsplash.com/photo-1573497620053-ea5300f94f21"),
        ("eye", "göz", "https://images.unsplash.com/photo-1494869042583-f6c911f04b4c"),
        ("heart", "kalp", "https://images.unsplash.com/photo-1505751172876-fa1923c5c528"),
        ("hair", "saç", "https://images.unsplash.com/photo-1503311268859-7eadd1c0974d"),
        ("leg", "bacak", "https://images.unsplash.com/photo-1562080849-ce58b7cb31db"),
        ("mouth", "ağız", "https://images.unsplash.com/photo-1581316378171-0a89a361f8a9"),
        ("nose", "burun", "https://images.unsplash.com/photo-1543643232-f1955d580b74"),
        ("ear", "kulak", "https://images.unsplash.com/photo-1513476382069-a3a3b5470518"),
        ("doctor", "doktor", "https://images.unsplash.com/photo-1537368910025-700350fe46c7"),

        // 6. EV ve EŞYALAR (10)
        ("house", "ev", "https://images.unsplash.com/photo-1570129477492-45c003edd2be"),
        ("table", "masa", "https://images.unsplash.com/photo-1533090161767-e6ffed986c88"),
        ("chair", "sandalye", "https://images.unsplash.com/photo-1503602642458-232111445657"),
        ("bed", "yatak", "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85"),
        ("door", "kapı", "https://images.unsplash.com/photo-1544864633-44cc6780f24d"),
        ("window", "pencere", "https://images.unsplash.com/photo-1490197415175-074fd86b1fcc"),
        ("kitchen", "mutfak", "https://images.unsplash.com/photo-1556910636-711966156c91"),
        ("bathroom", "banyo", "https://images.unsplash.com/photo-1584622650111-993a426fbf0a"),
        ("room", "oda", "https://images.unsplash.com/photo-1631495634750-0f14b50f4c5e"),
        ("garden", "bahçe", "https://images.unsplash.com/photo-1585320806297-9794b3e4eeae"),

        // 7. GİYİM ve MODA (10)
        ("clothes", "kıyafet", "https://images.unsplash.com/photo-1581798459219-306e7a0d9718"),
        ("shirt", "gömlek", "https://images.unsplash.com/photo-1596755094514-f87e34085b2c"),
        ("pants", "pantolon", "https://images.unsplash.com/photo-1541099649105-f69ad21f3246"),
        ("dress", "elbise", "https://images.unsplash.com/photo-1539008835657-9e8e9680c956"),
        ("shoe", "ayakkabı", "https://images.unsplash.com/photo-1542291026-7eec264c27ff"),
        ("hat", "şapka", "https://images.unsplash.com/photo-1584736286279-6174ede3c3a2"),
        ("jacket", "ceket", "https://images.unsplash.com/photo-1548126032-079a0fb0099d"),
        ("scarf", "atkı", "https://images.unsplash.com/photo-1601370552761-3488ad0642ff"),
        ("glove", "eldiven", "https://images.unsplash.com/photo-1578939662863-5cd416d45a69"),
        ("sock", "çorap", "https://images.unsplash.com/photo-1586350977771-b3b0abd50c82"),

        // 8. TEKNOLOJİ ve ARAÇLAR (10)
        ("computer", "bilgisayar", "https://images.unsplash.com/photo-1517694712202-14dd9538aa97"),
        ("phone", "telefon", "https://images.unsplash.com/photo-1585060544812-6b45742d762f"),
        ("car", "araba", "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf"),
        ("television", "televizyon", "https://images.unsplash.com/photo-1593784991095-a205069533cd"),
        ("radio", "radyo", "https://images.unsplash.com/photo-1593078165899-c7d2ac0d6aea"),
        ("camera", "kamera", "https://images.unsplash.com/photo-1516035069371-29a1b244cc32"),
        ("internet", "internet", "https://images.unsplash.com/photo-1558494949-ef010cbdcc31"),
        ("bicycle", "bisiklet", "https://images.unsplash.com/photo-1488684430052-f2d92fb178c2"),
        ("bus", "otobüs", "https://images.unsplash.com/photo-1570125909232-eb263c188f7e"),
        ("airplane", "uçak", "https://images.unsplash.com/photo-1544950110-15584be78101"),

        // 9. İŞ ve EĞİTİM (10)
        ("book", "kitap", "https://images.unsplash.com/photo-1544947950-fa07a98d237f"),
        ("pen", "kalem", "https://images.unsplash.com/photo-1585336261022-680e295ce3fe"),
        ("paper", "kağıt", "https://images.unsplash.com/photo-1582139329536-e7284fece509"),
        ("school", "okul", "https://images.unsplash.com/photo-1580582932707-520aed937b7b"),
        ("teacher", "öğretmen", "https://images.unsplash.com/photo-1569611930597-42e7ab8d4cbf"),
        ("student", "öğrenci", "https://images.unsplash.com/photo-1519452635265-7b1fbfd1e4e0"),
        ("university", "üniversite", "https://images.unsplash.com/photo-1541339907198-e08756dedf3f"),
        ("job", "iş", "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d"),
        ("office", "ofis", "https://images.unsplash.com/photo-1497366754035-f200968a6e72"),
        ("meeting", "toplantı", "https://images.unsplash.com/photo-1519389950473-47ba0277781c"),

        // 10. İNSAN İLİŞKİLERİ (5)
        ("family", "aile", "https://images.unsplash.com/photo-1541943181603-d8fe267a5dcf"),
        ("friend", "arkadaş", "https://images.unsplash.com/photo-1529156069898-49953e39b3ac"),
        ("love", "aşk", "https://images.unsplash.com/photo-1518199266791-5375a83190b7"),
        ("child", "çocuk", "https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9"),
        ("people", "insanlar", "https://images.unsplash.com/photo-1534531173927-aeb928d54385")
    };
    }

}