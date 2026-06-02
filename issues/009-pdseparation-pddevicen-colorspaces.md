### Title
Implement `PDSeparation` and `PDDeviceN` color spaces

### Summary

The separation and DeviceN color space classes (`PDSeparation` and `PDDeviceN`) are not ported to the .NET library. These are standard PDF color spaces used for spot colours (e.g., Pantone), multi-component inks, and special rendering intents.

### Missing classes

| Java class | Purpose |
|---|---|
| `PDSeparation` | `[/Separation name alternateSpace tintTransform]` — single spot color |
| `PDDeviceN` | `[/DeviceN names alternateSpace tintTransform attributes]` — multi-ink color space |

Key constructors and methods needed:
- `PDSeparation(string name, PDColorSpace alternateCS, PDFunction tintTransform)` — construct a separation color space.
- `PDDeviceN(List<string> names, PDColorSpace alternateCS, PDFunction tintTransform)` — construct a DeviceN color space.
- `PDSeparation.GetColorSpaceName()` — return the spot color name.
- Both classes must be registered and returned correctly from `PDColorSpace.Create(COSBase)`.

### Affected example files (currently stubs)

- `PDModel/CreateSeparationColorBox.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDSeparation.java`
`pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceN.java`

### Acceptance criteria

- `PDSeparation` and `PDDeviceN` are ported with correct COS dictionary construction.
- `PDColorSpace.Create` dispatches to them for `/Separation` and `/DeviceN` array entries.
- `PDModel/CreateSeparationColorBox.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
