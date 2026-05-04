using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.AdminSettings;
using MediatR;

namespace RegisterPanel.Application.Queries.AdminSettings;

public sealed record GetAdminSettingsQuery : IRequest<Result<AdminSettingsDto>>;
