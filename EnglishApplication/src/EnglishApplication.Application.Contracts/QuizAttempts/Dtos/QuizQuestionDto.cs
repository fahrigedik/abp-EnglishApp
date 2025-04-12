using System.Collections.Generic;
using System;

namespace EnglishApplication.QuizAttempts.Dtos;

public class QuizQuestionDto
{
    public Guid QuizAttemptId { get; set; }
    public Guid WordId { get; set; }
    public string EnglishWord { get; set; }
    public string Picture { get; set; }
    public List<string> Options { get; set; }
}