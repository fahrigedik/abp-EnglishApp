using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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


[Authorize(Roles = "student, admin")]
public class UserSettingService : ApplicationService, IUserSettingService
{
    private readonly IRepository<UserSetting, Guid> _userSettingRepository;
    private readonly IUserSettingRepository userSettingsRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Define the session key constant (same as in QuizAppService)
    private const string SESSION_QUESTION_COUNT_KEY = "QuizQuestionCount";

    public UserSettingService(
        IRepository<UserSetting, Guid> userSettingRepository,
        IUserSettingRepository userSettingsRepository,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _userSettingRepository = userSettingRepository;
        _currentUser = currentUser;
        this.userSettingsRepository = userSettingsRepository;
        _httpContextAccessor = httpContextAccessor;
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

            // Update session with default question count
            UpdateSessionQuestionCount(10);
        }

        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<UserSettingDto> CreateAsync(CreateUpdateUserSettingDto input)
    {
        var userSetting = ObjectMapper.Map<CreateUpdateUserSettingDto, UserSetting>(input);
        await _userSettingRepository.InsertAsync(userSetting);

        // Update session with new question count
        UpdateSessionQuestionCount(userSetting.QuestionCount);

        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<UserSettingDto> UpdateAsync(Guid id, CreateUpdateUserSettingDto input)
    {
        var userSetting = await _userSettingRepository.GetAsync(id);
        ObjectMapper.Map(input, userSetting);
        await _userSettingRepository.UpdateAsync(userSetting);

        // Update session with updated question count if it's the current user's settings
        if (_currentUser.Id.HasValue && userSetting.UserId == _currentUser.Id.Value)
        {
            UpdateSessionQuestionCount(userSetting.QuestionCount);
        }

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

        // Update session with new question count
        UpdateSessionQuestionCount(userSetting.QuestionCount);

        return ObjectMapper.Map<UserSetting, UserSettingDto>(userSetting);
    }

    public async Task<bool> GetIsWordSetLoad(Guid userId)
    {
        return await userSettingsRepository.GetIsWordSetLoad(userId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _userSettingRepository.DeleteAsync(id);
    }

    // Helper method to update session question count
    private void UpdateSessionQuestionCount(int questionCount)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Session.Set(
                SESSION_QUESTION_COUNT_KEY,
                System.Text.Encoding.UTF8.GetBytes(questionCount.ToString()));
        }
    }
}
