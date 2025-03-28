using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.WordSamples;

public class WordSample : AuditedAggregateRoot<Guid>
{
    public Guid WordId { get; set; }
    public string Sample { get; set; }
}