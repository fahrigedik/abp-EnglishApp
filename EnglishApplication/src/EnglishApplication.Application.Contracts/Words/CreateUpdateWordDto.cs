using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishApplication.Words;

public class CreateUpdateWordDto
{
    [Required]
    [StringLength(128)]
    [Display(Name = "English Word")]
    public string EnglishWordName { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    [Display(Name = "Turkish Word")]
    public string TurkishWordName { get; set; } = string.Empty;

    [Display(Name = "Picture")]
    public string Picture { get; set; } = string.Empty;
}