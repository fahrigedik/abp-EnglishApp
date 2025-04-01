using System;

namespace EnglishApplication.Words;

public class WordDto
{
    public Guid Id { get; set; }
    public string EnglishWordName { get; set; }
    public string TurkishWordName { get; set; }
    public string Picture { get; set; }
}