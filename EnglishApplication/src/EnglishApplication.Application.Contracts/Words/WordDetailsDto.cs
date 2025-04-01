using System;

namespace EnglishApplication.Words;

public class WordDetailsDto
{
    public Guid Id { get; set; }
    public string EnglishWordName { get; set; }
    public string TurkishWordName { get; set; }
    public string Picture { get; set; }
    public Guid UserId { get; set; }
    public int TrueCount { get; set; } = 0;
    public DateTime? NextDate { get; set; } = null;
    public bool? IsLearn { get; set; }
    public Guid WordId { get; set; }
}