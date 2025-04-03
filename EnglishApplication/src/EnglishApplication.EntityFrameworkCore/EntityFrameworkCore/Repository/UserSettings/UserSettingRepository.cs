using System;
using EnglishApplication.UserSettings;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.UserSettings;

public class UserSettingRepository : EfCoreRepository<EnglishApplicationDbContext, UserSetting, Guid>, IUserSettingRepository
{
    public UserSettingRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }
}