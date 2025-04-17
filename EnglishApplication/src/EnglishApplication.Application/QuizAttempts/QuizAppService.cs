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
using EnglishApplication.WordSamples;

namespace EnglishApplication.QuizAttempts;

[Authorize(Roles = "student, admin")]
public class QuizAppService : ApplicationService, IQuizAppService
{
    private readonly QuizService _quizService;
    private readonly SpacedRepetitionService _spacedRepetitionService;
    private readonly IRepository<Word, Guid> _wordRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserSettingRepository _userSettingRepository;
    private readonly IWordSampleService _wordSampleService;

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
        IUserSettingRepository userSettingRepository,
        IWordSampleService wordSampleService)
    {
        _quizService = quizService;
        _spacedRepetitionService = spacedRepetitionService;
        _wordRepository = wordRepository;
        _currentUser = currentUser;
        _quizAttemptRepository = quizAttemptRepository;
        _httpContextAccessor = httpContextAccessor;
        _userSettingRepository = userSettingRepository;
        _wordSampleService = wordSampleService;
    }

    private List<Guid> GetAskedWordsFromSession()
    {
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_ASKED_WORDS_KEY, out byte[] data))
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json);
        }

        return new List<Guid>();
    }

    private void SaveAskedWordsToSession(List<Guid> askedWords)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(askedWords);
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_ASKED_WORDS_KEY,
            System.Text.Encoding.UTF8.GetBytes(json));
    }

    public async Task<int> GetQuestionCountAsync()
    {
        if (_httpContextAccessor.HttpContext.Session.TryGetValue(SESSION_QUESTION_COUNT_KEY, out byte[] data))
        {
            string countString = System.Text.Encoding.UTF8.GetString(data);
            if (int.TryParse(countString, out int count) && count > 0)
            {
                return count;
            }
        }

        var userId = _currentUser.Id.Value;
        var userSetting = await _userSettingRepository.FirstOrDefaultAsync(us => us.UserId == userId);

        if (userSetting == null)
        {
            userSetting = new UserSetting
            {
                UserId = userId,
                QuestionCount = 10 
            };

            await _userSettingRepository.InsertAsync(userSetting, true);
        }

        int questionCount = userSetting.QuestionCount;

        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTION_COUNT_KEY,
            System.Text.Encoding.UTF8.GetBytes(questionCount.ToString()));

        return questionCount;
    }

    private void SetQuizActive(bool active)
    {
        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUIZ_ACTIVE_KEY,
            System.Text.Encoding.UTF8.GetBytes(active.ToString()));
    }

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

    public int GetQuestionsAnswered()
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

    private void IncrementQuestionsAnswered()
    {
        int currentAnswered = GetQuestionsAnswered();
        currentAnswered++;

        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTIONS_ANSWERED_KEY,
            System.Text.Encoding.UTF8.GetBytes(currentAnswered.ToString()));
    }

    private async Task InitializeQuizSessionAsync()
    {
        int questionCount = await GetQuestionCountAsync();

        _httpContextAccessor.HttpContext.Session.Remove(SESSION_ASKED_WORDS_KEY);
        _httpContextAccessor.HttpContext.Session.Remove(SESSION_QUESTIONS_ANSWERED_KEY);

        _httpContextAccessor.HttpContext.Session.Set(
            SESSION_QUESTIONS_ANSWERED_KEY,
            System.Text.Encoding.UTF8.GetBytes("0"));

        SetQuizActive(true);
    }

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

        if (!IsQuizActive())
        {
            await InitializeQuizSessionAsync();
        }

        int questionsAnswered = GetQuestionsAnswered();
        int questionCount = await GetQuestionCountAsync();

        if (questionsAnswered >= questionCount)
        {
            ResetQuizSession(); // Reset for next session
            return null;
        }

        var askedWords = GetAskedWordsFromSession();

        var dueWords = await _spacedRepetitionService.GetDueWordsForUserAsync(userId);
        if (dueWords.Count == 0)
        {
            ResetQuizSession(); // Reset session when no words are left
            return null; // No more words to study
        }

        var notAskedWords = dueWords.Where(w => !askedWords.Contains(w.Id)).ToList();

        if (notAskedWords.Count == 0)
        {
            if (questionsAnswered >= questionCount)
            {
                ResetQuizSession();
                return null;
            }

            askedWords = new List<Guid>();
            SaveAskedWordsToSession(askedWords);
            notAskedWords = dueWords;
        }

        var nextWord = notAskedWords.First();
        var nextQuizAttempt = await _quizService.CreateQuizAttemptAsync(userId, nextWord.Id);

        askedWords.Add(nextWord.Id);
        SaveAskedWordsToSession(askedWords);

        List<WordSampleDto> samples = new List<WordSampleDto>();
        var wordSamples = await _wordSampleService.GetListByWordId(nextWord.Id);
        if (wordSamples.Count > 0)
        {
            int randomIndex = new Random().Next(wordSamples.Count);
            samples.Add(wordSamples[randomIndex]);
        }

        return new QuizQuestionDto
        {
            QuizAttemptId = nextQuizAttempt.Id,
            WordId = nextWord.Id,
            EnglishWord = nextWord.EnglishWordName,
            Picture = nextWord.Picture,
            Options = new List<string> { nextQuizAttempt.Option1, nextQuizAttempt.Option2, nextQuizAttempt.Option3, nextQuizAttempt.Option4 },
            Samples = samples
        };
    }


    public async Task<QuizResultDto> SubmitQuizAnswerAsync(QuizAnswerDto answerDto)
    {
        var userId = _currentUser.Id.Value;

        bool isCorrect = await _quizService.EvaluateQuizAttemptAsync(
            answerDto.QuizAttemptId,
            answerDto.SelectedOptionIndex);

        var quizAttempt = await _quizAttemptRepository.GetAsync(answerDto.QuizAttemptId);

        var word = await _wordRepository.GetAsync(quizAttempt.WordId.Value);

        await _spacedRepetitionService.ProcessQuizAttemptAsync(
            userId,
            quizAttempt.WordId.Value,
            isCorrect);

        int correctOptionIndex = -1;
        if (quizAttempt.Option1 == quizAttempt.CorrectOption) correctOptionIndex = 0;
        else if (quizAttempt.Option2 == quizAttempt.CorrectOption) correctOptionIndex = 1;
        else if (quizAttempt.Option3 == quizAttempt.CorrectOption) correctOptionIndex = 2;
        else if (quizAttempt.Option4 == quizAttempt.CorrectOption) correctOptionIndex = 3;

        IncrementQuestionsAnswered();

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

    public async Task<QuizQuestionDto> SkipToNextQuestionAsync(QuizAnswerDto answerDto)
    {
        var userId = _currentUser.Id.Value;

        bool isCorrect = await _quizService.EvaluateQuizAttemptAsync(
            answerDto.QuizAttemptId,
            answerDto.SelectedOptionIndex);

        var quizAttempt = await _quizAttemptRepository.GetAsync(answerDto.QuizAttemptId);

        await _spacedRepetitionService.ProcessQuizAttemptAsync(
            userId,
            quizAttempt.WordId.Value,
            isCorrect);

        IncrementQuestionsAnswered();

        return await GetNextQuizQuestionAsync();
    }

    public async Task<int> GetQuestionResolveCountByUserId(Guid userId)
    {
        var questionCount = await _quizAttemptRepository.GetQuestionResolveCountByUserIdAsync(userId);
        return questionCount;
    }

    public async Task<int> GetTrueQuestionResolveCountByUserId(Guid userId)
    {
        var trueQuestionCount = await _quizAttemptRepository.GetTrueQuestionResolveCountByUserId(userId);
        return trueQuestionCount;
    }

    public async Task<int> GetFalseQuestionResolveCountByUserId(Guid userId)
    {
        var falseQuestionCount = await _quizAttemptRepository.GetFalseQuestionResolveCountByUserId(userId);
        return falseQuestionCount;
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
