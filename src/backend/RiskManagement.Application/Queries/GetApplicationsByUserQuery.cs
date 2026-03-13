using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetApplicationsByUserQuery(string UserEmail, string? Status = null) : IQuery<ApplicationResponse[]>;

public class GetApplicationsByUserHandler : IQueryHandler<GetApplicationsByUserQuery, ApplicationResponse[]>
{
    private readonly IApplicationRepository _repository;

    public GetApplicationsByUserHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApplicationResponse[]>> HandleAsync(GetApplicationsByUserQuery query,
        CancellationToken ct = default)
    {
        ApplicationStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status))
            status = ApplicationStatus.From(query.Status);

        var applications = await _repository.GetByUserAsync(EmailAddress.Create(query.UserEmail), status, ct);
        return Result<ApplicationResponse[]>.Success(ApplicationMapper.ToResponseArray(applications));
    }
}