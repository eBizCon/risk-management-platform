using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Infrastructure.ExternalServices;
using FluentAssertions;

namespace RiskManagement.Api.Tests;

public class MockSchufaProviderTests
{
    private readonly MockSchufaProvider _provider = new();

    private static Address CreateTestAddress() =>
        Address.Create("Musterstraße 1", "Berlin", "10115", "Deutschland");

    [Fact]
    public async Task CheckAsync_DefaultCase_ShouldReturnNoDefaultAndScore420()
    {
        var result = await _provider.CheckAsync("Max", "Mustermann", new DateOnly(1990, 1, 1), CreateTestAddress());

        result.HasPaymentDefault.Should().BeFalse();
        result.CreditScore.Should().Be(420);
        result.Provider.Should().Be("schufa_mock");
    }

    [Theory]
    [InlineData("Verzug")]
    [InlineData("Default")]
    [InlineData("Herr Verzug-Test")]
    [InlineData("TestDefault")]
    public async Task CheckAsync_LastNameContainsVerzugOrDefault_ShouldReturnPaymentDefaultAndScore250(string lastName)
    {
        var result = await _provider.CheckAsync("Max", lastName, new DateOnly(1990, 1, 1), CreateTestAddress());

        result.HasPaymentDefault.Should().BeTrue();
        result.CreditScore.Should().Be(250);
    }

    [Fact]
    public async Task CheckAsync_AgeOver65_ShouldReturnScore520()
    {
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-70));

        var result = await _provider.CheckAsync("Max", "Mustermann", dateOfBirth, CreateTestAddress());

        result.HasPaymentDefault.Should().BeFalse();
        result.CreditScore.Should().Be(520);
    }

    [Fact]
    public async Task CheckAsync_AgeUnder25_ShouldReturnScore350()
    {
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20));

        var result = await _provider.CheckAsync("Max", "Mustermann", dateOfBirth, CreateTestAddress());

        result.HasPaymentDefault.Should().BeFalse();
        result.CreditScore.Should().Be(350);
    }

    [Fact]
    public async Task CheckAsync_VerzugTakesPrecedenceOverAge_ShouldReturnPaymentDefault()
    {
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-70));

        var result = await _provider.CheckAsync("Max", "Verzug", dateOfBirth, CreateTestAddress());

        result.HasPaymentDefault.Should().BeTrue();
        result.CreditScore.Should().Be(250);
    }
}
