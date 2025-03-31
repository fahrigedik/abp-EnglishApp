using System;
using System.Data;
using System.Threading.Tasks;
using EnglishApplication.UserSettings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace EnglishApplication.Identity;



public class UserCreatedEventHandler : ILocalEventHandler<EntityCreatedEventData<IdentityUser>>, ITransientDependency
{
    private readonly IRepository<UserSetting, Guid> _userSettingsRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IUnitOfWork _unitOfWork;


    public UserCreatedEventHandler(
        IRepository<UserSetting, Guid> userSettingsRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IUnitOfWork unitOfWork)
    {
        _userSettingsRepository = userSettingsRepository;
        _unitOfWorkManager = unitOfWorkManager;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<IdentityUser> eventData)
    { 
        // Skip if the user already has settings
        if (await _userSettingsRepository.AnyAsync(x => x.UserId == eventData.Entity.Id))
        {
            return;
        }

        // Create default settings for the new user
        using var uow = _unitOfWorkManager.Begin(isTransactional: true,
            isolationLevel: IsolationLevel.RepeatableRead,
            timeout: 30
        );

        var userSetting = new UserSetting
        {
            UserId = eventData.Entity.Id,
            QuestionCount = 10 // Default value
        };

        await _userSettingsRepository.InsertAsync(userSetting);
        await uow.CompleteAsync();
    }
}