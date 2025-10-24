INSERT INTO "Orders" ("Id", "CustomerId", "Status", "TotalAmount", "Currency") 
VALUES (gen_random_uuid(), gen_random_uuid(), 'Pending', 99.99, 'USD');
