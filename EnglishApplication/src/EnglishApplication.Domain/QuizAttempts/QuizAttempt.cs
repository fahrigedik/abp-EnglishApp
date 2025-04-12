using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace EnglishApplication.QuizAttempts;
public class QuizAttempt : AuditedAggregateRoot<Guid>
{
    public Guid? WordId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsCorrect { get; set; } // Kullanıcının doğru cevaplayıp cevaplayamadığı
    public int SelectedOptionIndex { get; set; } // Kullanıcının seçtiği şık (0-3)
    public string CorrectOption { get; set; } // Doğru cevap
    public string Option1 { get; set; } // 1. şık
    public string Option2 { get; set; } // 2. şık
    public string Option3 { get; set; } // 3. şık
    public string Option4 { get; set; } // 4. şık

}