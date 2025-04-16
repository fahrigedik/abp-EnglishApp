using System;

namespace EnglishApplication.LearningProgress;
public class LearningProgressDto
{
    public DateTime Date { get; set; }
    public int LearnedWordsCount { get; set; }
    public int CorrectAnswersCount { get; set; }
}
