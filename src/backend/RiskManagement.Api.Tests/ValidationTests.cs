using RiskManagement.Application.DTOs;
using RiskManagement.Application.Validation;

namespace RiskManagement.Api.Tests;

public class ValidationTests
{
    private readonly ApplicationValidator _applicationValidator = new();
    private readonly ApplicationUpdateValidator _applicationUpdateValidator = new();
    private readonly ApproveApplicationValidator _approveValidator = new();
    private readonly RejectApplicationValidator _rejectValidator = new();

    // applicationSchema Tests

    [Fact]
    public void Should_Validate_A_Valid_Application()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Zero_CustomerId()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 0,
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Negative_CustomerId()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = -1,
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Kunde"));
    }

    [Fact]
    public void Should_Reject_Negative_Income()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = -100,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("positiv"));
    }

    [Fact]
    public void Should_Reject_Zero_Income()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = 0,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Negative_Fixed_Costs()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = 4000,
            FixedCosts = -500,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("nicht negativ"));
    }

    [Fact]
    public void Should_Allow_Zero_Fixed_Costs()
    {
        var model = new ApplicationCreateDto
        {
            CustomerId = 1,
            Income = 4000,
            FixedCosts = 0,
            DesiredRate = 500
        };

        var result = _applicationValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    // [Fact]
    // public void Should_Reject_Negative_Desired_Rate()
    // {
    //     var model = new ApplicationCreateDto
    //     {
    //         CustomerId = 1,
    //         Income = 4000,
    //         FixedCosts = 1500,
    //         DesiredRate = -100
    //     };
    //
    //     var result = _applicationValidator.Validate(model);
    //     Assert.False(result.IsValid);
    //     Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("positiv"));
    // }

    // applicationWithBusinessRulesSchema Tests

    [Fact]
    public void Should_Reject_When_Desired_Rate_Exceeds_Available_Income()
    {
        var model = new ApplicationUpdateDto
        {
            CustomerId = 1,
            Income = 3000,
            FixedCosts = 2000,
            DesiredRate = 1500
        };

        var result = _applicationUpdateValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("verfügbare"));
    }

    [Fact]
    public void Should_Accept_When_Desired_Rate_Is_Within_Available_Income()
    {
        var model = new ApplicationUpdateDto
        {
            CustomerId = 1,
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500
        };

        var result = _applicationUpdateValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Accept_When_Desired_Rate_Equals_Available_Income()
    {
        var model = new ApplicationUpdateDto
        {
            CustomerId = 1,
            Income = 3000,
            FixedCosts = 2000,
            DesiredRate = 1000
        };

        var result = _applicationUpdateValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    // ApproveApplicationValidator Tests

    [Fact]
    public void Should_Accept_Approve_Without_Comment()
    {
        var model = new ApproveApplicationDto
        {
            Comment = null
        };

        var result = _approveValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Accept_Approve_With_Comment()
    {
        var model = new ApproveApplicationDto
        {
            Comment = "Genehmigt aufgrund guter Bonität"
        };

        var result = _approveValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    // RejectApplicationValidator Tests

    [Fact]
    public void Should_Reject_Rejection_Without_Comment()
    {
        var model = new RejectApplicationDto
        {
            Comment = ""
        };

        var result = _rejectValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Begründung"));
    }

    [Fact]
    public void Should_Accept_Rejection_With_Comment()
    {
        var model = new RejectApplicationDto
        {
            Comment = "Abgelehnt wegen unzureichender Bonität"
        };

        var result = _rejectValidator.Validate(model);
        Assert.True(result.IsValid);
    }
}