using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishApplication.WordSamples;

public class CreateUpdateWordSampleDto
{

    [Required]
    public Guid WordId { get; set; }

    public string Sample { get; set; }
}