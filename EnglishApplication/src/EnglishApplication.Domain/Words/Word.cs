using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.Words;
public class Word : AuditedAggregateRoot<Guid>
{
    public string EnglishWordName { get; set; }
    public string TurkishWordName { get; set; }
    public string Picture { get; set; }
    public Guid UserId { get; set; }
}