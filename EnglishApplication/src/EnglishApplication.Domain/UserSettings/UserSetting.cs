using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.UserSettings;

public class UserSetting : AuditedAggregateRoot<Guid>
{
    public int QuestionCount { get; set; } = 10;
    public Guid UserId { get; set; }
    public bool IsWordSetLoad { get; set; } = false;
}