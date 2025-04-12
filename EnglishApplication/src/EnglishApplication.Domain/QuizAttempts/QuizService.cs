using EnglishApplication.Words;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace EnglishApplication.QuizAttempts;

public class QuizService
{
    private readonly IWordRepository _wordRepository;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly Random _random;
    private readonly ICurrentUser _currentUser;

    public QuizService(
        IWordRepository wordRepository,
        IQuizAttemptRepository quizAttemptRepository,
        ICurrentUser currentUser)
    {
        _wordRepository = wordRepository;
        _quizAttemptRepository = quizAttemptRepository;
        _random = new Random();
        _currentUser = currentUser;
    }

    // Belirli bir kelime için 4 şıklı quiz oluştur
    public async Task<List<QuizOption>> GenerateQuizOptionsAsync(Guid wordId, int optionCount = 4)
    {
        // Doğru kelimeyi al
        var correctWord = await _wordRepository.GetWordById(wordId);
        if (correctWord == null)
        {
            throw new Exception($"Word with id {wordId} not found");
        }

        // Doğru cevap için bir seçenek oluştur
        var options = new List<QuizOption>
        {
            new QuizOption(correctWord.TurkishWordName, true)
        };

        // Diğer kelimeleri al (yanlış şıklar için)
        var otherWords = await _wordRepository.GetWordsByUserId(_currentUser.GetId());
        otherWords.Remove(correctWord); // Doğru kelimeyi listeden çıkar

        // Eğer yeterli kelime yoksa hata fırlat veya yapay şıklar oluştur
        if (otherWords.Count < optionCount - 1)
        {
            // Seçenek 1: Hata fırlatmak
            // throw new Exception($"Not enough words in the database to generate {optionCount} options");

            // Seçenek 2: Yapay şıklar oluşturmak
            var fakeOptions = GenerateFakeOptions(correctWord.TurkishWordName, optionCount - 1 - otherWords.Count);
            foreach (var fakeOption in fakeOptions)
            {
                options.Add(new QuizOption(fakeOption, false));
            }
        }

        // Rastgele seçilen farklı kelimeleri şık olarak ekle
        var randomWords = otherWords.OrderBy(_ => _random.Next()).Take(Math.Min(optionCount - 1, otherWords.Count)).ToList();

        foreach (var word in randomWords)
        {
            options.Add(new QuizOption(word.TurkishWordName, false));
        }

        // Şıkları karıştır
        return options.OrderBy(_ => _random.Next()).ToList();
    }

    // Yapay şıklar oluşturmak için yardımcı metod
    private List<string> GenerateFakeOptions(string correctOption, int count)
    {
        var result = new List<string>();

        // Basit yapay şıklar oluştur
        for (int i = 0; i < count; i++)
        {
            // Örnek olarak doğru cevabın sonuna rastgele harf ekleyebilirsiniz
            // Gerçek uygulamada daha akıllı bir algoritma kullanın
            result.Add($"{correctOption}_{_random.Next(1000)}");
        }

        return result;
    }

    // Quiz denemesi oluştur
    public async Task<QuizAttempt> CreateQuizAttemptAsync(Guid userId, Guid wordId)
    {
        // 4 şıklı quiz oluştur
        var options = await GenerateQuizOptionsAsync(wordId);

        // Şık sayısının 4 olduğunu kontrol et
        if (options.Count < 4)
        {
            throw new Exception("Quiz generation failed: Not enough options generated");
        }

        // Quiz denemesi oluştur
        var attempt = new QuizAttempt
        {
            UserId = userId,
            WordId = wordId,
            CorrectOption = options.First(o => o.IsCorrect).Text,
            Option1 = options[0].Text,
            Option2 = options[1].Text,
            Option3 = options[2].Text,
            Option4 = options[3].Text,
            IsCorrect = false, // Başlangıçta false, cevap verildiğinde güncellenecek
            SelectedOptionIndex = -1 // Henüz seçim yapılmadı
        };

        return await _quizAttemptRepository.InsertAsync(attempt);
    }

    // Quiz denemesini değerlendir
    public async Task<bool> EvaluateQuizAttemptAsync(Guid attemptId, int selectedOptionIndex)
    {
        var attempt = await _quizAttemptRepository.GetAsync(attemptId);
        if (attempt == null)
        {
            throw new Exception($"Quiz attempt with id {attemptId} not found");
        }

        // Kullanıcının seçtiği şık
        string selectedOption;
        switch (selectedOptionIndex)
        {
            case 0: selectedOption = attempt.Option1; break;
            case 1: selectedOption = attempt.Option2; break;
            case 2: selectedOption = attempt.Option3; break;
            case 3: selectedOption = attempt.Option4; break;
            default: throw new Exception("Invalid option index");
        }

        // Cevabın doğru olup olmadığını kontrol et
        attempt.IsCorrect = selectedOption == attempt.CorrectOption;
        attempt.SelectedOptionIndex = selectedOptionIndex;

        // Güncelle ve sonucu döndür
        await _quizAttemptRepository.UpdateAsync(attempt);
        return attempt.IsCorrect;
    }

}