# 🧩 Developer Guide — Takealot Ordering Microservice

This guide covers expert-level setup, troubleshooting, and verification steps for ensuring the **Ordering Microservice** runs 100% reliably with **Redpanda**, **PostgreSQL**, and the **Outbox Pattern**.

---

## ⚙️ Overview

The service implements a **transactional outbox pattern** and **event-driven design** with **Redpanda (Kafka-compatible)** as the event backbone.  
It ensures reliable event propagation from domain actions (like OrderCreated) to Kafka topics using guaranteed delivery semantics.

---

## 🧱 Core Components

| Component | Role |
|------------|------|
| `OrderingDbContext` | Primary EF Core context for Orders & Outbox messages |
| `OutboxMessage` | Stores serialized domain events for eventual Kafka dispatch |
| `OutboxDispatcher` | Background worker that reads pending messages & publishes to Kafka |
| `KafkaProducer` | Publishes events to `ordering-events` topic via Redpanda |
| `AppDbContext` | Wrapper for EF compatibility and test contexts |

---

## 🔧 Fix Pack v2 (Expert-Level Adjustments)

### 🛠 What This Fix Does

- Adds **legacy property aliases** for `OutboxMessage` to maintain EF Core compatibility (`Type`, `OccurredAtUtc`, `PublishedUtc`)
- Simplifies the `AppDbContext` factory for **design-time EF migrations**
- Updates the **`OutboxWriter`** to use the unified schema (`Topic`, `OccurredAt`, `DispatchedAt`)
- Prevents EF mapping mismatches in older builds or partial database migrations

### 🗂 Files Updated in This Patch

| File | Purpose |
|------|----------|
| `Ordering.Infrastructure/Persistence/OrderingDbContext.cs` | Main EF mapping configuration for Orders & Outbox |
| `Ordering.Infrastructure/Outbox/OutboxWriter.cs` | Writes events to `OutboxMessages` table |
| `Ordering.Infrastructure/Persistence/AppDbContext.cs` | Provides design-time factory for EF migrations |

---

## 🧰 How to Apply Locally

1. **Ensure PostgreSQL and Redpanda containers are healthy:**
   ```bash
   docker ps
````

You should see containers for:

* `takealotorderingmicroservice-postgres-1`
* `takealotorderingmicroservice-redpanda-1`

2. **Rebuild and start fresh:**

   ```bash
   docker compose -f docker-compose.local.yaml build
   docker compose -f docker-compose.local.yaml up
   ```

3. **Apply EF migrations (if not auto-run):**

   ```bash
   dotnet ef database update --context OrderingDbContext -p Ordering.Infrastructure -s Ordering.Api
   ```

4. **Verify tables:**

   ```bash
   docker exec takealotorderingmicroservice-postgres-1 psql -U postgres -d ordering -c "\dt"
   ```

   You should see:

   ```
   public | Orders
   public | OrderLine
   public | OutboxMessages
   ```

---

## 🧪 Verify Event Dispersion via Redpanda

Open a terminal and consume the `ordering-events` topic directly from inside Redpanda:

```bash
docker compose -f docker-compose.local.yaml exec redpanda rpk topic create ordering-events -X brokers=redpanda:9092 || true
docker compose -f docker-compose.local.yaml exec redpanda rpk topic consume ordering-events -X brokers=redpanda:9092
```

Then trigger an order creation from **Swagger**:

* Open Swagger UI: [http://localhost:5055/swagger/index.html](http://localhost:5055/swagger/index.html)
* Use **POST /api/orders**
* You should see JSON payloads like:

```json
{
  "eventType": "OrderCreated",
  "orderId": "b9f4f4e0-9dfb-4af0-8a33-456db290b71f",
  "timestamp": "2025-10-24T12:34:56Z"
}
```

✅ **Expected Result:**
Messages appear in Redpanda, meaning the **OutboxDispatcher → KafkaProducer → Redpanda** chain is functioning correctly.

---

## 🩺 Health & Diagnostics

| Endpoint        | Description                                   |
| --------------- | --------------------------------------------- |
| `/health/live`  | Basic process liveness check                  |
| `/health/ready` | Readiness check for DB and Kafka connectivity |
| `/swagger`      | API Explorer and test UI                      |

To test manually:

```bash
curl http://localhost:5055/health/live
curl http://localhost:5055/health/ready
```

---

## 🐘 PostgreSQL Manual Verification

Run inside the PostgreSQL container:

```bash
docker exec -it takealotorderingmicroservice-postgres-1 psql -U postgres -d ordering
```

Then inspect counts:

```sql
SELECT COUNT(*) AS orders FROM "Orders";
SELECT COUNT(*) AS order_lines FROM "OrderLine";
SELECT COUNT(*) AS outbox_messages FROM "OutboxMessages";
```

---

## 🐳 Docker Services Summary

| Service          | Port  | Description                    |
| ---------------- | ----- | ------------------------------ |
| `ordering.api`   | 5055  | Main ASP.NET Core microservice |
| `postgres`       | 5432  | Persistent order database      |
| `redpanda`       | 19092 | Kafka-compatible broker        |
| `redpanda-admin` | 9644  | Redpanda admin interface       |

---

## 🧩 Common Issues & Fixes

| Symptom                                              | Root Cause                   | Fix                                                    |
| ---------------------------------------------------- | ---------------------------- | ------------------------------------------------------ |
| `relation "OutboxMessages" does not exist`           | Database not migrated        | Run: `dotnet ef database update`                       |
| `ordering.api exited with code 139`                  | EF schema or migration crash | Delete `bin/obj`, rebuild, re-run migrations           |
| `password authentication failed for user "postgres"` | Wrong DB password            | Ensure `POSTGRES_PASSWORD` matches in compose file     |
| `No argument for parameter 'ct'`                     | Old OutboxWriter interface   | Ensure updated version from Fix Pack v2 is used        |
| Kafka not showing messages                           | Dispatcher not started       | Check logs for `OutboxDispatcher` or re-run containers |

---

## 🧠 Developer Notes

* The service uses **`OrderingDbContextFactory`** for design-time EF migrations.
* Always run EF commands using:

  ```bash
  dotnet ef migrations add InitialCreate --context OrderingDbContext -p Ordering.Infrastructure -s Ordering.Api
  dotnet ef database update --context OrderingDbContext -p Ordering.Infrastructure -s Ordering.Api
  ```
* Use `rpk` inside Redpanda to inspect topics and consumer offsets.
* Health checks are integrated with Docker Compose to ensure service startup order.

---

## 🧾 Summary

✅ Reliable Outbox Pattern
✅ Fully integrated with Redpanda
✅ Clean Architecture & DDD principles
✅ Automated database migrations
✅ Verified event delivery via Kafka-compatible topics

---

**Author:** Robert Matiwa
📧 [robertmatiwa3@gmail.com](mailto:robertmatiwa3@gmail.com)
🔗 [GitHub: robertmatiwa1](https://github.com/robertmatiwa)
