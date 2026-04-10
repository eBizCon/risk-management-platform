using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Application.Commands;

public record UpdateScoringConfigCommand(ScoringConfigUpdateDto Dto, string UserEmail)
    : ICommand<ScoringConfigResponse>;

public class UpdateScoringConfigHandler : ICommandHandler<UpdateScoringConfigCommand, ScoringConfigResponse>
{
    private readonly IScoringConfigRepository _repository;

    public UpdateScoringConfigHandler(IScoringConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ScoringConfigResponse>> HandleAsync(UpdateScoringConfigCommand command,
        CancellationToken ct = default)
    {
        var current = await _repository.GetCurrentAsync(ct);
        var nextVersion = current is null ? 1 : current.Version + 1;

        var config = ScoringConfig.Create(
            command.Dto.GreenThreshold,
            command.Dto.YellowThreshold,
            command.Dto.IncomeRatioGood,
            command.Dto.IncomeRatioModerate,
            command.Dto.IncomeRatioLimited,
            command.Dto.PenaltyModerateRatio,
            command.Dto.PenaltyLimitedRatio,
            command.Dto.PenaltyCriticalRatio,
            command.Dto.RateGood,
            command.Dto.RateModerate,
            command.Dto.RateHeavy,
            command.Dto.PenaltyModerateRate,
            command.Dto.PenaltyHeavyRate,
            command.Dto.PenaltyExcessiveRate,
            command.Dto.PenaltySelfEmployed,
            command.Dto.PenaltyRetired,
            command.Dto.PenaltyUnemployed,
            command.Dto.PenaltyPaymentDefault,
            command.Dto.CreditScoreGood,
            command.Dto.CreditScoreModerate,
            command.Dto.PenaltyModerateCreditScore,
            command.Dto.PenaltyLowCreditScore,
            command.Dto.LoanToIncomeRatioGood,
            command.Dto.LoanToIncomeRatioModerate,
            command.Dto.LoanToIncomeRatioHigh,
            command.Dto.PenaltyModerateLoanToIncome,
            command.Dto.PenaltyHighLoanToIncome,
            command.Dto.PenaltyCriticalLoanToIncome,
            command.Dto.LoanTermShort,
            command.Dto.LoanTermMedium,
            command.Dto.LoanTermLong,
            command.Dto.PenaltyMediumLoanTerm,
            command.Dto.PenaltyLongLoanTerm,
            command.Dto.PenaltyVeryLongLoanTerm);

        var version = ScoringConfigVersion.Create(
            nextVersion,
            config,
            EmailAddress.Create(command.UserEmail));

        await _repository.AddAsync(version, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<ScoringConfigResponse>.Success(ScoringConfigMapper.ToResponse(version));
    }
}
