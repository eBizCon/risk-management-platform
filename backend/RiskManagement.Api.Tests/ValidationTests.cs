using RiskManagement.Api.Models;
using RiskManagement.Api.Validation;

namespace RiskManagement.Api.Tests;

public class ValidationTests
{
    private readonly ApplicationValidator _applicationValidator = new();
    private readonly ApplicationUpdateValidator _applicationUpdateValidator = new();
    private readonly ProcessorDecisionValidator _processorDecisionValidator = new();

    // applicationSchema Tests

    [Fact]
    public void Should_Validate_A_Valid_Application()
    {
        var model = new ApplicationCreateDto
        {
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Empty_Name()
    {
        var model = new ApplicationCreateDto
        {
            Name = "",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Name_Shorter_Than_2_Characters()
    {
        var model = new ApplicationCreateDto
        {
            Name = "A",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("mindestens"));
    }

    [Fact]
    public void Should_Reject_Negative_Income()
    {
        var model = new ApplicationCreateDto
        {
            Name = "Max Mustermann",
            Income = -100,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
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
            Name = "Max Mustermann",
            Income = 0,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Negative_Fixed_Costs()
    {
        var model = new ApplicationCreateDto
        {
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = -500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
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
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = 0,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Negative_Desired_Rate()
    {
        var model = new ApplicationCreateDto
        {
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = -100,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("positiv"));
    }

    [Fact]
    public void Should_Reject_Invalid_Employment_Status()
    {
        var model = new ApplicationCreateDto
        {
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "invalid_status",
            HasPaymentDefault = false
        };

        var result = _applicationValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Accept_All_Valid_Employment_Statuses()
    {
        var statuses = new[] { "employed", "self_employed", "unemployed", "retired" };

        foreach (var status in statuses)
        {
            var model = new ApplicationCreateDto
            {
                Name = "Max Mustermann",
                Income = 4000,
                FixedCosts = 1500,
                DesiredRate = 500,
                EmploymentStatus = status,
                HasPaymentDefault = false
            };

            var result = _applicationValidator.Validate(model);
            Assert.True(result.IsValid, $"Employment status '{status}' should be valid");
        }
    }

    // applicationWithBusinessRulesSchema Tests

    [Fact]
    public void Should_Reject_When_Desired_Rate_Exceeds_Available_Income()
    {
        var model = new ApplicationUpdateDto
        {
            Name = "Max Mustermann",
            Income = 3000,
            FixedCosts = 2000,
            DesiredRate = 1500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
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
            Name = "Max Mustermann",
            Income = 4000,
            FixedCosts = 1500,
            DesiredRate = 500,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationUpdateValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Accept_When_Desired_Rate_Equals_Available_Income()
    {
        var model = new ApplicationUpdateDto
        {
            Name = "Max Mustermann",
            Income = 3000,
            FixedCosts = 2000,
            DesiredRate = 1000,
            EmploymentStatus = "employed",
            HasPaymentDefault = false
        };

        var result = _applicationUpdateValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    // processorDecisionSchema Tests

    [Fact]
    public void Should_Accept_Approved_Decision_Without_Comment()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "approved",
            Comment = null
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Accept_Approved_Decision_With_Comment()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "approved",
            Comment = "Genehmigt aufgrund guter Bonität"
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Rejected_Decision_Without_Comment()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "rejected",
            Comment = null
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Begründung"));
    }

    [Fact]
    public void Should_Reject_Rejected_Decision_With_Empty_Comment()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "rejected",
            Comment = ""
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Accept_Rejected_Decision_With_Comment()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "rejected",
            Comment = "Abgelehnt wegen unzureichender Bonität"
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Reject_Invalid_Decision_Value()
    {
        var model = new ProcessorDecisionDto
        {
            Decision = "pending",
            Comment = "Some comment"
        };

        var result = _processorDecisionValidator.Validate(model);
        Assert.False(result.IsValid);
    }
}
