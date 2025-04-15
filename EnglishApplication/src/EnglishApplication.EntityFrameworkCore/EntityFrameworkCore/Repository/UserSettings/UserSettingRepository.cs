using System;
using System.Threading.Tasks;
using EnglishApplication.UserSettings;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishApplication.EntityFrameworkCore.Repository.UserSettings;

public class UserSettingRepository : EfCoreRepository<EnglishApplicationDbContext, UserSetting, Guid>, IUserSettingRepository
{
    public UserSettingRepository(IDbContextProvider<EnglishApplicationDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }

    public async Task<bool> GetIsWordSetLoad(Guid userId)
    {
        var query = await GetQueryableAsync();
        var userSetting = await query.FirstOrDefaultAsync(x => x.UserId == userId);
        return userSetting?.IsWordSetLoad ?? false;
    }
}