#!/usr/bin/env bash
set -euo pipefail

psql \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set ON_ERROR_STOP=1 \
  --set=orders_db="$ORDERS_DB_NAME" \
  --set=orders_user="$ORDERS_DB_USER" \
  --set=orders_password="$ORDERS_DB_PASSWORD" \
  --set=inventory_db="$INVENTORY_DB_NAME" \
  --set=inventory_user="$INVENTORY_DB_USER" \
  --set=inventory_password="$INVENTORY_DB_PASSWORD" \
  --set=payments_db="$PAYMENTS_DB_NAME" \
  --set=payments_user="$PAYMENTS_DB_USER" \
  --set=payments_password="$PAYMENTS_DB_PASSWORD" <<'EOSQL'
    CREATE ROLE :"orders_user"
      LOGIN PASSWORD :'orders_password';

    CREATE ROLE :"inventory_user"
      LOGIN PASSWORD :'inventory_password';

    CREATE ROLE :"payments_user"
      LOGIN PASSWORD :'payments_password';

    CREATE DATABASE :"orders_db"
      OWNER :"orders_user";

    CREATE DATABASE :"inventory_db"
      OWNER :"inventory_user";

    CREATE DATABASE :"payments_db"
      OWNER :"payments_user";

    REVOKE CONNECT ON DATABASE :"orders_db" FROM PUBLIC;
    REVOKE CONNECT ON DATABASE :"inventory_db" FROM PUBLIC;
    REVOKE CONNECT ON DATABASE :"payments_db" FROM PUBLIC;

    GRANT CONNECT ON DATABASE :"orders_db" TO :"orders_user";
    GRANT CONNECT ON DATABASE :"inventory_db" TO :"inventory_user";
    GRANT CONNECT ON DATABASE :"payments_db" TO :"payments_user";
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "$ORDERS_DB_NAME" \
  --set ON_ERROR_STOP=1 \
  --set=database_name="$ORDERS_DB_NAME" \
  --set=database_user="$ORDERS_DB_USER" <<'EOSQL'
    CREATE SCHEMA :"database_name"
      AUTHORIZATION :"database_user";

    ALTER DATABASE :"database_name"
      SET search_path TO :"database_name", public;
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "$INVENTORY_DB_NAME" \
  --set ON_ERROR_STOP=1 \
  --set=database_name="$INVENTORY_DB_NAME" \
  --set=database_user="$INVENTORY_DB_USER" <<'EOSQL'
    CREATE SCHEMA :"database_name"
      AUTHORIZATION :"database_user";

    ALTER DATABASE :"database_name"
      SET search_path TO :"database_name", public;
EOSQL

psql \
  --username "$POSTGRES_USER" \
  --dbname "$PAYMENTS_DB_NAME" \
  --set ON_ERROR_STOP=1 \
  --set=database_name="$PAYMENTS_DB_NAME" \
  --set=database_user="$PAYMENTS_DB_USER" <<'EOSQL'
    CREATE SCHEMA :"database_name"
      AUTHORIZATION :"database_user";

    ALTER DATABASE :"database_name"
      SET search_path TO :"database_name", public;
EOSQL
