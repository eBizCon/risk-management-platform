using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Queries;

public record GetInquiriesQuery(int ApplicationId, string UserEmail, string UserRole)
    : IQuery<List<ApplicationInquiry>>;

public class GetInquiriesHandler : IQueryHandler<GetInquiriesQuery, List<ApplicationInquiry>>
{
    private readonly IApplicationRepository _repository;

    public GetInquiriesHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ApplicationInquiry>>> HandleAsync(GetInquiriesQuery query,
        CancellationToken ct = default)
    {
        var appId = new AppId(query.ApplicationId);
        var application = await _repository.GetByIdAsync(appId, ct);
        if (application is null)
            return Result<List<ApplicationInquiry>>.NotFound("Antrag nicht gefunden");

        if (query.UserRole != "processor" && application.CreatedBy != EmailAddress.Create(query.UserEmail))
            return Result<List<ApplicationInquiry>>.Forbidden("Zugriff verweigert");

        var inquiries = await _repository.GetInquiriesAsync(appId, ct);
        return Result<List<ApplicationInquiry>>.Success(inquiries);
    }
}