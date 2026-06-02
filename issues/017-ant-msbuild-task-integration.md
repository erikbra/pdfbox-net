### Title
Implement MSBuild task integration for the `Ant` examples module

### Summary

The `Ant/PDFToTextTask.cs` example wraps PDF text extraction as an Apache Ant `Task` subclass (`org.apache.tools.ant.Task`). There is no meaningful .NET equivalent for Ant tasks in the examples module.

### Options

1. **MSBuild custom task** — implement `PDFToTextTask` as an `ITask` / `Microsoft.Build.Utilities.Task` subclass that can be used in `.csproj` files.
2. **FAKE build task** — implement it as an F# FAKE build helper.
3. **Standalone CLI tool** — simplify the port to a command-line tool (the most portable option, matching how most .NET build pipelines would consume it).

The recommended approach is option 3 (standalone CLI), as it is the most compatible with .NET tooling and avoids a dependency on MSBuild APIs in the examples project.

### Affected example files (currently stubs)

- `Ant/PDFToTextTask.cs`

### Upstream Java reference

`examples/src/main/java/org/apache/pdfbox/examples/ant/PDFToTextTask.java`

### Acceptance criteria

- `Ant/PDFToTextTask.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical` using the chosen approach.
- The chosen approach and its rationale are documented in a comment at the top of the file.
