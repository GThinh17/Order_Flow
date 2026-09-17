INSERT INTO orderflow_inventory.stock_items
    (sku, quantity_on_hand, quantity_reserved)
VALUES
    ('DEMO-SUCCESS', 10, 0),
    ('DEMO-FAILURE', 10, 0),
    ('DEMO-SHORTAGE', 1, 0),
    ('DEMO-LAST-UNIT', 1, 0),
    ('DEMO-ADJUST', 10, 0)
ON CONFLICT (sku)
DO UPDATE SET
    quantity_on_hand = EXCLUDED.quantity_on_hand,
    quantity_reserved = 0
WHERE orderflow_inventory.stock_items.quantity_reserved = 0;