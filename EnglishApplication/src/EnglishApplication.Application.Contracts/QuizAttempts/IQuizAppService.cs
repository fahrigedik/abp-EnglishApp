using EnglishApplication.QuizAttempts.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EnglishApplication.QuizAttempts;

public interface IQuizAppService : IApplicationService
{
    Task<QuizQuestionDto> GetNextQuizQuestionAsync();
    Task<QuizResultDto> SubmitQuizAnswerAsync(QuizAnswerDto answerDto);
}