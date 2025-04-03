using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EnglishApplication.UserSettings;

public class CreateUpdateUserSettingDto
{
    [Required]
    public int QuestionCount { get; set; } = 10;

    [Required]
    [HiddenInput]
    public Guid UserId { get; set; }
}