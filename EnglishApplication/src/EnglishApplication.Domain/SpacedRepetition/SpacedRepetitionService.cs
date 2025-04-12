using EnglishApplication.QuizAttempts;
using EnglishApplication.UserSettings;
using EnglishApplication.WordDetails;
using EnglishApplication.Words;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EnglishApplication.SpacedRepetition;


public class SpacedRepetitionService : DomainService
{
    private readonly IWordRepository _wordRepository;
    private readonly IWordDetailRepository _wordDetailRepository;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly IUserSettingRepository _userSettingRepository;

    public SpacedRepetitionService(
        IWordRepository wordRepository,
        IWordDetailRepository wordDetailRepository,
        IQuizAttemptRepository quizAttemptRepository,
        IUserSettingRepository userSettingRepository)
    {
        _wordRepository = wordRepository;
        _wordDetailRepository = wordDetailRepository;
        _quizAttemptRepository = quizAttemptRepository;
        _userSettingRepository = userSettingRepository;
    }

    // 6 tekrar algoritmasına göre tekrar aralıklarını hesapla (gün olarak)
    public int[] GetSpacingIntervals()
    {
        // Aralıklar: Aynı gün, 1 gün sonra, 3 gün sonra, 7 gün sonra, 14 gün sonra, 30 gün sonra
        return new int[] { 0, 1, 3, 7, 14, 30 };
    }

    // Doğru/yanlış cevaba göre kelime detayını güncelle ve sonraki tekrar tarihini hesapla
    public async Task<DateTime?> ProcessQuizAttemptAsync(Guid userId, Guid wordId, bool isCorrect)
    {
        var wordDetail = await _wordDetailRepository.FirstOrDefaultAsync(wd => wd.WordId == wordId);
        if (wordDetail == null)
        {
            // Eğer henüz bir WordDetail yoksa oluştur
            wordDetail = new WordDetail
            {
                WordId = wordId,
                TrueCount = 0,
                NextDate = DateTime.Now,
                IsLearn = false
            };
            await _wordDetailRepository.InsertAsync(wordDetail);
        }

        // TrueCount'u RepetitionStage olarak kullan
        var intervals = GetSpacingIntervals();
        int currentStage = wordDetail.TrueCount;

        // Doğru cevapsa bir sonraki aşamaya geç, yanlış cevapsa aşama 0'a dön
        if (isCorrect)
        {
            currentStage = Math.Min(currentStage + 1, intervals.Length - 1);
            wordDetail.TrueCount = currentStage;
        }
        else
        {
            currentStage = 0; // Yanlışsa başa dön
            wordDetail.TrueCount = 0;
        }

        // Öğrenildi mi kontrolü (son aşamaya geldiyse)
        if (currentStage == intervals.Length - 1 && isCorrect)
        {
            wordDetail.IsLearn = true;
        }

        // Bir sonraki tekrar tarihini hesapla
        DateTime nextDate = DateTime.Now.AddDays(intervals[currentStage]);
        wordDetail.NextDate = nextDate;

        await _wordDetailRepository.UpdateAsync(wordDetail);

        return nextDate;
    }

    // Kullanıcı için bugün çalışılması gereken kelimeleri getir (QuestionCount kadar)
    public async Task<List<Word>> GetDueWordsForUserAsync(Guid userId)
    {
        var today = DateTime.Now;

        // Kullanıcı ayarlarından soru sayısını al
        var userSetting = await _userSettingRepository.FirstOrDefaultAsync(us => us.UserId == userId);
        int questionCount = userSetting?.QuestionCount ?? 10; // Varsayılan değer 10

        // Kullanıcının kelimelerini al
        var userWords = await _wordRepository.GetWordsByUserId(userId);
        if (userWords.Count == 0)
        {
            return new List<Word>();
        }

        var userWordIds = userWords.Select(w => w.Id).ToList();

        // Bugün için zamanı gelmiş kelimelerin detaylarını bul
        var wordDetails = await _wordDetailRepository.GetDueWordDetailsAsync(userWordIds, today);


        // Eğer hiç kelime detayı yoksa, öğrenilmemiş kelimeleri al
        if (wordDetails.Count == 0)
        {
            // Detayı olmayan veya öğrenilmemiş kelimeleri bul
            var wordDetailsAll = await _wordDetailRepository.GetWordDetailsByWordIds(userWordIds);
            
            var existingWordIds = wordDetailsAll.Select(wd => wd.WordId).ToList();

            // Henüz detayı olmayan kelimeleri al
            var newWords = userWords
                .Where(w => !existingWordIds.Contains(w.Id))
                .Take(questionCount)
                .ToList();

            return newWords;
        }

        // WordId'leri kullanarak kelimeleri al
        var dueWordIds = wordDetails.Select(wd => wd.WordId).ToList();
        var dueWords = await _wordRepository.GetWordsByIds(dueWordIds);

        // Sorulacak kelimeleri kullanıcının ayarladığı sayıya göre sınırla
        return dueWords.Take(questionCount).ToList();
    }

    // Kullanıcının öğrendiği kelimelerin sayısını getir
    public async Task<int> GetLearnedWordCountAsync(Guid userId)
    {
        var words = await _wordRepository.GetWordsByUserId(userId);
        var wordIds = words.Select(w => w.Id).ToList();

        var learnedWordDetails = await _wordDetailRepository.CountAsync(
            wd => wordIds.Contains(wd.WordId) && wd.IsLearn == true);

        return learnedWordDetails;
    }

    // Kullanıcının toplam kelime sayısını getir
    public async Task<int> GetTotalWordCountAsync(Guid userId)
    {
        return await _wordRepository.CountAsync(w => w.UserId == userId);
    }
}