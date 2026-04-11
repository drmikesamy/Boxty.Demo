# Database Seeds

This directory contains SQL seed files that are automatically loaded into the PostgreSQL databases when they are first initialized (when the data volume is empty).

## Files

- `keycloak-seed.sql` - Seed data for the Keycloak authentication database

## How It Works

When you run `docker compose up` for the first time (or after removing volumes with `docker compose down -v`), PostgreSQL automatically executes any `.sql` files found in `/docker-entrypoint-initdb.d/`.

The seed files are committed to version control, so anyone cloning the repository gets a working database setup immediately.

## Updating Seeds

To update the seed files with current database state:

```bash
# Export keycloak database
docker exec chkeycloak.database pg_dump -U keycloak -d keycloak > database-seeds/keycloak-seed.sql
```

Then commit the updated seed files to version control.
