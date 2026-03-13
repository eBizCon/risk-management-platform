using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Queries;

public record GetProcessorApplicationsQuery(string? Status = null, int Page = 1, int PageSize = 10) : IQuery<ProcessorApplicationsResponse>;

public class GetProcessorApplicationsHandler : IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse>
{
    private readonly IApplicationRepository _repository;

    public GetProcessorApplicationsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProcessorApplicationsResponse>> HandleAsync(GetProcessorApplicationsQuery query, CancellationToken ct = default)
    {
        ApplicationStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status))
            status = ApplicationStatus.From(query.Status);

        var (items, totalCount) = await _repository.GetAllPaginatedAsync(status, query.Page, query.PageSize, ct);
        var stats = await _repository.GetProcessorStatsAsync(ct);

        var response = new ProcessorApplicationsResponse
        {
            Applications = ApplicationMapper.ToResponseArray(items),
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
