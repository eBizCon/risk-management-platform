using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Queries;

public record GetApplicationQuery(int ApplicationId, string UserEmail, string UserRole) : IQuery<ApplicationResponse>;

public class GetApplicationHandler : IQueryHandler<GetApplicationQuery, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;

    public GetApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(GetApplicationQuery query,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(query.ApplicationId, ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (query.UserRole != "processor" && application.CreatedBy != query.UserEmail)
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}