using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record DeleteApplicationCommand(int ApplicationId, string UserEmail) : ICommand<bool>;

public class DeleteApplicationHandler : ICommandHandler<DeleteApplicationCommand, bool>
{
    private readonly IApplicationRepository _repository;

    public DeleteApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> HandleAsync(DeleteApplicationCommand command, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<bool>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != EmailAddress.Create(command.UserEmail))
            return Result<bool>.Forbidden("Zugriff verweigert");

        application.Delete();
        await _repository.RemoveAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
