using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EnglishApplication.UserSettings;

public interface IUserSettingRepository : IRepository<UserSetting, Guid>
{
    public Task<bool> GetIsWordSetLoad(Guid userId);
}