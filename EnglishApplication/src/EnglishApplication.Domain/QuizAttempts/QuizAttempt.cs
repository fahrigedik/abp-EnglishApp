using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.QuizAttempts;
public class QuizAttempt : AuditedAggregateRoot<Guid>
{
    public Guid? WordId { get; set; }
    public Guid? UserId { get; set; }
}