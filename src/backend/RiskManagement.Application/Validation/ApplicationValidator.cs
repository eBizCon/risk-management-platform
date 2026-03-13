using FluentValidation;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Validation;

public class ApplicationValidator : AbstractValidator<ApplicationCreateDto>
{
    public ApplicationValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name muss mindestens 2 Zeichen lang sein")
            .MaximumLength(100).WithMessage("Name darf maximal 100 Zeichen lang sein");

        RuleFor(x => x.Income)
            .GreaterThan(0).WithMessage("Einkommen muss positiv sein")
            .LessThanOrEqualTo(10000000).WithMessage("Einkommen scheint unrealistisch hoch");

        RuleFor(x => x.FixedCosts)
            .GreaterThanOrEqualTo(0).WithMessage("Fixkosten können nicht negativ sein")
            .LessThanOrEqualTo(10000000).WithMessage("Fixkosten scheinen unrealistisch hoch");

        RuleFor(x => x.DesiredRate)
            .GreaterThan(0).WithMessage("Gewünschte Rate muss positiv sein")
            .LessThanOrEqualTo(1000000).WithMessage("Gewünschte Rate scheint unrealistisch hoch");

        RuleFor(x => x.EmploymentStatus)
            .Must(x => EmploymentStatus.AllValues.Contains(x))
            .WithMessage("Bitte wählen Sie einen gültigen Beschäftigungsstatus");

        RuleFor(x => x.FixedCosts)
            .LessThan(x => x.Income)
            .WithMessage("Fixkosten müssen geringer als das Einkommen sein");

        RuleFor(x => x.DesiredRate)
            .Must((dto, desiredRate) => desiredRate <= dto.Income - dto.FixedCosts)
            .WithMessage(
                "Gewünschte Rate kann nicht höher sein als das verfügbare Einkommen (Einkommen minus Fixkosten)");
    }
}

public class ApplicationUpdateValidator : AbstractValidator<ApplicationUpdateDto>
{
    public ApplicationUpdateValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name muss mindestens 2 Zeichen lang sein")
            .MaximumLength(100).WithMessage("Name darf maximal 100 Zeichen lang sein");

        RuleFor(x => x.Income)
            .GreaterThan(0).WithMessage("Einkommen muss positiv sein")
            .LessThanOrEqualTo(10000000).WithMessage("Einkommen scheint unrealistisch hoch");

        RuleFor(x => x.FixedCosts)
            .GreaterThanOrEqualTo(0).WithMessage("Fixkosten können nicht negativ sein")
            .LessThanOrEqualTo(10000000).WithMessage("Fixkosten scheinen unrealistisch hoch");

        RuleFor(x => x.DesiredRate)
            .GreaterThan(0).WithMessage("Gewünschte Rate muss positiv sein")
            .LessThanOrEqualTo(1000000).WithMessage("Gewünschte Rate scheint unrealistisch hoch");

        RuleFor(x => x.EmploymentStatus)
            .Must(x => EmploymentStatus.AllValues.Contains(x))
            .WithMessage("Bitte wählen Sie einen gültigen Beschäftigungsstatus");

        RuleFor(x => x.FixedCosts)
            .LessThan(x => x.Income)
            .WithMessage("Fixkosten müssen geringer als das Einkommen sein");

        RuleFor(x => x.DesiredRate)
            .Must((dto, desiredRate) => desiredRate <= dto.Income - dto.FixedCosts)
            .WithMessage(
                "Gewünschte Rate kann nicht höher sein als das verfügbare Einkommen (Einkommen minus Fixkosten)");
    }
}