using RegisterPanel.Application.Common;
using RegisterPanel.Application.Interfaces;
using RegisterPanel.Domain.Interfaces.Repositories;
using MediatR;
using AdminSettingsEntity = RegisterPanel.Domain.Entities.AdminSettings;

namespace RegisterPanel.Application.Commands.AdminSettings;

public sealed class UpdateAdminSettingsCommandHandler : IRequestHandler<UpdateAdminSettingsCommand, Result>
{
    private readonly IAdminSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdminSettingsCommandHandler(IAdminSettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateAdminSettingsCommand command,
        CancellationToken cancellationToken)
    {
        AdminSettingsEntity settings = await _repository.GetAsync(cancellationToken);

        settings.Update(
            maxTrainers:                command.MaxSimultaneousTrainers,
            maxClientsPerTrainer:       command.MaxSimultaneousClientsPerTrainer,
            minHoursBeforeCancellation: command.MinHoursBeforeCancellation,
            allowLateCancelWithPenalty: command.AllowLateCancelWithPenalty,
            updatedByUserId:            command.UpdatedByUserId
        );

        await _repository.UpdateAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
