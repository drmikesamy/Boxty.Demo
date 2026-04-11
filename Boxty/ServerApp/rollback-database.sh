#!/bin/bash

# Navigate to the Server directory
cd "$(dirname "$0")"

# Collect all modules
modules=()
for module in Modules/*/Infrastructure; do
    if [ -d "$module" ]; then
        modules+=("$module")
    fi
done

# Check if any modules were found
if [ ${#modules[@]} -eq 0 ]; then
    echo "No modules found."
    exit 1
fi

# List all modules
echo "Modules found:"
for i in "${!modules[@]}"; do
    module_name=$(basename "$(dirname "${modules[$i]}")")
    echo "$((i + 1)). $module_name"
done

# Prompt the user to choose a module
read -p "Enter the number of the module to rollback: " choice

# Validate the choice
if [[ ! "$choice" =~ ^[0-9]+$ ]] || [[ "$choice" -lt 1 || "$choice" -gt ${#modules[@]} ]]; then
    echo "Invalid choice."
    exit 1
fi

# Handle module selection
module="${modules[$((choice - 1))]}"
module_name=$(basename "$(dirname "$module")")
context_name="${module_name}DbContext"
project_path="$(pwd)/Modules/${module_name}/Infrastructure/Boxty.ServerApp.Modules.${module_name}.Infrastructure.csproj"
migrations_dir="$(pwd)/Modules/${module_name}/Infrastructure/Database/Migrations"

echo "Selected context: $context_name"
echo

# Get list of migrations
echo "Getting migration history..."
migrations_output=$(ASPNETCORE_ENVIRONMENT=Local dotnet ef migrations list --context "$context_name" --project "$project_path" --startup-project "$(pwd)/WebApi/WebApi.csproj" 2>/dev/null)

if [[ $? -ne 0 ]] || [[ -z "$migrations_output" ]]; then
    echo "Failed to retrieve migrations or no migrations found for $context_name."
    exit 1
fi

# Parse migrations into an array (reverse order to get latest first)
# Filter out status messages and only keep actual migration names (they start with timestamps)
mapfile -t all_migrations < <(echo "$migrations_output" | grep -E "^[0-9]{14}_" | tac)

# Check if there are any migrations
if [[ ${#all_migrations[@]} -eq 0 ]]; then
    echo "No migrations found for $context_name."
    exit 1
fi

# Get the latest 5 migrations (or all if less than 5)
latest_migrations=()
max_count=5
for i in "${!all_migrations[@]}"; do
    if [[ $i -lt $max_count ]]; then
        latest_migrations+=("${all_migrations[$i]}")
    else
        break
    fi
done

echo "Latest migrations for $context_name (latest at the top):"
echo "0. (Initial/Empty database - remove all migrations)"
for i in "${!latest_migrations[@]}"; do
    migration="${latest_migrations[$i]}"
    echo "$((i + 1)). $migration"
done

# If there are more than 5 migrations, show the count
if [[ ${#all_migrations[@]} -gt $max_count ]]; then
    echo "... and $((${#all_migrations[@]} - $max_count)) older migrations"
fi

echo

# Prompt for migration selection
read -p "Enter the number of the migration to rollback to: " migration_choice

# Validate the migration choice
if [[ ! "$migration_choice" =~ ^[0-9]+$ ]] || [[ "$migration_choice" -lt 0 || "$migration_choice" -gt ${#latest_migrations[@]} ]]; then
    echo "Invalid migration choice."
    exit 1
fi

# Determine the target migration
if [[ "$migration_choice" -eq 0 ]]; then
    target_migration="0"
    echo "Rolling back to initial state (removing all migrations)..."
else
    target_migration="${latest_migrations[$((migration_choice - 1))]}"
    echo "Rolling back to migration: $target_migration"
fi

# Confirm the rollback
echo
read -p "Are you sure you want to rollback the database? This action cannot be undone. (y/N): " confirm

if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
    echo "Rollback cancelled."
    exit 0
fi

# Execute the rollback
echo "Executing rollback..."
if [[ "$target_migration" == "0" ]]; then
    ASPNETCORE_ENVIRONMENT=Local dotnet ef database update 0 --context "$context_name" --project "$project_path" --startup-project "$(pwd)/WebApi/WebApi.csproj"
else
    ASPNETCORE_ENVIRONMENT=Local dotnet ef database update "$target_migration" --context "$context_name" --project "$project_path" --startup-project "$(pwd)/WebApi/WebApi.csproj"
fi

if [[ $? -eq 0 ]]; then
    echo "Database rollback completed successfully."
else
    echo "Database rollback failed."
    exit 1
fi
