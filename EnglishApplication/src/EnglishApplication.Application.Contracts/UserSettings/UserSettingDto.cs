using System;

namespace EnglishApplication.UserSettings;

public class UserSettingDto
{
    public Guid Id { get; set; }
    public int QuestionCount { get; set; } = 10;
    public Guid UserId { get; set; }
    public bool IsWordSetLoad { get; set; } = false;
}