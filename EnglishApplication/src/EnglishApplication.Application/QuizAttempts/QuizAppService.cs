using EnglishApplication.QuizAttempts.Dtos;
using EnglishApplication.SpacedRepetition;
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

    // Session key to store already asked word IDs
    private const string SESSION_ASKED_WORDS_KEY = "QuizAskedWordIds";

    public QuizAppService(
        QuizService quizService,
        SpacedRepetitionService spacedRepetitionService,
        IRepository<Word, Guid> wordRepository,
        ICurrentUser currentUser,
        IQuizAttemptRepository quizAttemptRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _quizService = quizService;
        _spacedRepetitionService = spacedRepetitionService;
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _quizAttemptRepository = quizAttemptRepository;
        _httpContextAccessor = httpContextAccessor;
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

    // Clear the asked words session
    public void ResetAskedWords()
    {
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_ASKED_WORDS_KEY);
    }

    public async Task<QuizQuestionDto> GetNextQuizQuestionAsync()
    {
        var userId = _currentUser.Id.Value;

        // Zaten sorulan kelimeleri al
        var askedWords = GetAskedWordsFromSession();

        // Tekrar edilmesi gereken kelimeleri al
        var dueWords = await _spacedRepetitionService.GetDueWordsForUserAsync(userId);
        if (dueWords.Count == 0)
        {
            ResetAskedWords(); // Kelime kalmadığında session'ı sıfırla
            return null; // Çalışılacak kelime kalmadı
        }

        // Henüz sorulmamış kelimeleri filtrele
        var notAskedWords = dueWords.Where(w => !askedWords.Contains(w.Id)).ToList();

        // Eğer tüm kelimeler sorulmuşsa, session'ı sıfırla ve baştan başla
        if (notAskedWords.Count == 0)
        {
            ResetAskedWords();
            // İlk kelimeyi al
            var word = dueWords.First();
            var quizAttempt = await _quizService.CreateQuizAttemptAsync(userId, word.Id);

            // Kelimeyi sorulmuş olarak işaretle
            askedWords = new List<Guid> { word.Id };
            SaveAskedWordsToSession(askedWords);

            return new QuizQuestionDto
            {
                QuizAttemptId = quizAttempt.Id,
                WordId = word.Id,
                EnglishWord = word.EnglishWordName,
                Picture = word.Picture,
                Options = new List<string> { quizAttempt.Option1, quizAttempt.Option2, quizAttempt.Option3, quizAttempt.Option4 }
            };
        }

        // Henüz sorulmamış bir kelime seç
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