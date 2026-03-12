using FluentValidation;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Validation;

public class ProcessorDecisionValidator : AbstractValidator<ProcessorDecisionDto>
{
    public ProcessorDecisionValidator()
    {
        RuleFor(x => x.Decision)
            .Must(x => x == "approved" || x == "rejected")
            .WithMessage("Bitte wählen Sie eine Entscheidung");

        RuleFor(x => x.Comment)
            .Must((dto, comment) =>
                dto.Decision != "rejected" ||
                (!string.IsNullOrWhiteSpace(comment) && comment.Trim().Length > 0))
            .WithMessage("Bei Ablehnung ist eine Begründung erforderlich");
    }
}
