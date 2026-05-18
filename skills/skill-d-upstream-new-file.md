# Skill D - Upstream new-file intake

## Purpose
Onboard upstream Java files newly added in tracked modules into the .NET port.

## Inputs
- Set of new upstream Java files
- Target module/path mapping rules
- Upstream commit SHA

## Output
- New mapped C# files
- Provenance headers stamped in each new file
- Parity/backlog records updated for newly tracked files

## Mapping rules (required)
- Path rule: keep relative package structure where practical.  
  Example: `org/apache/pdfbox/util/StringUtil.java` -> `src/PdfBox/Util/StringUtil.cs`
- Namespace rule: convert package path to PascalCase namespace segments.  
  Example: `org.apache.pdfbox.util` -> `Org.Apache.PdfBox.Util`
- Class/file rule: keep one primary type per file with matching file name.

## Required intake record fields (per file)
- `source_path`
- `target_path`
- `source_commit`
- `namespace`
- `status` (`converted` | `backlog`)
- `note`
