using RegisterPanel.Application.Commands.AdminSettings;
using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.AdminSettings;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Application.Queries.AdminSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RegisterPanel.Api.Controllers.V1;

/// <summary>Request body for PUT /admin/settings.</summary>
public sealed record UpdateAdminSettingsRequest(
    int MaxSimultaneousTrainers,
    int MaxSimultaneousClientsPerTrainer,
    int MinHoursBeforeCancellation,
    bool AllowLateCancelWithPenalty
);

[Route("api/v1/admin/settings")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminSettingsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AdminSettingsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Returns the current system-wide configuration.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        Result<AdminSettingsDto> result = await _mediator.Send(new GetAdminSettingsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Updates the system-wide configuration. Only the Admin role is allowed.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateAdminSettingsRequest request,
        CancellationToken ct)
    {
        UpdateAdminSettingsCommand command = new(
            MaxSimultaneousTrainers:        request.MaxSimultaneousTrainers,
            MaxSimultaneousClientsPerTrainer: request.MaxSimultaneousClientsPerTrainer,
            MinHoursBeforeCancellation:     request.MinHoursBeforeCancellation,
            AllowLateCancelWithPenalty:     request.AllowLateCancelWithPenalty,
            UpdatedByUserId:                _currentUser.UserId.ToString()
        );

        Result result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}
