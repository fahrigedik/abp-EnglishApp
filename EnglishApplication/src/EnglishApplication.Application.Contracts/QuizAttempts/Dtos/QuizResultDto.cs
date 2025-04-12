using System;

namespace EnglishApplication.QuizAttempts.Dtos;

public class QuizResultDto
{
    public Guid QuizAttemptId { get; set; }
    public string EnglishWord { get; set; }
    public string CorrectTranslation { get; set; }
    public string SelectedTranslation { get; set; }
    public bool IsCorrect { get; set; }
    public int CorrectOptionIndex { get; set; }
    public int SelectedOptionIndex { get; set; }
}