### Title
Port documentinterchange attribute objects and role/class mapping

### Background
Tagged PDF semantics rely on attribute object dictionaries and mapping tables
(`RoleMap`, `ClassMap`) to interpret structure element meaning consistently.

### Depends on
- Issue #43 (structure tree core)
- Issue #44 (reference classes)

### Scope
Port attribute and mapping types in `documentinterchange` needed for semantic parity:
- `PDAttributeObject` and concrete attribute object variants
- `PDUserAttributeObject` + user-property support
- role/class map accessors on structure tree root/element types

### Expected test scope
- Verify role map lookup for custom-to-standard structure type mapping.
- Verify attribute object roundtrip behavior on tagged fixture elements.

### Entry criteria
- Issues #43 and #44 merged.

### Exit criteria
- Attribute object and role/class map APIs are functional.
- Tagged fixture assertions can read mapped structure semantics.

### Risk register
- Attribute object subtype fan-out can introduce partial parity if not phased cleanly.
- COS numeric/string coercion differences can alter attribute values.

### Definition of done
- Build passes.
- Attribute/mapping tests pass.
- Conversion + normalization + traceability records updated.
