# Skill G - Java → C# API and type mapping reference

## Purpose
Provide a definitive lookup table for recurring Java-to-C# API and type substitutions
observed in the PDFBox I/O port.  Use this alongside Skill A (initial conversion) and
Skill F (normalization) whenever the upstream Java file uses any of the patterns listed
below.

---

## 1. I/O – `java.io.RandomAccessFile`

| Java | C# |
|---|---|
| `new RandomAccessFile(file, "rw")` | `new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.RandomAccess)` |
| `raf.seek(long)` | `stream.Seek(offset, SeekOrigin.Begin)` |
| `raf.length()` | `stream.Length` |
| `raf.setLength(long)` | `stream.SetLength(long)` |
| `raf.readFully(byte[])` | Loop: `while (read < n) read += stream.Read(buf, read, n - read)` |
| `raf.write(byte[])` | `stream.Write(buf, 0, buf.Length)` |
| `raf.close()` | `stream.Close()` |

**Note**: `FileStream.Read` does not guarantee a full read like `readFully`.
Always wrap in a loop.

---

## 2. I/O – `java.nio.channels.FileChannel` + `StandardOpenOption`

| Java | C# |
|---|---|
| `FileChannel.open(path, StandardOpenOption.READ)` | `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.RandomAccess)` |
| `channel.size()` | `stream.Length` |
| `channel.position(long)` | `stream.Seek(long, SeekOrigin.Begin)` |
| `channel.read(byteBuffer)` | `stream.Read(page, offset, count)` in a loop |
| `channel.close()` | `stream.Close()` (or `using` block / `Dispose()`) |

---

## 3. I/O – `java.nio.ByteBuffer`

`ByteBuffer` has no direct .NET equivalent.  The position/limit/flip model must be
translated to explicit index arithmetic on `byte[]`.

| Java ByteBuffer pattern | C# equivalent |
|---|---|
| `ByteBuffer.allocate(n)` | `new byte[n]` |
| `ByteBuffer.clear()` (reset for reuse) | reuse array reference; reset offset variables to 0 |
| `buf.get()` (read one byte, advance) | `page[offset++] & 0xff` |
| `buf.get(dst, off, len)` | `Array.Copy(page, srcOffset, dst, off, len)` |
| `buf.position()` | `offset` field |
| `buf.position(n)` | `offset = n` |
| `buf.capacity()` / `PAGE_SIZE` | array `.Length` / constant |
| `buf.duplicate()` for view | see §5 (memory-mapped) |

**Consequence**: Java's `LinkedHashMap<Long, ByteBuffer>` LRU page cache
(backed by `ByteBuffer` objects) becomes a custom C# `LruPageCache` inner class
using `Dictionary<long, LinkedListNode<(long, byte[])>>` + `LinkedList<>`.

---

## 4. I/O – `java.nio` memory-mapped files

| Java | C# |
|---|---|
| `FileChannel.map(MapMode.READ_ONLY, 0, size)` | `MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, Read)` + `.CreateViewAccessor(0, size, Read)` |
| `MappedByteBuffer.duplicate()` (independent position, shared data) | `mappedFile.CreateViewAccessor(0, size, Read)` – new accessor on same `MemoryMappedFile` |
| `MappedByteBuffer.rewind()` | `_position = 0` |
| `IOUtils::unmap` (unsafe JVM cleaner) | `accessor.Dispose()` + `mappedFile.Dispose()` on owner |
| `Consumer<ByteBuffer> unmapper` + `Optional.ifPresent` | null-conditional `Dispose()` |
| `Optional.ofNullable(x).ifPresent(u -> u.accept(y))` | `x?.Invoke(y)` or simple null check |

**Critical .NET edge case – empty files:**
`FileChannel.map(READ_ONLY, 0, 0)` works in Java.
`MemoryMappedFile.CreateFromFile` **throws** `ArgumentException` for zero-length files.
Guard:
```csharp
if (_size > 0)
{
    _mappedFile = MemoryMappedFile.CreateFromFile(path, ...);
    _accessor = _mappedFile.CreateViewAccessor(0, _size, Read);
}
```
Add an explicit `bool _isClosed` field so `IsClosed()` can distinguish "empty file
(accessor is null but not closed)" from "closed (accessor disposed)".

---

## 5. Collections – `java.util.BitSet`

`BitSet.nextSetBit(fromIdx)` has no efficient .NET equivalent.  Replace with a
custom `int[]` bit-array and the following helper methods:

```csharp
private void SetFreeBits(int from, int toExclusive) { ... }
private void ClearFreeBit(int idx)                  { ... }
private bool GetFreeBit(int idx)                    { ... }
private int  NextSetBit(int fromIdx)                { ... }
private void EnsureBitCapacity(int minBits)         { ... }
private static int TrailingZeroCount(int x)         { ... }
```

`BitSet.set(from, to)` → `SetFreeBits(from, toExclusive)`  
`BitSet.clear(idx)` → `ClearFreeBit(idx)`  
`BitSet.get(idx)` → `GetFreeBit(idx)`  
`BitSet.nextSetBit(from)` → `NextSetBit(from)` (returns -1 when none found)

---

## 6. Collections – `java.util.LinkedHashMap` with `removeEldestEntry` (LRU cache)

Java's subclassing trick for an LRU map has no .NET equivalent.  Replace with a
nested `private sealed class LruCache` combining `Dictionary<K, LinkedListNode<(K, V)>>`
and `LinkedList<(K, V)>`.

Pattern:
```csharp
private sealed class LruPageCache(int capacity)
{
    private readonly Dictionary<long, LinkedListNode<(long key, byte[] page)>> _map = new();
    private readonly LinkedList<(long key, byte[] page)> _list = new();
    private byte[]? _recycled;

    public byte[]? Get(long key) { /* move to tail; return value */ }
    public void Put(long key, byte[] page) { /* add to tail; evict head when over capacity */ }
    public byte[]? TakeRecycledBuffer() { /* return and clear _recycled */ }
    public void Clear() { /* clear all */ }
}
```
When evicting, stash the evicted `byte[]` as `_recycled` so `ReadPage()` can
reuse the buffer instead of allocating.

---

## 7. Threading – `synchronized` / thread identity

| Java | C# |
|---|---|
| `synchronized(obj) { ... }` | `lock(obj) { ... }` |
| `volatile long field;` | `volatile long field;` (same keyword) |
| `Thread.currentThread().getId()` | `Environment.CurrentManagedThreadId` (returns `int`; widen to `long` if needed) |
| `ConcurrentHashMap<K,V>` | `ConcurrentDictionary<K,V>` |

---

## 8. Exceptions

| Java | C# |
|---|---|
| `java.io.IOException` | `System.IO.IOException` |
| `java.io.EOFException` | `System.IO.EndOfStreamException` |
| `java.io.FileNotFoundException` | `System.IO.FileNotFoundException` |
| `throws IOException` on method signature | remove — C# uses unchecked exceptions |
| Multi-catch `catch (IOException ioe) { ... }` | same pattern; store first exception only: `catch (IOException ioe) when (ioexc == null)` |

---

## 9. File-system utilities – `java.io.File` / `java.nio.file.Path`

| Java | C# |
|---|---|
| `new File(path)` | `new FileInfo(path)` or just `string path` |
| `file.isDirectory()` | `new DirectoryInfo(path).Exists` |
| `file.getAbsolutePath()` | `fileInfo.FullName` |
| `file.delete()` | `File.Delete(path)` (call `fileInfo.Refresh()` first if using `FileInfo`) |
| `file.exists()` | `File.Exists(path)` or `fileInfo.Exists` |
| `file.toPath()` | `path` (string) |
| `Paths.get(str)` | string literal or `Path.Combine(...)` |
| `Path.toFile()` | string path |

---

## 10. PDFBox utility class `IOUtils`

| Java IOUtils call | C# equivalent |
|---|---|
| `IOUtils.createProtectedTempFile(null, "PDFBox", ".tmp")` | `Path.GetTempFileName()` *(note: no permission restriction like Java's 600)* |
| `IOUtils.createProtectedTempFile(dir, "PDFBox", ".tmp")` | `Path.Combine(dir, "PDFBox" + Path.GetRandomFileName() + ".tmp")` |
| `IOUtils.closeQuietly(closeable)` | `try { x.Close(); } catch { /* ignore */ }` |
| `IOUtils::unmap` (unsafe ByteBuffer cleaner) | `accessor.Dispose()` (see §4) |

---

## 11. Logging – Log4j / SLF4J

Log4j has no direct .NET equivalent in this codebase yet.  For now:

| Java | C# |
|---|---|
| `private static final Logger LOG = LogManager.getLogger(X.class)` | **remove** — no logging infrastructure yet |
| `LOG.error(() -> "msg: " + e, e)` | **remove** — document as `PORT-LOCAL` region if added later |
| `LOG.warn("msg {}", val)` | **remove** |

Add a `// PORT-LOCAL-START / PORT-LOCAL-END` region when logging is added
post-conversion so it is preserved across re-syncs.

---

## 12. Primitive types and standard library

| Java | C# |
|---|---|
| `Integer.MAX_VALUE` | `int.MaxValue` |
| `Long.MAX_VALUE` | `long.MaxValue` |
| `Math.min(a, b)` | `Math.Min(a, b)` |
| `Math.max(a, b)` | `Math.Max(a, b)` |
| `System.arraycopy(src, srcOff, dst, dstOff, n)` | `Array.Copy(src, srcOff, dst, dstOff, n)` |
| `(int) Math.min(int.MAX_VALUE, longVal)` | `(int)Math.Min(int.MaxValue, longVal)` |
| `int[] arr = new int[n]; Arrays.fill(arr, 0)` | `new int[n]` (zero-initialized by default) |
| `System.currentTimeMillis()` | `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()` |
| `b & 0xff` (byte to unsigned int) | `b & 0xff` (same) |

---

## 13. Visibility – package-private

Java package-private (no modifier) maps to C# `internal`:

| Java | C# |
|---|---|
| `class Foo { ... }` (package-private class) | `internal class Foo { ... }` |
| `void doThing()` (package-private method) | `internal void DoThing()` |
| `int field;` (package-private field) | `internal int _field;` |

---

## 14. Testing – `char` literal widening in assertions

Java's `char` is implicitly widened to `int` in arithmetic / comparison contexts.
xUnit's `Assert.Equal<T>` uses generic overload resolution and does **not** widen
`char` to `int`/`byte` automatically.

| Java JUnit | C# xUnit v3 (correct) |
|---|---|
| `assertEquals('5', randomAccess.read())` | `Assert.Equal((int)'5', randomAccess.Read())` |
| `assertEquals('0', buffer[0])` (`buffer` is `byte[]`) | `Assert.Equal((byte)'0', buffer[0])` |
| `assertEquals('A', peek())` | `Assert.Equal((int)'A', reader.Peek())` |

**Rule**: Cast char literals to the **expected return type** of the method under test.
`Read()` / `Peek()` return `int` → cast to `(int)'x'`.
`byte[]` element comparison → cast to `(byte)'x'`.

---

## 15. Provenance header – block comment format

All existing C# files in this port use the following **block comment** format
(not single-line `//` comments as shown in the worked example):

```csharp
/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/ScratchFile.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  ...
 */
```

Always place the provenance block **before** the Apache license block.

---

## 16. `PORT_MODE` selection guide

| Situation | PORT_MODE |
|---|---|
| Direct structural+behavioral 1:1 translation, only Java syntax→C# syntax changes | `mechanical` |
| Platform API replaced (e.g. `ByteBuffer`→`byte[]`) but **behavior preserved** | `mechanical` *(note the substitution in the traceability note field)* |
| Behavior intentionally changed (e.g. richer .NET API surface, culture-invariant formatting) | `adapted` |
| Upstream logic removed or replaced by a fundamentally different algorithm | `adapted` |
