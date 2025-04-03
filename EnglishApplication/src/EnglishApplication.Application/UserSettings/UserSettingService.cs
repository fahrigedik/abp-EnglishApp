using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace EnglishApplication.UserSettings;

public class UserSettingService : ApplicationService, IUserSettingService
{
    private readonly IRepository<UserSetting, Guid> _userSettingRepository;
    private readonly ICurrentUser _currentUser;

    public UserSettingService(
        IRepository<UserSetting, Guid> userSettingRepository,
        ICurrentUser currentUser)
    {
        _userSettingRepository = userSettingRepository;
        _currentUser = currentUser;
    }

    public async Task<UserSettingDto> GetAsync(Guid id)
    {
        var userSetting = await _userSettingRepository.GetAsync(id);
        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<PagedResultDto<UserSettingDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _userSettingRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(x => x.UserId)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var userSettings = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<UserSettingDto>(
            totalCount,
            ObjectMapper.Map<List<UserSetting>, List<UserSettingDto>>(userSettings)
        );
    }

    public async Task<UserSettingDto> GetByCurrentUserAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new UserFriendlyException(L["UserNotLoggedIn"]);
        }

        var userId = _currentUser.Id.Value;
        var queryable = await _userSettingRepository.GetQueryableAsync();
        var userSetting = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.UserId == userId));

        if (userSetting == null)
        {
            // Create default settings if not exists
            userSetting = new UserSetting
            {
                UserId = userId,
                QuestionCount = 10
            };
            await _userSettingRepository.InsertAsync(userSetting, true);
        }

        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<UserSettingDto> CreateAsync(CreateUpdateUserSettingDto input)
    {
        var userSetting = ObjectMapper.Map<CreateUpdateUserSettingDto, UserSetting>(input);
        await _userSettingRepository.InsertAsync(userSetting);
        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<UserSettingDto> UpdateAsync(Guid id, CreateUpdateUserSettingDto input)
    {
        var userSetting = await _userSettingRepository.GetAsync(id);
        ObjectMapper.Map(input, userSetting);
        await _userSettingRepository.UpdateAsync(userSetting);
        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<UserSettingDto> UpdateCurrentUserSettingsAsync(CreateUpdateUserSettingDto input)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new UserFriendlyException(L["UserNotLoggedIn"]);
        }

        var userId = _currentUser.Id.Value;
        var queryable = await _userSettingRepository.GetQueryableAsync();
        var userSetting = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.UserId == userId));

        if (userSetting == null)
        {
            // Create settings if not exists
            userSetting = ObjectMapper.Map<CreateUpdateUserSettingDto, UserSetting>(input);
            userSetting.UserId = userId;
            await _userSettingRepository.InsertAsync(userSetting, true);
        }
        else
        {
            // Update existing settings
            ObjectMapper.Map(input, userSetting);
            await _userSettingRepository.UpdateAsync(userSetting, true);
        }

        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _userSettingRepository.DeleteAsync(id);
    }
}