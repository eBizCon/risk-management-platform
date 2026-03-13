using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Queries;

public record GetInquiriesQuery(int ApplicationId, string UserEmail, string UserRole);

public class GetInquiriesHandler : IQueryHandler<GetInquiriesQuery, List<ApplicationInquiry>>
{
    private readonly IApplicationRepository _repository;

    public GetInquiriesHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ApplicationInquiry>>> HandleAsync(GetInquiriesQuery query, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(query.ApplicationId, ct);
        if (application is null)
            return Result<List<ApplicationInquiry>>.NotFound("Antrag nicht gefunden");

        if (query.UserRole != "processor" && application.CreatedBy != query.UserEmail)
            return Result<List<ApplicationInquiry>>.Forbidden("Zugriff verweigert");

        var inquiries = await _repository.GetInquiriesAsync(query.ApplicationId, ct);
        return Result<List<ApplicationInquiry>>.Success(inquiries);
    }
}
