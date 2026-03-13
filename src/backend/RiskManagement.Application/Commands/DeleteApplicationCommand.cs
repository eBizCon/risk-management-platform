using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record DeleteApplicationCommand(int ApplicationId, string UserEmail) : ICommand<bool>;

public class DeleteApplicationHandler : ICommandHandler<DeleteApplicationCommand, bool>
{
    private readonly IApplicationRepository _repository;
    private readonly IDispatcher _dispatcher;

    public DeleteApplicationHandler(IApplicationRepository repository, IDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<bool>> HandleAsync(DeleteApplicationCommand command, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<bool>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<bool>.Forbidden("Zugriff verweigert");

        application.Delete();
        await _repository.RemoveAsync(application, ct);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<bool>.Success(true);
    }
}
