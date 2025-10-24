Fix Pack v2 (expert-level)

WHAT THIS DOES
- Reconciles OutboxMessage property mismatches by adding legacy aliases (Type, OccurredAtUtc, PublishedUtc).
- Replaces OutboxWriter to use new fields (Topic, OccurredAt, DispatchedAt).
- Provides a safe AppDbContext wrapper to avoid EF mapping errors from older files.

HOW TO APPLY
1) Copy these files into your repo (overwrite existing):
   - Ordering.Infrastructure/Persistence/OrderingDbContext.cs
   - Ordering.Infrastructure/Outbox/OutboxWriter.cs
   - Ordering.Infrastructure/Persistence/AppDbContext.cs

2) Ensure you have removed any manual EF mappings for:
   - Order.Property(...), Order.Lines
   - OutboxMessage.Type / OccurredAtUtc / PublishedUtc
   (they are not needed; the new DbContext handles everything)

3) Rebuild and run:
   docker compose -f docker-compose.local.yaml build
   docker compose -f docker-compose.local.yaml up

4) Verify events in Redpanda:
   docker compose -f docker-compose.local.yaml exec redpanda rpk topic create ordering-events -X brokers=redpanda:9092 || true
   docker compose -f docker-compose.local.yaml exec redpanda rpk topic consume ordering-events -X brokers=redpanda:9092
   Then POST /api/orders from Swagger and watch the event appear.
