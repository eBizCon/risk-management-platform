using CustomerManagement.Application.DTOs;
using FluentValidation;

namespace CustomerManagement.Application.Validation;

public class CustomerUpdateValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Vorname ist erforderlich")
            .MinimumLength(2).WithMessage("Vorname muss mindestens 2 Zeichen lang sein")
            .MaximumLength(50).WithMessage("Vorname darf maximal 50 Zeichen lang sein");

        RuleFor(x => x.LastName).NotEmpty().WithMessage("Nachname ist erforderlich")
            .MinimumLength(2).WithMessage("Nachname muss mindestens 2 Zeichen lang sein")
            .MaximumLength(50).WithMessage("Nachname darf maximal 50 Zeichen lang sein");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Ungültige E-Mail-Adresse");

        RuleFor(x => x.Phone).NotEmpty().WithMessage("Telefonnummer ist erforderlich");

        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Geburtsdatum ist erforderlich")
            .Must(BeAValidDate).WithMessage("Ungültiges Datumsformat (erwartet: YYYY-MM-DD)");

        RuleFor(x => x.Street).NotEmpty().WithMessage("Straße ist erforderlich");
        RuleFor(x => x.City).NotEmpty().WithMessage("Stadt ist erforderlich");
        RuleFor(x => x.ZipCode).NotEmpty().WithMessage("PLZ ist erforderlich");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Land ist erforderlich");
    }

    private static bool BeAValidDate(string dateString)
    {
        return DateOnly.TryParse(dateString, out _);
    }
}
