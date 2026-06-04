# Issue 017 — Ant/MSBuild task integration

## Summary

Implement an MSBuild task equivalent to the upstream Java `PDFToTextTask` Ant task so that
the Ant example file compiles and functions as a .NET build tool integration.

## Background

The upstream Java example `PDFToTextTask.java` extends `org.apache.tools.ant.Task` to expose
`PDFTextStripper` as an Ant build task. In .NET, the appropriate target is an MSBuild custom
task (`Microsoft.Build.Utilities.Task`).

## Required API surface

- `PDFToTextTask` as an MSBuild custom task extending `Microsoft.Build.Utilities.Task`:
  - `InputFile` — path to the source PDF (`ITaskItem`)
  - `OutputFile` — path to the text output file (`ITaskItem`)
  - `Execute()` — calls `PDFTextStripper.GetText(PDDocument)` and writes the result
- The task should be usable from an MSBuild `.targets` / `.props` import

## Affected example files

- `Ant/PDFToTextTask.cs`

## Acceptance criteria

- `PDFToTextTask` compiles as an MSBuild task without stubs.
- A unit test (or the existing test in `tests/PdfBox.Net.Examples.Tests/Ant/TestPDFToTextTask.cs`)
  exercises the task against a sample PDF and verifies text output.
- Traceability row for the affected source path is `in-sync`.

## Notes

- The MSBuild SDK package `Microsoft.Build.Utilities.Core` should be referenced as a build-only
  dependency so it does not pollute runtime consumers.
- If the task is platform-agnostic, the test can run in a standard xUnit CI environment without
  invoking MSBuild directly.
