# Patch: Make it run 100% with Redpanda + Outbox proof

## Quick start
```bash
docker compose -f docker-compose.local.yaml up --build
# Open Swagger at http://localhost:5055
```

## Verify event dispersion
Open a shell into the Redpanda container and consume the topic:
```bash
docker compose -f docker-compose.local.yaml exec redpanda rpk topic create ordering-events -X brokers=redpanda:9092 || true
docker compose -f docker-compose.local.yaml exec redpanda rpk topic consume ordering-events -X brokers=redpanda:9092
```

In Swagger, call `POST /api/orders` with a valid body. You should see `OrderCreated` payloads appear in the terminal (produced by the OutboxDispatcher via KafkaProducer).

## Health
- Live: `GET http://localhost:5055/health/live`
- Ready: `GET http://localhost:5055/health/ready`
