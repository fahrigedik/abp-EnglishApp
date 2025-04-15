using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EnglishApplication.UserSettings;

public interface IUserSettingService : ICrudAppService<UserSettingDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateUserSettingDto>
{
    Task<UserSettingDto> GetByCurrentUserAsync();
    Task<UserSettingDto> UpdateCurrentUserSettingsAsync(CreateUpdateUserSettingDto input);

    Task<bool> GetIsWordSetLoad(Guid userId);
}