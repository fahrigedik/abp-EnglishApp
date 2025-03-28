using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.WordDetails;
public class WordDetail : AuditedAggregateRoot<Guid>
{
    public int TrueCount { get; set; } = 0;
    public DateTime? NextDate { get; set; } = null;
    public bool? IsLearn { get; set; }
    public Guid WordId { get; set; }
}
