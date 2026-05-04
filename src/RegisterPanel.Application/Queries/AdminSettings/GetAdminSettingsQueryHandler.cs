using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.AdminSettings;
using RegisterPanel.Domain.Interfaces.Repositories;
using MediatR;
using AdminSettingsEntity = RegisterPanel.Domain.Entities.AdminSettings;

namespace RegisterPanel.Application.Queries.AdminSettings;

public sealed class GetAdminSettingsQueryHandler
    : IRequestHandler<GetAdminSettingsQuery, Result<AdminSettingsDto>>
{
    private readonly IAdminSettingsRepository _repository;

    public GetAdminSettingsQueryHandler(IAdminSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AdminSettingsDto>> Handle(
        GetAdminSettingsQuery request,
        CancellationToken cancellationToken)
    {
        AdminSettingsEntity settings = await _repository.GetAsync(cancellationToken);

        AdminSettingsDto dto = new(
            MaxSimultaneousTrainers:        settings.MaxSimultaneousTrainers,
            MaxSimultaneousClientsPerTrainer: settings.MaxSimultaneousClientsPerTrainer,
            MinHoursBeforeCancellation:     settings.MinHoursBeforeCancellation,
            AllowLateCancelWithPenalty:     settings.AllowLateCancelWithPenalty,
            UpdatedAt:                      settings.UpdatedAt,
            UpdatedByUserId:                settings.UpdatedByUserId
        );

        return Result<AdminSettingsDto>.Success(dto);
    }
}
