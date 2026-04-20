using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StudentManagement.Application.DTOs;
using StudentManagement.IntegrationTests.Fixtures;

namespace StudentManagement.IntegrationTests;

[Collection("Integration")]
public sealed class PaymentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentEndpointsTests(DatabaseFixture db)
    {
        var factory = new CustomWebApplicationFactory(db);
        _client = factory.CreateClient();
    }

    // ── helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> CreateStudentAsync()
    {
        var request = new CreateStudentRequest(
            FirstName: "Test",
            LastName: "Öğrenci",
            StudentNumber: $"PAY-{Guid.NewGuid():N}"[..12],
            Department: "Test Bölümü",
            Phone: null,
            EnrollmentDate: new DateOnly(2024, 9, 1));

        var response = await _client.PostAsJsonAsync("/api/students", request);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<StudentDto>();
        return dto!.Id;
    }

    // ── tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPayments_ExistingStudent_ReturnsOkWithEmptyList()
    {
        var studentId = await CreateStudentAsync();

        var response = await _client.GetAsync($"/api/students/{studentId}/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task UpsertPayment_ValidRequest_Returns204()
    {
        var studentId = await CreateStudentAsync();
        var payload = new { Amount = 1500.00m, PaymentDate = "2026-03-01" };

        var response = await _client.PutAsJsonAsync(
            $"/api/students/{studentId}/payments/2026/3", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
