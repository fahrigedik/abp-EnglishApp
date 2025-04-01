using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishApplication.Words;

public class CreateUpdateWordDto
{
    [Required]
    [StringLength(128)]
    public string EnglishWordName { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string TurkishWordName { get; set; } = string.Empty;

    public string Picture { get; set; } = string.Empty;
}