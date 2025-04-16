using EnglishApplication.QuizAttempts.Dtos;
using EnglishApplication.SpacedRepetition;
using EnglishApplication.UserSettings;
using EnglishApplication.Words;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Http;

namespace EnglishApplication.QuizAttempts;

[Authorize]
public class QuizAppService : ApplicationService, IQuizAppService
{
    private readonly QuizService _quizService;
    private readonly SpacedRepetitionService _spacedRepetitionService;
    private readonly IRepository<Word, Guid> _wordRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserSettingRepository _userSettingRepository;

    // Session keys
    private const string SESSION_ASKED_WORDS_KEY = "QuizAskedWordIds";
    private const string SESSION_QUESTION_COUNT_KEY = "QuizQuestionCount";
    private const string SESSION_TOTAL_QUESTIONS_KEY = "QuizTotalQuestions";
    private const string SESSION_QUESTIONS_ANSWERED_KEY = "QuizQuestionsAnswered";
    private const string SESSION_QUIZ_ACTIVE_KEY = "QuizActive";

    public QuizAppService(
        QuizService quizService,
        SpacedRepetitionService spacedRepetitionService,
        IRepository<Word, Guid> wordRepository,
        ICurrentUser currentUser,
        IQuizAttemptRepository quizAttemptRepository,
        IHttpContextAccessor httpContextAccessor,
        IUserSettingRepository userSettingRepository)
    {
        _quizService = quizService;
        _spacedRepetitionService = spacedRepetitionService;
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _quizAttemptRepository = quizAttemptRepository;
        _httpContextAccessor = httpContextAccessor;
        _userSettingRepository = userSettingRepository;
    }

    // Helper method to get asked words from session
    private List<Guid> GetAskedWordsFromSession()
    {
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_ASKED_WORDS_KEY, out byte[] data))
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json);
        }

        return new List<Guid>();
    }

    // Helper method to save asked words to session
    private void SaveAskedWordsToSession(List<Guid> askedWords)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(askedWords);
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_ASKED_WORDS_KEY,
            System.Text.Encoding.UTF8.GetBytes(json));
    }

    // Get question count from session or initialize
    private async Task<int> GetQuestionCountAsync()
    {
        // If already in session, return it
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_QUESTION_COUNT_KEY, out byte[] data))
        {
            string countString = System.Text.Encoding.UTF8.GetString(data);
            if (int.TryParse(countString, out int count) && count > 0)
            {
                return count;
            }
        }

        // Otherwise, get from user settings
        var userId = _currentUser.Id.Value;
        var userSetting = await _userSettingRepository.FirstOrDefaultAsync(us => us.UserId == userId);

        // Eğer kullanıcı ayarı yoksa, oluştur
        if (userSetting == null)
        {
            userSetting = new UserSetting
            {
                UserId = userId,
                QuestionCount = 10 // Varsayılan değer
            };

            await _userSettingRepository.InsertAsync(userSetting, true);
        }

        int questionCount = userSetting.QuestionCount;

        // Save to session
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTION_COUNT_KEY,
            System.Text.Encoding.UTF8.GetBytes(questionCount.ToString()));

        return questionCount;
    }

    // Set quiz as active
    private void SetQuizActive(bool active)
    {
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUIZ_ACTIVE_KEY,
            System.Text.Encoding.UTF8.GetBytes(active.ToString()));
    }

    // Check if quiz is active
    private bool IsQuizActive()
    {
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_QUIZ_ACTIVE_KEY, out byte[] data))
        {
            string activeString = System.Text.Encoding.UTF8.GetString(data);
            if (bool.TryParse(activeString, out bool active))
            {
                return active;
            }
        }

        return false;
    }

    // Get number of questions answered so far
    private int GetQuestionsAnswered()
    {
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_QUESTIONS_ANSWERED_KEY, out byte[] data))
        {
            string answeredString = System.Text.Encoding.UTF8.GetString(data);
            if (int.TryParse(answeredString, out int answered))
            {
                return answered;
            }
        }

        return 0;
    }

    // Increment questions answered counter
    private void IncrementQuestionsAnswered()
    {
        int currentAnswered = GetQuestionsAnswered();
        currentAnswered++;

        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTIONS_ANSWERED_KEY,
            System.Text.Encoding.UTF8.GetBytes(currentAnswered.ToString()));
    }

    // Initialize quiz session
    private async Task InitializeQuizSessionAsync()
    {
        // Soru sayısını kullanıcı ayarlarından al
        int questionCount = await GetQuestionCountAsync();

        // Reset all session values
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_ASKED_WORDS_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_QUESTIONS_ANSWERED_KEY);

        // Set initial values
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTIONS_ANSWERED_KEY,
            System.Text.Encoding.UTF8.GetBytes("0"));

        // Mark quiz as active
        SetQuizActive(true);
    }

    // Clear the quiz session
    public void ResetQuizSession()
    {
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_ASKED_WORDS_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_QUESTION_COUNT_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_TOTAL_QUESTIONS_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_QUESTIONS_ANSWERED_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_QUIZ_ACTIVE_KEY);
    }

    public async Task<QuizQuestionDto> GetNextQuizQuestionAsync()
    {
        var userId = _currentUser.Id.Value;

        // Quiz aktif değilse, oturumu başlat
        if (!IsQuizActive())
        {
            await InitializeQuizSessionAsync();
        }

        // Cevaplanan soru sayısını kontrol et
        int questionsAnswered = GetQuestionsAnswered();
        int questionCount = await GetQuestionCountAsync();

        // Eğer ayarlanan soru sayısına ulaşıldıysa, quiz'i sonlandır
        if (questionsAnswered >= questionCount)
        {
            ResetQuizSession(); // Reset for next session
            return null;
        }

        // Daha önce sorulmuş kelimeleri al
        var askedWords = GetAskedWordsFromSession();

        // Kullanıcı için çalışılması gereken kelimeleri getir
        var dueWords = await _spacedRepetitionService.GetDueWordsForUserAsync(userId);
        if (dueWords.Count == 0)
        {
            ResetQuizSession(); // Reset session when no words are left
            return null; // No more words to study
        }

        // Henüz sorulmamış kelimeleri filtrele
        var notAskedWords = dueWords.Where(w => !askedWords.Contains(w.Id)).ToList();

        // Eğer tüm kelimeler sorulmuşsa ve henüz soru limiti dolmadıysa,
        // kelimeleri tekrar kullanabiliriz
        if (notAskedWords.Count == 0)
        {
            // Mevcut sorulmuş kelimelerin sayısını kontrol et
            // Eğer kullanıcının ayarladığı soru sayısı kadar kelime sorulmuşsa ve
            // tüm kelimeler bitmiş ise, quiz'i sonlandır
            if (questionsAnswered >= questionCount)
            {
                ResetQuizSession();
                return null;
            }

            // Henüz yeterli sayıda soru sorulmadıysa, sorulan kelimeleri sıfırla ve devam et
            askedWords = new List<Guid>();
            SaveAskedWordsToSession(askedWords);
            notAskedWords = dueWords;
        }

        // Sıradaki kelimeyi al
        var nextWord = notAskedWords.First();
        var nextQuizAttempt = await _quizService.CreateQuizAttemptAsync(userId, nextWord.Id);

        // Kelimeyi sorulmuş olarak işaretle
        askedWords.Add(nextWord.Id);
        SaveAskedWordsToSession(askedWords);

        return new QuizQuestionDto
        {
            QuizAttemptId = nextQuizAttempt.Id,
            WordId = nextWord.Id,
            EnglishWord = nextWord.EnglishWordName,
            Picture = nextWord.Picture,
            Options = new List<string> { nextQuizAttempt.Option1, nextQuizAttempt.Option2, nextQuizAttempt.Option3, nextQuizAttempt.Option4 }
        };
    }

    public async Task<QuizResultDto> SubmitQuizAnswerAsync(QuizAnswerDto answerDto)
    {
        var userId = _currentUser.Id.Value;

        // Cevabı değerlendir
        bool isCorrect = await _quizService.EvaluateQuizAttemptAsync(
            answerDto.QuizAttemptId,
            answerDto.SelectedOptionIndex);

        // Quiz denemesini al
        var quizAttempt = await _quizAttemptRepository.GetAsync(answerDto.QuizAttemptId);

        // Kelimeyi al
        var word = await _wordRepository.GetAsync(quizAttempt.WordId.Value);

        // Spaced repetition algoritmasını uygula
        await _spacedRepetitionService.ProcessQuizAttemptAsync(
            userId,
            quizAttempt.WordId.Value,
            isCorrect);

        // Doğru cevabın indeksini bul
        int correctOptionIndex = -1;
        if (quizAttempt.Option1 == quizAttempt.CorrectOption) correctOptionIndex = 0;
        else if (quizAttempt.Option2 == quizAttempt.CorrectOption) correctOptionIndex = 1;
        else if (quizAttempt.Option3 == quizAttempt.CorrectOption) correctOptionIndex = 2;
        else if (quizAttempt.Option4 == quizAttempt.CorrectOption) correctOptionIndex = 3;

        // Cevaplanan soru sayısını artır - doğru ve yanlış cevaplar için
        IncrementQuestionsAnswered();

        // Sonuç DTO'sunu oluştur ve döndür
        return new QuizResultDto
        {
            QuizAttemptId = quizAttempt.Id,
            EnglishWord = word.EnglishWordName,
            CorrectTranslation = quizAttempt.CorrectOption,
            SelectedTranslation = GetOptionByIndex(quizAttempt, answerDto.SelectedOptionIndex),
            IsCorrect = isCorrect,
            CorrectOptionIndex = correctOptionIndex,
            SelectedOptionIndex = answerDto.SelectedOptionIndex
        };
    }

    // Yanıt değerlendirmeden bir sonraki soruya geç
    public async Task<QuizQuestionDto> SkipToNextQuestionAsync(QuizAnswerDto answerDto)
    {
        var userId = _currentUser.Id.Value;

        // Cevabı değerlendir ama sonucu kullanma
        bool isCorrect = await _quizService.EvaluateQuizAttemptAsync(
            answerDto.QuizAttemptId,
            answerDto.SelectedOptionIndex);

        // Quizz attempt'i al
        var quizAttempt = await _quizAttemptRepository.GetAsync(answerDto.QuizAttemptId);

        // Spaced repetition algoritmasını uygula
        await _spacedRepetitionService.ProcessQuizAttemptAsync(
            userId,
            quizAttempt.WordId.Value,
            isCorrect);

        // Cevaplanan soru sayısını artır
        IncrementQuestionsAnswered();

        // Bir sonraki soruyu getir
        return await GetNextQuizQuestionAsync();
    }

    private string GetOptionByIndex(QuizAttempt attempt, int index)
    {
        switch (index)
        {
            case 0: return attempt.Option1;
            case 1: return attempt.Option2;
            case 2: return attempt.Option3;
            case 3: return attempt.Option4;
            default: return string.Empty;
        }
    }
}
