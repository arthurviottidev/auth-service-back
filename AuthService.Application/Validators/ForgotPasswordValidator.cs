using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");
    }
}