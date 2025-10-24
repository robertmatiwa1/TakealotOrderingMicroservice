DELETE FROM "OrderLine" 
WHERE "Id" NOT IN (
    SELECT MIN("Id") 
    FROM "OrderLine" 
    GROUP BY "OrderId", "Sku", "Quantity", "UnitPrice"
);
