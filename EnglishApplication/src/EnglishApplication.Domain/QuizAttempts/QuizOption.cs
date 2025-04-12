namespace EnglishApplication.QuizAttempts;

public class QuizOption
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }

    public QuizOption() { }

    public QuizOption(string text, bool isCorrect = false)
    {
        Text = text;
        IsCorrect = isCorrect;
    }

}