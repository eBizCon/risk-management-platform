using CustomerManagement.Domain.Events;
using CustomerManagement.Domain.ValueObjects;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Domain.Aggregates.CustomerAggregate;

public class Customer : AggregateRoot<CustomerId>
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public EmailAddress? Email { get; private set; }
    public PhoneNumber Phone { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public Address Address { get; private set; } = null!;
    public EmploymentStatus EmploymentStatus { get; private set; } = EmploymentStatus.Employed;
    public CustomerStatus Status { get; private set; } = null!;
    public EmailAddress CreatedBy { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer()
    {
    }

    public static Customer Create(
        string firstName,
        string lastName,
        EmailAddress? email,
        PhoneNumber phone,
        DateOnly dateOfBirth,
        Address address,
        EmploymentStatus employmentStatus,
        EmailAddress createdBy)
    {
        GuardName(firstName, "Vorname");
        GuardName(lastName, "Nachname");

        var customer = new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            Address = address,
            EmploymentStatus = employmentStatus,
            Status = CustomerStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        return customer;
    }

    public void NotifyCreated()
    {
        AddDomainEvent(new CustomerCreatedEvent(Id, FirstName, LastName));
    }

    public void Update(
        string firstName,
        string lastName,
        EmailAddress? email,
        PhoneNumber phone,
        DateOnly dateOfBirth,
        Address address,
        EmploymentStatus employmentStatus)
    {
        GuardActiveStatus();
        GuardName(firstName, "Vorname");
        GuardName(lastName, "Nachname");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Address = address;
        EmploymentStatus = employmentStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerUpdatedEvent(Id, FirstName, LastName, Status.Value));
    }

    public void Archive()
    {
        GuardActiveStatus();
        Status = CustomerStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CustomerArchivedEvent(Id));
    }

    public void Activate()
    {
        if (Status != CustomerStatus.Archived)
            throw new DomainException("Nur archivierte Kunden können aktiviert werden");

        Status = CustomerStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CustomerActivatedEvent(Id));
    }

    public void Delete()
    {
        AddDomainEvent(new CustomerDeletedEvent(Id));
    }

    private void GuardActiveStatus()
    {
        if (Status != CustomerStatus.Active)
            throw new DomainException("Kunde muss aktiv sein für diese Aktion");
    }

    private static void GuardName(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} darf nicht leer sein");
        if (value.Trim().Length < 2)
            throw new DomainException($"{fieldName} muss mindestens 2 Zeichen lang sein");
        if (value.Trim().Length > 50)
            throw new DomainException($"{fieldName} darf maximal 50 Zeichen lang sein");
    }
}