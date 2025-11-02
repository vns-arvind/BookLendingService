# 📚 Book Lending Service API

A professional-grade **.NET 8 Web API** implementing a clean
architecture, SOLID principles, and production-ready patterns for a
fictional book lending service.

------------------------------------------------------------------------

## 🚀 Features

-   Add, view, check out, and return books.
-   Layered architecture: **Controller → Service → Repository →
    DbContext**
-   Built using **Dependency Injection**, **FluentValidation**, and
    **ProblemDetails**
-   Custom **Exception Middleware**
-   Structured **logging with Serilog**
-   Optional **in-memory caching**
-   Health Checks and Resilience patterns (Polly-ready)
-   Containerized with **Docker**
-   Deployable to **AWS (ECS or Lambda)** using IaC.

------------------------------------------------------------------------

## 🧩 Architecture Overview

    Controller → Service → Repository → DbContext

-   **Controller Layer:** Handles API requests and responses, performs
    input validation via FluentValidation and ProblemDetails.
-   **Service Layer:** Contains business logic, uses repositories, and
    enforces domain rules.
-   **Repository Layer:** Encapsulates data access logic using Entity
    Framework Core.
-   **Data Layer (DbContext):** Interacts with SQLite or in-memory data
    source.

------------------------------------------------------------------------

## ⚙️ Setup Instructions

### 🧱 Prerequisites

-   .NET 8 SDK
-   Docker (optional, for containerization)
-   SQLite or in-memory database (default)

### 🧩 Run Locally

``` bash
git clone <repo-url>
cd BookLending.Api
dotnet restore
dotnet run
```

Visit: <http://localhost:5000/swagger>

------------------------------------------------------------------------

## 🧠 Design Principles

  Principle             Implementation
  --------------------- -----------------------------------------------------
  **Scalability**       Async APIs, caching, containerization
  **Security**          HTTPS redirection, input validation, error handling
  **Maintainability**   Clean architecture, SOLID principles, DI
  **Resilience**        Exception middleware, retry patterns (Polly-ready)
  **Reliability**       Health checks, structured logging
  **Observability**     Serilog + health probes
  **Testability**       Unit tests, integration tests, mock repositories

------------------------------------------------------------------------

## 🧩 API Endpoints

  Method   Endpoint                     Description
  -------- ---------------------------- -------------------------
  `POST`   `/api/books`                 Add a new book
  `GET`    `/api/books`                 Get all available books
  `POST`   `/api/books/{id}/checkout`   Check out a book
  `POST`   `/api/books/{id}/return`     Return a book

------------------------------------------------------------------------

## 🐳 Docker Setup

### Build Image

``` bash
docker build -t booklending-api .
```

### Run Container

``` bash
docker run -d -p 5000:80 booklending-api
```

------------------------------------------------------------------------

## ☁️ AWS Deployment (Optional)

### Using ECS (Fargate)

-   Define infrastructure using **IaC (CloudFormation/Terraform)**.
-   Configure environment variables for DB and connection strings.
-   Push Docker image to **Amazon ECR**.
-   Deploy ECS Service with Load Balancer + Auto Scaling.

### Using Lambda + API Gateway

-   Use **AWS Lambda Web API template** or **Serverless Framework**.
-   Deploy via GitHub Actions or AWS CLI.

------------------------------------------------------------------------

## 🧰 Development Practices

-   **TDD-friendly:** Independent testable components.
-   **Custom Middleware:** Handles global exceptions.
-   **Validation:** FluentValidation integrated at controller level.
-   **Logging:** Serilog with console sink (lightweight setup).

------------------------------------------------------------------------

## 🧾 Example Log Output (Serilog)

    [11:20:15 INF] HTTP POST /api/books responded 201 in 52.4235ms
    [11:20:25 ERR] Book with Id=3 not found in repository.

------------------------------------------------------------------------

## 🧠 Bonus Enhancements

-   Caching demonstration via IMemoryCache.
-   Optional retry strategy (Polly).
-   Unit testing (xUnit + Moq).
-   Health check endpoint `/health`.

------------------------------------------------------------------------

## 🧰 Technologies Used

-   .NET 8 Web API
-   Entity Framework Core (SQLite / In-Memory)
-   FluentValidation
-   Serilog (Console Sink)
-   Docker
-   AWS ECS / Lambda Ready
-   xUnit, Moq (for testing)

------------------------------------------------------------------------

## 🧾 License

MIT License © 2025 Book Lending Service Demo

------------------------------------------------------------------------
