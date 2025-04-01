using System;

namespace EnglishApplication.WordDetails;

public class CreateUpdateWordDetailDto
{
    public int TrueCount { get; set; } = 0;
    public DateTime? NextDate { get; set; } = null;
    public bool? IsLearn { get; set; }
    public Guid WordId { get; set; }
}