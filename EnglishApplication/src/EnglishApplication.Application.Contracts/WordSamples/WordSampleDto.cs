using System;

namespace EnglishApplication.WordSamples;

public class WordSampleDto
{
    public Guid Id { get; set; }
    public Guid WordId { get; set; }
    public string Sample { get; set; }
}