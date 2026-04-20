# Student Management System

An AI-agent-powered student management system built with .NET 10 and React. It handles student records, internship payments, and exam grades — with an AI agent that can parse uploaded documents (OCR), answer natural language queries, and generate Word/Excel reports.

## Architecture

The solution follows a **Service-Oriented Clean Architecture** with pure DI (no MediatR). Dependencies flow inward only.

```
StudentManagementSystem/
├── src/
│   ├── StudentManagement.Domain          # Entities, enums, repository interfaces
│   ├── StudentManagement.Application     # Business logic, DTOs, service interfaces
│   ├── StudentManagement.Infrastructure  # EF Core, MongoDB, Redis, external services
│   ├── StudentManagement.Api             # ASP.NET Core API (port 5000)
│   ├── StudentManagement.Agent           # LLM orchestration, prompt engineering (port 5200)
│   └── StudentManagement.MCP             # Model Context Protocol tool definitions
├── student-management-ui/                # React + Vite + TailwindCSS frontend
└── tests/
    ├── StudentManagement.UnitTests       # 26 unit tests (xUnit + Moq)
    └── StudentManagement.IntegrationTests # 7 integration tests (Testcontainers)
```

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | .NET 10, ASP.NET Core, C# 14 |
| AI / Agent | Microsoft.Extensions.AI, Azure OpenAI (GPT), MCP |
| Document OCR | Azure AI Document Intelligence |
| Document Generation | OpenXml (Word), ClosedXML (Excel) |
| Relational DB | PostgreSQL + Entity Framework Core 10 |
| NoSQL DB | MongoDB (chat sessions, structured logs) |
| Cache | Redis (StackExchange.Redis) |
| Logging | NLog with session-ID correlation |
| Frontend | React, TypeScript, Vite, TailwindCSS |
| API Docs | Scalar (OpenAPI) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (for PostgreSQL, MongoDB, Redis)
- Azure OpenAI resource
- Azure AI Document Intelligence resource

## Getting Started

### 1. Start infrastructure

```bash
docker run -d --name postgres -e POSTGRES_USER=studentmanagement -e POSTGRES_PASSWORD=studentmanagement -e POSTGRES_DB=studentmanagement -p 5432:5432 postgres:16-alpine
docker run -d --name mongodb -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=password -p 27017:27017 mongo:latest
docker run -d --name redis -p 6379:6379 redis:alpine
```

### 2. Configure the API

Edit `src/StudentManagement.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=studentmanagement;Username=studentmanagement;Password=studentmanagement",
    "Mongo": "mongodb://admin:password@localhost:27017/student_management?authSource=admin",
    "MongoLog": "mongodb://admin:password@localhost:27017?authSource=admin",
    "Redis": "localhost:6379"
  },
  "AzureDocumentIntelligence": {
    "Endpoint": "https://<your-resource>.cognitiveservices.azure.com/",
    "ApiKey": "<your-api-key>"
  }
}
```

Edit `src/StudentManagement.Agent/appsettings.Development.json` with your Azure OpenAI credentials.

### 3. Run the API

```bash
cd src/StudentManagement.Api
dotnet run
```

API will be available at `http://localhost:5000`. The root URL redirects to **Scalar API docs** at `http://localhost:5000/scalar/v1`.

### 4. Run the Agent

```bash
cd src/StudentManagement.Agent
dotnet run
```

Agent will be available at `http://localhost:5200/scalar/v1`.

## Key Features

**AI Document Processing**
Upload payment or exam grade documents. Azure Document Intelligence extracts the data; the agent maps it to existing student records using fuzzy name matching (Levenshtein distance). Confidence below 85% triggers a Human-in-the-Loop confirmation step before writing to the database.

**Natural Language Queries**
Ask questions like "How much was Ahmet paid this month?" — the agent calls the relevant MCP tool and formats the response in natural language.

**Document Generation**
Request a Word exam paper or Excel payment summary; the agent assembles the content and the infrastructure layer renders a formatted `.docx` or `.xlsx` file.

**Real-time Updates**
SignalR hub pushes progress notifications to the frontend during long-running OCR and agent operations.

**Audit Logging**
Every write operation is intercepted and stored in MongoDB's `AuditLogs` collection with full user context and session correlation.

## Running Tests

```bash
# Unit tests (no external dependencies)
dotnet test tests/StudentManagement.UnitTests

# Integration tests (requires Docker)
dotnet test tests/StudentManagement.IntegrationTests
```

> [!NOTE]
> Integration tests use Testcontainers and spin up isolated PostgreSQL, Redis, and MongoDB containers automatically. Docker must be running.

## Project Background

This system was built phase-by-phase following a specification-driven workflow. The phase prompts in `.github/prompts/` document each architectural decision from domain modeling through AI agent integration and automated testing.
