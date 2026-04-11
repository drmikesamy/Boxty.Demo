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
read -p "Enter the number of the module to migrate: " choice

# Validate the choice
if [[ ! "$choice" =~ ^[0-9]+$ ]] || [[ "$choice" -lt 1 || "$choice" -gt ${#modules[@]} ]]; then
    echo "Invalid choice."
    exit 1
fi

# Process the selected module(s)
for i in "${!modules[@]}"; do
    if [[ -n "$choice" && "$choice" -ne $((i + 1)) ]]; then
        continue
    fi

    module="${modules[$i]}"
    module_name=$(basename "$(dirname "$module")")

    # Prompt for the migration name
    read -p "Enter the migration name for $module_name: " migration_name

    # Skip if no migration name is provided
    if [[ -z "$migration_name" ]]; then
        echo "Skipping migration for $module_name."
        continue
    fi

    # Run the migration command
    ASPNETCORE_ENVIRONMENT=Local dotnet ef migrations add "$migration_name" --context "${module_name}DbContext" --output-dir Database/Migrations --project "$(pwd)/Modules/${module_name}/Infrastructure/Boxty.ServerApp.Modules.${module_name}.Infrastructure.csproj" --startup-project "$(pwd)/WebApi/WebApi.csproj"
done