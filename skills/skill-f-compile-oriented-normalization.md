# Skill F - Compile-oriented normalization pass

## Purpose
Apply a constrained, post-mechanical normalization pass so converted files are closer to valid, compilable C# while preserving provenance and parity intent.

## When to run
- After Skill A for newly converted files.
- After Skill B for updated files with significant syntax/API drift.

## Inputs
- Mechanically converted C# files (with provenance headers)
- Current compile diagnostics (such as `dotnet build` output)
- Mapping/provenance records

## Output
- Updated C# files with compile-oriented fixes
- Normalization record per file
- Updated status for Skill E reporting

## Allowed normalization categories
- Java syntax leftovers -> C# syntax equivalents.
- Namespace/type qualification fixes needed for C# compilation.
- Primitive/collection/type alias fixes required by C# type system.
- Exception/using/disposable pattern adjustments only when required to compile.
- Framework/test harness upgrades required by repository policy (for example .NET target framework updates and xUnit major version migration).
- **Closeable → IDisposable**: Java's `java.io.Closeable` maps to .NET's `System.IDisposable`. Any interface extending `Closeable` must also extend `IDisposable`. Add a default `void IDisposable.Dispose() => Close();` to the interface so implementing classes get `IDisposable` for free. When multiple `IDisposable`-bearing interfaces are combined in one super-interface, only one should carry the default `Dispose()` implementation to avoid C# diamond-inheritance errors.
- **java.nio.ByteBuffer / FileChannel → byte[] / FileStream**: Replace `ByteBuffer` position/limit/flip semantics with explicit index variables on `byte[]` arrays. Replace `FileChannel` with `FileStream`. See **Skill G §§2–4** for the complete substitution table.
- **java.util.BitSet → custom int[] bit-array**: .NET's `System.Collections.BitArray` has no `NextSetBit(from)` equivalent. Replace with a custom `int[]`-backed helper set. See **Skill G §5**.
- **java.util.LinkedHashMap with removeEldestEntry (LRU) → custom LruCache class**: Implement as a nested `private sealed class` using `Dictionary<K, LinkedListNode<(K,V)>>` + `LinkedList<(K,V)>`. See **Skill G §6**.
- **java.io.RandomAccessFile → FileStream with FileOptions.RandomAccess**: See **Skill G §1**.
- **Memory-mapped files — empty file guard**: `MemoryMappedFile.CreateFromFile` throws for zero-length files. Add `if (_size > 0)` guard and explicit `_isClosed` flag. See **Skill G §4**.
- **Test assertion char-literal widening**: `Assert.Equal('A', reader.Read())` does not compile in xUnit v3 (no implicit char→int widening). Cast to `(int)'A'` for `int`-returning methods or `(byte)'A'` for `byte[]` element comparisons. See **Skill G §14**.

## Disallowed changes in this skill
- Functional redesign of behavior.
- Public API redesign for ".NET-feeling" ergonomics.
- Silent semantic changes without note.
- Removing upstream license/provenance/doc traceability content from mechanically ported files.

## Required normalization record fields (per file)
- `source_path`
- `target_path`
- `normalization_applied` (`true` | `false`)
- `change_categories` (list)
- `semantic_risk` (`low` | `medium` | `high`)
- `compiles_after_change` (`true` | `false`)
- `note`
