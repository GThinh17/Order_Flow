#!/usr/bin/env bash
set -euo pipefail

psql \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 <<-EOSQL
    CREATE ROLE orders_app
      LOGIN PASSWORD '${ORDERS_DB_PASSWORD}';

    CREATE ROLE inventory_app
      LOGIN PASSWORD '${INVENTORY_DB_PASSWORD}';

    CREATE ROLE payments_app
      LOGIN PASSWORD '${PAYMENTS_DB_PASSWORD}';

    CREATE DATABASE orderflow_orders
      OWNER orders_app;

    CREATE DATABASE orderflow_inventory
      OWNER inventory_app;

    CREATE DATABASE orderflow_payments
      OWNER payments_app;
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "orderflow_orders" \
  --set ON_ERROR_STOP=1 <<-EOSQL
    CREATE SCHEMA orderflow_orders
      AUTHORIZATION orders_app;

    ALTER DATABASE orderflow_orders
      SET search_path TO orderflow_orders, public;
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "orderflow_inventory" \
  --set ON_ERROR_STOP=1 <<-EOSQL
    CREATE SCHEMA orderflow_inventory
      AUTHORIZATION inventory_app;

    ALTER DATABASE orderflow_inventory
      SET search_path TO orderflow_inventory, public;
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "orderflow_payments" \
  --set ON_ERROR_STOP=1 <<-EOSQL
    CREATE SCHEMA orderflow_payments
      AUTHORIZATION payments_app;

    ALTER DATABASE orderflow_payments
      SET search_path TO orderflow_payments, public;
EOSQL