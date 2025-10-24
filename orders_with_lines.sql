SELECT 
    o."Id" as order_id, 
    o."Status", 
    o."TotalAmount",
    ol."Sku", 
    ol."Quantity", 
    ol."UnitPrice",
    (ol."Quantity" * ol."UnitPrice") as line_total
FROM "Orders" o
LEFT JOIN "OrderLine" ol ON o."Id" = ol."OrderId";
