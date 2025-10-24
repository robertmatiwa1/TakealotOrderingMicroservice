-- Table counts
SELECT 'Orders' as table_name, COUNT(*) as record_count FROM "Orders"
UNION ALL
SELECT 'OrderLine', COUNT(*) FROM "OrderLine"
UNION ALL
SELECT 'OutboxMessages', COUNT(*) FROM "OutboxMessages";

-- Orders with their line items
SELECT 
    o."Id" as order_id, 
    o."Status", 
    o."TotalAmount",
    ol."Sku", 
    ol."Quantity", 
    ol."UnitPrice"
FROM "Orders" o
LEFT JOIN "OrderLine" ol ON o."Id" = ol."OrderId";
