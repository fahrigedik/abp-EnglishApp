using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EnglishApplication.LearningProgress;

public interface ILearningProgressService : IApplicationService
{
    Task<List<LearningProgressDto>> GetLearningProgressAsync(int days = 7);
}