using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetProcessorApplicationsQuery(string? Status = null, int Page = 1, int PageSize = 10)
    : IQuery<ProcessorApplicationsResponse>;

public class
    GetProcessorApplicationsHandler : IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ICustomerReadModelRepository _customerReadModelRepository;

    public GetProcessorApplicationsHandler(IApplicationRepository repository,
        ICustomerReadModelRepository customerReadModelRepository)
    {
        _repository = repository;
        _customerReadModelRepository = customerReadModelRepository;
    }

    public async Task<Result<ProcessorApplicationsResponse>> HandleAsync(GetProcessorApplicationsQuery query,
        CancellationToken ct = default)
    {
        ApplicationStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status))
            status = ApplicationStatus.From(query.Status);

        var (items, totalCount) = await _repository.GetAllPaginatedAsync(status, query.Page, query.PageSize, ct);
        var stats = await _repository.GetProcessorStatsAsync(ct);

        var applications = ApplicationMapper.ToResponseArray(items);

        var customerIds = applications.Select(a => a.CustomerId).Distinct();
        var names = await _customerReadModelRepository.GetCustomerNamesAsync(customerIds, ct);
        foreach (var app in applications)
            if (names.TryGetValue(app.CustomerId, out var name))
                app.CustomerName = name;

        var response = new ProcessorApplicationsResponse
        {
            Applications = applications,
            StatusFilter = query.Status,
            Stats = new ProcessorStatsDto
            {
                Total = stats.Total,
                Submitted = stats.Submitted,
                Approved = stats.Approved,
                Rejected = stats.Rejected
            },
            Pagination = new PaginationInfo
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            }
        };

        return Result<ProcessorApplicationsResponse>.Success(response);
    }
}