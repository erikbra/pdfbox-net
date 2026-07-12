namespace PdfBox.Net.Layout;

/// <summary>
/// Semantic AcroForm field metadata and the geometry of one widget placement.
/// </summary>
public sealed class PdfLayoutFormControl
{
    public PdfLayoutFormControl(
        int index,
        string name,
        string accessibleName,
        PdfLayoutFormControlKind kind,
        PdfLayoutRectangle bounds,
        IReadOnlyList<string>? values = null,
        IReadOnlyList<string>? defaultValues = null,
        IReadOnlyList<PdfLayoutFormOption>? options = null,
        bool isReadOnly = false,
        bool isRequired = false,
        bool isChecked = false,
        bool isDefaultChecked = false,
        bool isMultiline = false,
        bool isPassword = false,
        bool isMultiple = false,
        int? maxLength = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessibleName);

        Index = index;
        Name = name;
        AccessibleName = accessibleName;
        Kind = kind;
        Bounds = bounds;
        Values = values?.ToArray() ?? [];
        DefaultValues = defaultValues?.ToArray() ?? [];
        Options = options?.ToArray() ?? [];
        IsReadOnly = isReadOnly;
        IsRequired = isRequired;
        IsChecked = isChecked;
        IsDefaultChecked = isDefaultChecked;
        IsMultiline = isMultiline;
        IsPassword = isPassword;
        IsMultiple = isMultiple;
        MaxLength = maxLength is > 0 ? maxLength : null;
    }

    /// <summary>Gets the zero-based control index on the page.</summary>
    public int Index { get; }

    /// <summary>Gets the stable fully qualified AcroForm name, or a deterministic fallback.</summary>
    public string Name { get; }

    /// <summary>Gets the field tooltip when present, otherwise <see cref="Name"/>.</summary>
    public string AccessibleName { get; }

    /// <summary>Gets the semantic control kind.</summary>
    public PdfLayoutFormControlKind Kind { get; }

    /// <summary>Gets the widget bounds in normalized page coordinates.</summary>
    public PdfLayoutRectangle Bounds { get; }

    /// <summary>Gets the current field values.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Gets the default field values.</summary>
    public IReadOnlyList<string> DefaultValues { get; }

    /// <summary>Gets choice options or the export value for a button widget.</summary>
    public IReadOnlyList<PdfLayoutFormOption> Options { get; }

    /// <summary>Gets whether the PDF field is read-only.</summary>
    public bool IsReadOnly { get; }

    /// <summary>Gets whether the PDF field is required.</summary>
    public bool IsRequired { get; }

    /// <summary>Gets whether this checkbox or radio widget is currently selected.</summary>
    public bool IsChecked { get; }

    /// <summary>Gets whether this checkbox or radio widget is selected by default.</summary>
    public bool IsDefaultChecked { get; }

    /// <summary>Gets whether a text field accepts multiple lines.</summary>
    public bool IsMultiline { get; }

    /// <summary>Gets whether a text field masks its value.</summary>
    public bool IsPassword { get; }

    /// <summary>Gets whether a choice field permits multiple selections.</summary>
    public bool IsMultiple { get; }

    /// <summary>Gets the maximum text length when specified.</summary>
    public int? MaxLength { get; }
}
