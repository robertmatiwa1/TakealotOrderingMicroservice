# 🛒 Takealot Ordering Microservice

![.NET 8 CI](https://github.com/robertmatiwa1/TakealotOrderingMicroservice/actions/workflows/dotnet-ci.yml/badge.svg)
![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![Docker](https://img.shields.io/badge/Containerized-Docker-blue)
![Kafka](https://img.shields.io/badge/Event--Driven-Kafka-orange)
![Architecture](https://img.shields.io/badge/Design-Clean%20Architecture-brightgreen)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow)

A **production-grade, cloud-native microservice** for managing the complete order lifecycle in an e-commerce platform.  
Built with **.NET 8**, following **Domain-Driven Design (DDD)** and **Clean Architecture** principles.

---

## 🚀 Features

- **Order Management:** Create, retrieve, and cancel customer orders  
- **Event-Driven Architecture:** Asynchronous communication using Kafka  
- **Reliable Messaging:** Outbox pattern for guaranteed delivery  
- **Cloud-Native:** Containerized with Docker and health checks  
- **Observability:** Structured logging with Serilog  
- **Scalable Design:** Clean Architecture with clear separation of concerns

---

## 🏗️ Architecture

```

Ordering Microservice Layers:
├── API Layer (Presentation)
│   ├── RESTful endpoints
│   ├── Swagger documentation
│   └── Health checks
├── Application Layer
│   ├── Use cases & business logic
│   ├── Validation & DTOs
│   └── Service interfaces
├── Domain Layer
│   ├── Entities & aggregates
│   ├── Domain events
│   └── Business rules
└── Infrastructure Layer
├── Entity Framework Core
├── PostgreSQL database
├── Kafka messaging
└── Outbox pattern implementation

```

---

## 🛠️ Technology Stack

| Component | Technology |
|------------|-------------|
| Framework | .NET 8, ASP.NET Core Web API |
| Database | PostgreSQL with Entity Framework Core |
| Messaging | Apache Kafka (via Redpanda) |
| Logging | Serilog (structured JSON) |
| Containerization | Docker & Docker Compose |
| Architecture | Clean Architecture + DDD |

---

## 📦 Solution Structure

```

TakealotOrderingMicroservice/
├── Ordering.Api/              # API layer (Controllers, Swagger, Health Checks)
├── Ordering.Application/      # Application layer (Use cases & services)
├── Ordering.Domain/           # Core business domain
├── Ordering.Infrastructure/   # Persistence, Messaging, Outbox
├── docker-compose.local.yaml  # Local environment setup
└── TakealotOrdering.sln       # Visual Studio solution

````

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Option 1: Run with Docker Compose (Recommended)
```bash
docker compose -f docker-compose.local.yaml up --build
````

### Option 2: Run Locally

```bash
cd Ordering.Api
dotnet run --urls "http://localhost:5055"
```

---

## 🌐 API Endpoints

### Create Order

```http
POST /api/orders
Content-Type: application/json

{
  "customerId": "a8f73ad9-7e8c-4b1d-ae11-bf2cb62a9f52",
  "lines": [
    { "sku": "SKU-001", "quantity": 2, "unitPrice": 199.99 },
    { "sku": "SKU-002", "quantity": 1, "unitPrice": 89.99 }
  ]
}
```

### Get Order

```http
GET /api/orders/{orderId}
```

### Cancel Order

```http
POST /api/orders/{orderId}/cancel
Content-Type: application/json

{
  "reason": "Customer request"
}
```

---

## 🔍 Health Checks

* Liveness: [http://localhost:5055/health/live](http://localhost:5055/health/live)
* Readiness: [http://localhost:5055/health/ready](http://localhost:5055/health/ready)

---

## 📚 API Documentation

Swagger UI available at:
👉 [http://localhost:5055/swagger/index.html](http://localhost:5055/swagger/index.html)

---

## 🎯 Key Features Explained

### 📨 Outbox Pattern & Kafka Integration

The service implements the **Outbox Pattern** to ensure reliable message delivery.

1. **Transactional Safety:** Domain events are written to the `OutboxMessages` table within the same DB transaction
2. **Background Processing:** The `OutboxDispatcher` polls and publishes events to Kafka
3. **Guaranteed Delivery:** Messages are marked dispatched only after successful Kafka send
4. **Fault Tolerance:** Resilient to restarts and transient failures

---

### 📦 Domain Events

* `OrderCreatedEvent` — published when a new order is placed
* `OrderCancelledEvent` — published when an order is cancelled
* Easily extendable for more lifecycle events

---

### 🩺 Health Monitoring

* Liveness and Readiness checks for container orchestration
* Structured Serilog logging for observability
* Ready for OpenTelemetry integration

---

## 🔧 Configuration

### Environment Variables

```json
{
  "ConnectionStrings:OrderingDb": "Host=localhost;Database=ordering;Username=postgres;Password=password",
  "Kafka:BootstrapServers": "localhost:9092",
  "Kafka:Topic": "ordering-events"
}
```

### App Settings

Edit `appsettings.json` for:

* Database connection strings
* Kafka broker configuration
* Logging levels
* Health check thresholds

---

## 🐳 Docker Services

| Service          | Port  | Purpose               |
| ---------------- | ----- | --------------------- |
| Ordering.API     | 5055  | Main microservice API |
| PostgreSQL       | 5432  | Primary database      |
| Kafka (Redpanda) | 19092 | Message broker        |
| Redpanda Admin   | 9644  | Cluster info UI       |

---

## 🛡️ Production Considerations

### Security

* JWT authentication-ready structure
* Input validation via FluentValidation
* API rate limiting (future enhancement)

### Scalability

* Stateless microservice (supports horizontal scaling)
* Event-driven architecture for decoupling
* Database connection pooling

### Monitoring

* Structured JSON logging
* Health endpoints for orchestration systems
* OpenTelemetry-ready for distributed tracing

---

## 🚧 Future Enhancements

* [ ] JWT Authentication & Authorization
* [ ] CQRS with MediatR
* [ ] Unit & Integration Tests
* [ ] OpenTelemetry tracing
* [ ] Kubernetes manifests
* [ ] Redis caching layer
* [ ] API versioning
* [ ] Rate limiting

---

## 👨‍💻 Author

**Robert Matiwa**
Senior Cloud & Solutions Architect
📧 [robertmatiwa3@gmail.com](mailto:robertmatiwa3@gmail.com)
🌍 Cape Town, South Africa
🔗 [GitHub: robertmatiwa1](https://github.com/robertmatiwa1)

---

## 📄 License

This project is licensed under the **MIT License** — see the LICENSE file for details.

---

## 🏁 Summary

The **Takealot Ordering Microservice** demonstrates enterprise-grade .NET architecture with:

* ✅ Clean Architecture & DDD principles
* ✅ Reliable Outbox-based event delivery
* ✅ Kafka + PostgreSQL integration
* ✅ Production-grade observability and health checks
* ✅ Cloud-native containerization

**Ready for scalable e-commerce order management in real-world environments.**

---

## 🧩 Developer Notes

For advanced patching and Redpanda testing instructions, see
👉 [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md)
