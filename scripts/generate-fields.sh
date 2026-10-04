#!/bin/bash

# Script to add multiple fields to an existing entity, DTO and mapper
# Example input:
#   public int Id, public required string Name, public string Description, public Guid TenantId, public SchemaListing SchemaListing

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MODULES_DIR="$PROJECT_ROOT/Boxty/ServerApp/Modules"
SHARED_DTO_DIR="$PROJECT_ROOT/Boxty/SharedApp/DTOs"

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

show_help() {
    echo "Usage: ./generate-fields.sh"
    echo ""
    echo "This script adds multiple properties to an existing entity, DTO and mapper."
    echo "You can enter one or more fields separated by commas."
    echo "Examples:"
    echo "  public int Id"
    echo "  public required string Name"
    echo "  public string Description"
    echo "  public Guid TenantId"
    echo "  public SchemaListing SchemaListing"
    echo ""
    echo "Full example:"
    echo "  public int Id, public required string Name, public string Description, public Guid TenantId, public SchemaListing SchemaListing"
    echo ""
}

list_modules() {
    print_info "Available modules:" >&2
    local index=1
    for module in "$MODULES_DIR"/*; do
        if [ -d "$module" ] && [ "$(basename "$module")" != "Shared" ]; then
            echo "  $index) $(basename "$module")" >&2
            ((index++))
        fi
    done
}

get_module() {
    list_modules
    echo "" >&2
    read -p "Enter the module name (or number): " module_input >&2

    if [[ "$module_input" =~ ^[0-9]+$ ]]; then
        local index=1
        for module in "$MODULES_DIR"/*; do
            if [ -d "$module" ] && [ "$(basename "$module")" != "Shared" ]; then
                if [ "$index" -eq "$module_input" ]; then
                    echo "$(basename "$module")"
                    return 0
                fi
                ((index++))
            fi
        done
        print_error "Invalid module number" >&2
        exit 1
    fi

    if [ -d "$MODULES_DIR/$module_input" ]; then
        echo "$module_input"
        return 0
    fi

    print_error "Module '$module_input' does not exist" >&2
    exit 1
}

get_entity_name() {
    read -p "Enter the existing entity name (PascalCase): " entity_name >&2

    if [[ ! "$entity_name" =~ ^[A-Z][a-zA-Z0-9]*$ ]]; then
        print_error "Entity name must be in PascalCase (e.g., MyEntity)" >&2
        exit 1
    fi

    echo "$entity_name"
}

get_field_input() {
    echo "" >&2
    print_info "Enter the fields to add, separated by commas." >&2
    print_info "Example: public int Id, public required string Name, public string Description" >&2
    read -r -p "Fields: " field_input >&2
    echo "$field_input"
}

update_files() {
    local module="$1"
    local entity_name="$2"
    local field_input="$3"
    local entity_path="$MODULES_DIR/$module/Entities/${entity_name}.cs"
    local dto_path="$SHARED_DTO_DIR/$module/${entity_name}Dto.cs"
    local mapper_path="$MODULES_DIR/$module/Infrastructure/Mappers/${entity_name}Mapper.cs"

    python3 - "$field_input" "$entity_path" "$dto_path" "$mapper_path" "$module" "$entity_name" <<'PY'
import re
import sys
from pathlib import Path

raw_fields, entity_path, dto_path, mapper_path, module, entity_name = sys.argv[1:7]
entity_path = Path(entity_path)
dto_path = Path(dto_path)
mapper_path = Path(mapper_path)


def split_top_level_fields(text: str):
    items = []
    current = []
    depth = 0
    for ch in text:
        if ch == '<':
            depth += 1
        elif ch == '>':
            depth = max(0, depth - 1)
        if ch == ',' and depth == 0:
            item = ''.join(current).strip()
            if item:
                items.append(item)
            current = []
        else:
            current.append(ch)
    item = ''.join(current).strip()
    if item:
        items.append(item)
    return items


def split_generic_args(text: str):
    items = []
    current = []
    depth = 0
    for ch in text:
        if ch == '<':
            depth += 1
        elif ch == '>':
            depth = max(0, depth - 1)
        if ch == ',' and depth == 0:
            item = ''.join(current).strip()
            if item:
                items.append(item)
            current = []
        else:
            current.append(ch)
    item = ''.join(current).strip()
    if item:
        items.append(item)
    return items


def map_type_name(type_name: str, dto_mode: bool = False) -> str:
    type_name = type_name.strip()
    if type_name in {'string', 'int', 'long', 'short', 'byte', 'bool', 'double', 'float', 'decimal', 'Guid', 'DateTime', 'DateOnly', 'TimeOnly', 'object'}:
        return type_name
    if type_name.startswith('Nullable<'):
        inner = type_name[9:-1]
        return f'Nullable<{map_type_name(inner, dto_mode)}>'
    if type_name.endswith('[]'):
        item = map_type_name(type_name[:-2], dto_mode)
        return f'{item}[]'
    if type_name.startswith('FrozenDictionary<') and type_name.endswith('>'):
        content = type_name[len('FrozenDictionary<'):-1]
        key_type, value_type = split_generic_args(content)
        return f'FrozenDictionary<{map_type_name(key_type, dto_mode)}, {map_type_name(value_type, dto_mode)}>'
    for container in ('Dictionary', 'IReadOnlyDictionary', 'IReadOnlyList', 'IList', 'ICollection', 'List', 'HashSet', 'IEnumerable'):
        if type_name.startswith(f'{container}<') and type_name.endswith('>'):
            content = type_name[len(container) + 1:-1]
            return f'{container}<{map_type_name(content, dto_mode)}>'
    if re.fullmatch(r'[A-Z][A-Za-z0-9_]*', type_name):
        return f'{type_name}Dto' if dto_mode else type_name
    return type_name


def parse_field_definition(item: str):
    field_pattern = re.compile(r'(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:\{.*\})?\s*(?:=\s*[^;]+)?\s*;?\s*$', re.DOTALL)
    match = field_pattern.search(item)
    if not match:
        raise SystemExit(f"Invalid field definition: '{item}'. Expected format: public required string Name or public required FrozenDictionary<string, ColumnMetadata> Columns {{ get; init; }}")
    name = match.group('name')
    prefix = item[:match.start()].strip()
    access = 'public'
    if prefix.startswith('public '):
        prefix = prefix[len('public '):]
    elif prefix.startswith('private '):
        access = 'private'; prefix = prefix[len('private '):]
    elif prefix.startswith('protected '):
        access = 'protected'; prefix = prefix[len('protected '):]
    elif prefix.startswith('internal '):
        access = 'internal'; prefix = prefix[len('internal '):]
    required = False
    if prefix.startswith('required '):
        required = True
        prefix = prefix[len('required '):]
    type_name = prefix.strip()
    default_expr = None
    default_match = re.search(r'=\s*([^;]+)\s*$', item)
    if default_match:
        default_expr = default_match.group(1).strip()
    accessor = 'set'
    if 'get; init' in item or 'get;init' in item:
        accessor = 'init'
    return {
        'access': access,
        'required': required,
        'type': type_name,
        'name': name,
        'accessor': accessor,
        'default': default_expr,
    }


props = []
for chunk in split_top_level_fields(raw_fields):
    item = chunk.strip()
    if item:
        props.append(parse_field_definition(item))

if not props:
    raise SystemExit('No valid field definitions were provided.')


def required_prefix(prop):
    return ' required' if prop['required'] else ''


def default_value_for_type(prop, dto_mode: bool = False):
    type_name = map_type_name(prop['type'], dto_mode)
    if prop['default']:
        return prop['default']
    if prop['required']:
        return None
    if type_name == 'string':
        return 'string.Empty'
    if type_name.endswith('[]'):
        item_type = type_name[:-2]
        return f'Array.Empty<{item_type}>()'
    if type_name.startswith('FrozenDictionary<'):
        return f'{type_name}.Empty'
    if type_name.startswith('List<') or type_name.startswith('IList<') or type_name.startswith('ICollection<') or type_name.startswith('HashSet<') or type_name.startswith('Dictionary<') or type_name.startswith('IReadOnlyList<') or type_name.startswith('IEnumerable<'):
        return 'new()'
    return None


def entity_line(prop):
    type_name = map_type_name(prop['type'], False)
    default_value = default_value_for_type(prop, False)
    required_part = required_prefix(prop)
    if default_value is not None:
        return f"        {prop['access']}{required_part} {type_name} {prop['name']} {{ get; {prop['accessor']}; }} = {default_value};"
    return f"        {prop['access']}{required_part} {type_name} {prop['name']} {{ get; {prop['accessor']}; }}"


def dto_line(prop):
    type_name = map_type_name(prop['type'], True)
    default_value = default_value_for_type(prop, True)
    required_part = required_prefix(prop)
    if default_value is not None:
        return f"        {prop['access']}{required_part} {type_name} {prop['name']} {{ get; {prop['accessor']}; }} = {default_value};"
    return f"        {prop['access']}{required_part} {type_name} {prop['name']} {{ get; {prop['accessor']}; }}"


def dto_property_value(prop):
    type_name = prop['type']
    if type_name.startswith('FrozenDictionary<'):
        value_type = split_generic_args(type_name[len('FrozenDictionary<'):-1])[1]
        dto_value_type = map_type_name(value_type, True)
        return f"entity.{prop['name']}.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value == null ? null : new {dto_value_type} {{ Name = kvp.Value.Name, DataType = kvp.Value.DataType, IsPrimaryKey = kvp.Value.IsPrimaryKey }})"
    return f"entity.{prop['name']}"


def entity_property_value(prop):
    type_name = prop['type']
    if type_name.startswith('FrozenDictionary<'):
        value_type = split_generic_args(type_name[len('FrozenDictionary<'):-1])[1]
        dto_value_type = map_type_name(value_type, True)
        return f"dto.{prop['name']}.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value == null ? null : new {value_type} {{ Name = kvp.Value.Name, DataType = kvp.Value.DataType, IsPrimaryKey = kvp.Value.IsPrimaryKey }})"
    return f"dto.{prop['name']}"


def generate_entity_content():
    lines = [entity_line(prop) for prop in props]
    return (
        "using Boxty.ServerBase.Entities;\n"
        "using Boxty.SharedBase.Interfaces;\n"
        + ("using System.Collections.Frozen;\n" if any('FrozenDictionary<' in prop['type'] for prop in props) else '') +
        "\n"
        f"namespace Boxty.ServerApp.Modules.{module}.Entities\n"
        "{\n"
        f"    public class {entity_name} : BaseEntity<{entity_name}>, IEntity\n"
        "    {\n"
        + '\n'.join(lines) + "\n"
        "    }\n"
        "}\n"
    )


def generate_dto_content():
    lines = [dto_line(prop) for prop in props]
    return (
        "using Boxty.SharedBase.DTOs;\n"
        "using Boxty.SharedBase.Interfaces;\n"
        "using Boxty.SharedApp.DTOs;\n"
        + ("using System.Collections.Frozen;\n" if any('FrozenDictionary<' in prop['type'] for prop in props) else '') +
        "\n"
        f"namespace Boxty.SharedApp.DTOs.{module}\n"
        "{\n"
        f"    public class {entity_name}Dto : BaseDto<{entity_name}Dto>, IDto, IAuditDto, IAutoCrud\n"
        "    {\n"
        + '\n'.join(lines) + "\n\n"
        "        public string DisplayName => \"TODO: Set display name\"; // TODO: Update this\n"
        "    }\n"
        "}\n"
    )


def generate_mapper_content():
    settable_props = [prop for prop in props if prop['accessor'] != 'init']
    map_to_dto = (
        "                Id = entity.Id,\n"
        + ''.join(f"                {prop['name']} = {dto_property_value(prop)},\n" for prop in props)
        + "                CreatedDate = entity.CreatedDate,\n"
        + "                CreatedBy = entity.CreatedBy,\n"
        + "                ModifiedDate = entity.ModifiedDate,\n"
        + "                LastModifiedBy = entity.LastModifiedBy\n"
    )
    map_from_dto = (
        "                Id = dto.Id,\n"
        + ''.join(f"                {prop['name']} = {entity_property_value(prop)},\n" for prop in props)
        + "                CreatedDate = dto.CreatedDate,\n"
        + "                CreatedBy = dto.CreatedBy,\n"
        + "                ModifiedDate = dto.ModifiedDate,\n"
        + "                LastModifiedBy = dto.LastModifiedBy\n"
    )
    assign_dto_to_entity = (
        "            entity.Id = dto.Id;\n"
        + ''.join(f"            entity.{prop['name']} = {entity_property_value(prop)};\n" for prop in settable_props if prop['name'] not in {'Id', 'CreatedDate', 'CreatedBy', 'ModifiedDate', 'LastModifiedBy'})
        + "            entity.CreatedDate = dto.CreatedDate;\n"
        + "            entity.CreatedBy = dto.CreatedBy;\n"
        + "            entity.ModifiedDate = dto.ModifiedDate;\n"
        + "            entity.LastModifiedBy = dto.LastModifiedBy;\n"
    )
    assign_entity_to_dto = (
        "            dto.Id = entity.Id;\n"
        + ''.join(f"            dto.{prop['name']} = {dto_property_value(prop)};\n" for prop in settable_props if prop['name'] not in {'Id', 'CreatedDate', 'CreatedBy', 'ModifiedDate', 'LastModifiedBy'})
        + "            dto.CreatedDate = entity.CreatedDate;\n"
        + "            dto.CreatedBy = entity.CreatedBy;\n"
        + "            dto.ModifiedDate = entity.ModifiedDate;\n"
        + "            dto.LastModifiedBy = entity.LastModifiedBy;\n"
    )
    return (
        "using Boxty.ServerBase.Mappers;\n"
        f"using Boxty.ServerApp.Modules.{module}.Entities;\n"
        f"using Boxty.SharedApp.DTOs.{module};\n"
        "using System.Security.Claims;\n"
        + ("using System.Collections.Frozen;\n" if any('FrozenDictionary<' in prop['type'] for prop in props) else '') +
        "\n"
        f"namespace Boxty.ServerApp.Modules.{module}.Infrastructure.Mappers\n"
        "{\n"
        f"    public class {entity_name}Mapper : IMapper<{entity_name}, {entity_name}Dto>\n"
        "    {\n"
        f"        public {entity_name}Dto Map({entity_name} entity, ClaimsPrincipal? user = null)\n"
        "        {\n"
        "            return new " + f"{entity_name}Dto" + "\n"
        "            {\n"
        + map_to_dto + "\n"
        + "            };\n"
        + "        }\n\n"
        + f"        public {entity_name} Map({entity_name}Dto dto, ClaimsPrincipal? user = null)\n"
        + "        {\n"
        + "            return new " + f"{entity_name}" + "\n"
        + "            {\n"
        + map_from_dto + "\n"
        + "            };\n"
        + "        }\n\n"
        + f"        public IEnumerable<{entity_name}Dto> Map(IEnumerable<{entity_name}> entities, ClaimsPrincipal? user = null)\n"
        + "        {\n"
        + "            return entities.Select(entity => Map(entity, user));\n"
        + "        }\n\n"
        + f"        public IEnumerable<{entity_name}> Map(IEnumerable<{entity_name}Dto> dtos, ClaimsPrincipal? user = null)\n"
        + "        {\n"
        + "            return dtos.Select(dto => Map(dto, user));\n"
        + "        }\n\n"
        + f"        public void Map({entity_name}Dto dto, {entity_name} entity, ClaimsPrincipal? user = null)\n"
        + "        {\n"
        + assign_dto_to_entity
        + "        }\n\n"
        + f"        public void Map({entity_name} entity, {entity_name}Dto dto, ClaimsPrincipal? user = null)\n"
        + "        {\n"
        + assign_entity_to_dto
        + "        }\n"
        + "    }\n"
        + "}\n"
    )


entity_path.parent.mkdir(parents=True, exist_ok=True)
dto_path.parent.mkdir(parents=True, exist_ok=True)
mapper_path.parent.mkdir(parents=True, exist_ok=True)

entity_path.write_text(generate_entity_content())
dto_path.write_text(generate_dto_content())
mapper_path.write_text(generate_mapper_content())
PY

    print_success "Fields added to entity, DTO and mapper for module '$module' and entity '$entity_name'."
}

main() {
    if [[ "${1:-}" == "-h" || "${1:-}" == "--help" || "${1:-}" == "help" ]]; then
        show_help
        exit 0
    fi

    echo "================================" >&2
    print_info "Field Generator Script" >&2
    echo "================================" >&2
    echo "" >&2

    local module
    module=$(get_module)
    print_success "Selected module: $module" >&2

    local entity_name
    entity_name=$(get_entity_name)
    print_success "Entity name: $entity_name" >&2

    local field_input
    field_input=$(get_field_input)
    print_success "Field definitions captured." >&2

    echo "" >&2
    print_warning "This will update:" >&2
    echo "  - Entity: $MODULES_DIR/$module/Entities/${entity_name}.cs" >&2
    echo "  - DTO:    $SHARED_DTO_DIR/$module/${entity_name}Dto.cs" >&2
    echo "  - Mapper: $MODULES_DIR/$module/Infrastructure/Mappers/${entity_name}Mapper.cs" >&2
    echo "" >&2
    read -p "Continue? (y/n): " confirm >&2

    if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
        print_info "Cancelled by user" >&2
        exit 0
    fi

    update_files "$module" "$entity_name" "$field_input"

    echo "" >&2
    print_success "Field generation complete!" >&2
    echo "" >&2
    print_info "Review the updated files and adjust any property defaults or navigation mappings as needed." >&2
}

main "$@"
