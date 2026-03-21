using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Queries;

public record GetApplicationQuery(int ApplicationId, string UserEmail, string UserRole) : IQuery<ApplicationResponse>;

public class GetApplicationHandler : IQueryHandler<GetApplicationQuery, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ICustomerReadModelRepository _customerReadModelRepository;

    public GetApplicationHandler(IApplicationRepository repository, ICustomerReadModelRepository customerReadModelRepository)
    {
        _repository = repository;
        _customerReadModelRepository = customerReadModelRepository;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(GetApplicationQuery query,
        CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(new AppId(query.ApplicationId), ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (query.UserRole != "processor" && application.CreatedBy != EmailAddress.Create(query.UserEmail))
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        var response = ApplicationMapper.ToResponse(application);
        var names = await _customerReadModelRepository.GetCustomerNamesAsync([application.CustomerId], ct);
        if (names.TryGetValue(application.CustomerId, out var name))
            response.CustomerName = name;
        return Result<ApplicationResponse>.Success(response);
    }
}