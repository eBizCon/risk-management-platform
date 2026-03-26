using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetApplicationsByUserQuery(string UserEmail, string? Status = null) : IQuery<ApplicationResponse[]>;

public class GetApplicationsByUserHandler : IQueryHandler<GetApplicationsByUserQuery, ApplicationResponse[]>
{
    private readonly IApplicationRepository _repository;
    private readonly ICustomerReadModelRepository _customerReadModelRepository;

    public GetApplicationsByUserHandler(IApplicationRepository repository,
        ICustomerReadModelRepository customerReadModelRepository)
    {
        _repository = repository;
        _customerReadModelRepository = customerReadModelRepository;
    }

    public async Task<Result<ApplicationResponse[]>> HandleAsync(GetApplicationsByUserQuery query,
        CancellationToken ct = default)
    {
        ApplicationStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status))
            status = ApplicationStatus.From(query.Status);

        var applications = await _repository.GetByUserAsync(EmailAddress.Create(query.UserEmail), status, ct);
        var responses = ApplicationMapper.ToResponseArray(applications);

        var customerIds = responses.Select(r => r.CustomerId).Distinct();
        var names = await _customerReadModelRepository.GetCustomerNamesAsync(customerIds, ct);
        foreach (var response in responses)
            if (names.TryGetValue(response.CustomerId, out var name))
                response.CustomerName = name;

        return Result<ApplicationResponse[]>.Success(responses);
    }
}