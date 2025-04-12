using System.ComponentModel.DataAnnotations;
using System;

namespace EnglishApplication.QuizAttempts.Dtos;

public class QuizAnswerDto
{
    [Required]
    public Guid QuizAttemptId { get; set; }

    [Required]
    [Range(0, 3)]
    public int SelectedOptionIndex { get; set; }
}