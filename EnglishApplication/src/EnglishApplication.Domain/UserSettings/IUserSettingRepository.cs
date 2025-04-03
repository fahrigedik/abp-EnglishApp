using System;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.UserSettings;

public interface IUserSettingRepository : IRepository<UserSetting, Guid>
{
    
}