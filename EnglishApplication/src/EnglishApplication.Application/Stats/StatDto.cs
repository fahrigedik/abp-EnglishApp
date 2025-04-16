using System.Collections.Generic;
using EnglishApplication.Words;

namespace EnglishApplication.Stats;

public class StatDto
{
    public int TrueCount { get; set; } = 0;
    public int FalseCount { get; set; } = 0;
    public int QuestionCount { get; set; } = 0;
    public int LearnedWordCount { get; set; } = 0;
    public List<WordDto> LearnedWords { get; set; } = new List<WordDto>();

}