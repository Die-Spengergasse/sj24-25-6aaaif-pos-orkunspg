using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using SPG_Fachtheorie.Aufgabe3.Model;

namespace SPG_Fachtheorie.Aufgabe3.Test;

public class PaymentsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static IEnumerable<object[]> GetPaymentsFilterTestData()
    {
        yield return new object[] { 1, "2024-05-13", 2 };  // Example: 2 payments for cashDesk 1 on that date
        yield return new object[] { 1, null, 3 };          // Example: 3 payments for cashDesk 1
        yield return new object[] { null, "2024-05-13", 4 }; // Example: 4 payments on that date
    }

    [Theory]
    [MemberData(nameof(GetPaymentsFilterTestData))]
    public async Task GetPayments_WithFilters_ReturnsFilteredResults(int? cashDesk, string? dateFrom, int expectedCount)
    {
        // Arrange
        var query = "";
        if (cashDesk.HasValue) query += $"cashDesk={cashDesk}";
        if (!string.IsNullOrEmpty(dateFrom))
        {
            if (query.Length > 0) query += "&";
            query += $"dateFrom={dateFrom}";
        }

        // Act
        var response = await _client.GetAsync($"/api/payments?{query}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();
        Assert.NotNull(payments);
        Assert.Equal(expectedCount, payments.Count);

        if (cashDesk.HasValue)
        {
            Assert.All(payments, p => Assert.Equal(cashDesk.Value, p.CashDesk.Number));
        }
        if (!string.IsNullOrEmpty(dateFrom))
        {
            var date = DateOnly.Parse(dateFrom);
            Assert.All(payments, p => Assert.True(p.Date >= date));
        }
    }

    [Fact]
    public async Task GetPaymentById_ExistingId_ReturnsPayment()
    {
        // Act
        var response = await _client.GetAsync("/api/payments/1");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var payment = await response.Content.ReadFromJsonAsync<PaymentDto>();
        Assert.NotNull(payment);
        Assert.Equal(1, payment.Id);
    }

    [Fact]
    public async Task GetPaymentById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/payments/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchPayment_ValidConfirmation_ReturnsOk()
    {
        // Act
        var response = await _client.PatchAsync("/api/payments/1", null);
        
        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PatchPayment_AlreadyConfirmed_ReturnsBadRequest()
    {
        // First confirmation
        await _client.PatchAsync("/api/payments/1", null);
        
        // Second confirmation attempt
        var response = await _client.PatchAsync("/api/payments/1", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchPayment_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PatchAsync("/api/payments/999", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePayment_ExistingId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/payments/1");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletePayment_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/payments/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
} 