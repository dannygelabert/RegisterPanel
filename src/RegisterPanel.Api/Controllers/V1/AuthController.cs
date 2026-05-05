using RegisterPanel.Application.Commands.Auth.ForgotPassword;
using RegisterPanel.Application.Commands.Auth.Login;
using RegisterPanel.Application.Commands.Auth.Register;
using RegisterPanel.Application.Commands.Auth.ResendVerification;
using RegisterPanel.Application.Commands.Auth.ResetPassword;
using RegisterPanel.Application.Commands.Auth.VerifyEmail;
using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RegisterPanel.Api.Controllers.V1;

/// <summary>Request body for POST /api/v1/auth/register.</summary>
public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
);

/// <summary>Request body for POST /api/v1/auth/login.</summary>
public sealed record LoginRequest(
    string Email,
    string Password
);

/// <summary>Request body for POST /api/v1/auth/forgot-password.</summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Request body for POST /api/v1/auth/reset-password.</summary>
public sealed record ResetPasswordRequest(string UserId, string Token, string NewPassword);

/// <summary>Request body for POST /api/v1/auth/resend-verification.</summary>
public sealed record ResendVerificationRequest(string Email);

[Route("api/v1/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Registers a new user with the Client role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        RegisterCommand command = new(
            FirstName:   request.FirstName,
            LastName:    request.LastName,
            Email:       request.Email,
            Password:    request.Password,
            PhoneNumber: request.PhoneNumber);

        Result<RegisterResponse> result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>Authenticates a user and returns a JWT Bearer token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        LoginCommand command = new(
            Email:    request.Email,
            Password: request.Password);

        Result<LoginResponse> result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Verifies the user's email address using the token sent via email.</summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string userId,
        [FromQuery] string token,
        CancellationToken ct)
    {
        Result result = await _mediator.Send(new VerifyEmailCommand(userId, token), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Email verificado correctamente. Ya puedes iniciar sesión." });
    }

    /// <summary>Initiates the password recovery flow. Always returns 200 on a valid request.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        Result result = await _mediator.Send(new ForgotPasswordCommand(request.Email), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Si el email existe en nuestra plataforma, recibirás un email con instrucciones." });
    }

    /// <summary>Resets the user's password using the recovery token.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        ResetPasswordCommand command = new(
            UserId:      request.UserId,
            Token:       request.Token,
            NewPassword: request.NewPassword);

        Result result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    /// <summary>Resends the verification email. Always returns 200 on a valid request.</summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken ct)
    {
        Result result = await _mediator.Send(new ResendVerificationCommand(request.Email), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Si el email existe y no está verificado, recibirás un nuevo enlace." });
    }
}
