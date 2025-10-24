-- Drop existing tables if they exist (clean start)
DROP TABLE IF EXISTS "OrderLine";
DROP TABLE IF EXISTS "Orders";
DROP TABLE IF EXISTS "OutboxMessages";

-- Create Orders table
CREATE TABLE "Orders" (
    "Id" UUID NOT NULL,
    "CustomerId" UUID NOT NULL,
    "Status" TEXT NOT NULL,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "Currency" TEXT NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
);

-- Create OrderLine table  
CREATE TABLE "OrderLine" (
    "Id" UUID NOT NULL,
    "OrderId" UUID NOT NULL,
    "Sku" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "Currency" TEXT NOT NULL,
    CONSTRAINT "PK_OrderLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderLine_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

-- Create OutboxMessages table
CREATE TABLE "OutboxMessages" (
    "Id" UUID NOT NULL,
    "OccurredAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "DispatchedAt" TIMESTAMP WITH TIME ZONE NULL,
    "Topic" TEXT NOT NULL,
    "Payload" JSONB NOT NULL,
    CONSTRAINT "PK_OutboxMessages" PRIMARY KEY ("Id")
);
