using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StudentManagement.Application.DTOs;
using StudentManagement.IntegrationTests.Fixtures;

namespace StudentManagement.IntegrationTests;

[Collection("Integration")]
public sealed class StudentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StudentEndpointsTests(DatabaseFixture db)
    {
        var factory = new CustomWebApplicationFactory(db);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyOrPopulatedList()
    {
        var response = await _client.GetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
        students.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidStudent_Returns201WithLocation()
    {
        var request = new CreateStudentRequest(
            FirstName: "Ayşe",
            LastName: "Kaya",
            StudentNumber: $"INT-{Guid.NewGuid():N}"[..12],
            Department: "Bilgisayar Mühendisliği",
            Phone: null,
            EnrollmentDate: new DateOnly(2024, 9, 1));

        var response = await _client.PostAsJsonAsync("/api/students", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<StudentDto>();
        created.Should().NotBeNull();
        created!.FirstName.Should().Be("Ayşe");
        created.LastName.Should().Be("Kaya");
    }

    [Fact]
    public async Task GetById_ExistingStudent_Returns200()
    {
        // Arrange — create a student first
        var createReq = new CreateStudentRequest(
            FirstName: "Mehmet",
            LastName: "Demir",
            StudentNumber: $"INT-{Guid.NewGuid():N}"[..12],
            Department: "Elektrik Mühendisliği",
            Phone: "+90 555 000 0001",
            EnrollmentDate: new DateOnly(2023, 9, 1));

        var createResponse = await _client.PostAsJsonAsync("/api/students", createReq);
        var created = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

        // Act
        var response = await _client.GetAsync($"/api/students/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        student!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_NonExistingStudent_Returns404()
    {
        var response = await _client.GetAsync($"/api/students/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
