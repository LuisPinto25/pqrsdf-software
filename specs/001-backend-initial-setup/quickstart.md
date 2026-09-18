# Quickstart & Validation Guide: Backend Initial Architecture Setup

**Feature**: `001-backend-initial-setup`  
**Date**: 2026-09-17  

This guide describes how to build, run, and validate the initial backend architecture setup.

---

## 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/) installed (`dotnet --version` output >= 10.0.100).
- Running Microsoft SQL Server instance or local container (optional for unit tests; required for live database health checks).
- `curl` or browser/Postman for testing HTTP endpoints.

---

## 2. Build and Test Commands

### 2.1 Restore and Build the Entire Solution
From the repository root:
```bash
dotnet restore backend/Pqrsdf.sln
dotnet build backend/Pqrsdf.sln --no-restore
```
*Expected Outcome*: Build succeeds with 0 errors and 0 warnings (enforced by `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).

### 2.2 Run Automated Tests
Execute all unit and architectural tests:
```bash
dotnet test backend/Pqrsdf.sln --no-build --verbosity normal
```
*Expected Outcome*:
- Unit tests pass for `Result<T>` and `Error` primitives.
- Unit tests pass for `GetSystemStatusQueryHandler` validating the CQRS pipeline and Spanish response message.
- Architectural boundary tests verify that `Domain` has no dependencies on `Application`, `Infrastructure`, or `Api`.

---

## 3. Running the Backend API

### 3.1 Start the API Service
```bash
dotnet run --project backend/src/Api/Pqrsdf.Api.csproj
```
The application will start listening on default ports (e.g., `https://localhost:7155` and `http://localhost:5155`).

---

## 4. End-to-End Validation Scenarios

### Scenario 1: Reference System Status Query (Result Pattern & CQRS)
Execute:
```bash
curl -i http://localhost:5155/api/v1/system/status
```
*Expected Response (HTTP 200 OK)*:
Conforms to [contracts/system-status-contract.json](contracts/system-status-contract.json):
```json
{
  "isSuccess": true,
  "value": {
    "status": "Healthy",
    "service": "PQRSDF Backend API",
    "version": "1.0.0",
    "environment": "Development",
    "timestamp": "2026-09-17T...",
    "databaseConnected": true,
    "message": "El sistema se encuentra operativo y conectado a la base de datos SQL Server."
  },
  "error": null
}
```

### Scenario 2: Built-in Health Endpoint
Execute:
```bash
curl -i http://localhost:5155/health
```
*Expected Response (HTTP 200 OK)*:
Body: `Healthy`

### Scenario 3: Global Exception Interception (ProblemDetails)
Simulate an unhandled error by requesting a test exception endpoint (in Development mode or triggering an internal fault):
```bash
curl -i http://localhost:5155/api/v1/system/simulate-error
```
*Expected Response (HTTP 500 Internal Server Error)*:
Conforms to [contracts/problem-details-contract.json](contracts/problem-details-contract.json):
- Status: `500`
- `detail`: `"Ha ocurrido un error inesperado al procesar su solicitud. Por favor intente más tarde."`
- No internal stack trace or SQL Server connection string exposed in the JSON body.

### Scenario 4: OpenAPI / Swagger Documentation
Open in a browser:
```
http://localhost:5155/swagger
```
*Expected Outcome*: Interactive Swagger UI renders the OpenAPI schema with endpoints, contracts, and schema definitions.
