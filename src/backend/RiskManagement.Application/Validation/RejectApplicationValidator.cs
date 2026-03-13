using FluentValidation;
using RiskManagement.Application.DTOs;

namespace RiskManagement.Application.Validation;

public class RejectApplicationValidator : AbstractValidator<RejectApplicationDto>
{
    public RejectApplicationValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Bei Ablehnung ist eine Begründung erforderlich");
    }
}
