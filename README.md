# TaskTracker API

**TaskTracker** is a clean, modular REST API built with .NET 9 for managing tasks and users.  
It supports task creation, assignment, status updates, overdue detection, and user management.  
The architecture is layered and test-driven, making it scalable and maintainable.  

---

## Architecture

This project follows the **Layered Architecture** pattern with a clear separation of concerns:  

TaskTracker.Domain // Core entities and enums  
TaskTracker.Application // Interfaces, services, DTOs, models, and AutoMapper profiles  
TaskTracker.Infrastructure // EF Core, SQLite, repositories, background services  
TaskTracker.Presentation // ASP.NET Core controllers, validation, startup  

Test projects (unit + integration) are structured per layer:  
Tests/  
├── TaskTracker.Application.Tests.Unit  
├── TaskTracker.Infrastructure.Tests.Unit  
├── TaskTracker.Infrastructure.Tests.Integration  
├── TaskTracker.Presentation.Tests.Unit  
└── TaskTracker.Presentation.Tests.Integration  

---
## Technologies Used
| Area                 | Stack                                    |
| -------------------- | ---------------------------------------- |
| Language & Runtime   | C# 12, .NET 9                            |
| ORM / Database       | Entity Framework Core + SQLite           |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Validation           | FluentValidation                         |
| Object Mapping       | AutoMapper                               |
| Background Jobs      | BackgroundService (Overdue Task Updater) |
| API Documentation    | Swagger / OpenAPI                        |
| Testing              | xUnit, Moq, FluentValidation.TestHelper  |

---

##  Getting Started
### 1. Clone the repository
`git clone <repo-url>`  
`cd TaskTracker`

### 2. Apply migrations & create SQLite database
`dotnet ef database update --project TaskTracker.Infrastructure --startup-project TaskTracker.Presentation`  
`dotnet run --project TaskTracker.Presentation`  
Open http://localhost:5020/swagger to explore the API documentation.  

## Running Tests
From the /Tests directory:  
`dotnet test TaskTracker.Application.Tests.Unit`  
`dotnet test TaskTracker.Infrastructure.Tests.Integration`  
`dotnet test TaskTracker.Infrastructure.Tests.Unit`  
`dotnet test TaskTracker.Presentation.Tests.Integration`  
`dotnet test TaskTracker.Presentation.Tests.Unit`  

### Author
Maxim Astahov  
maximastahov0@gmail.com  