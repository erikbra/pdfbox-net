# Multi-generation upstream PDFBox support — feasibility assessment

Date: 2026-05-25 (UTC)

## Executive summary

Supporting both PDFBox **3.x (current stable)** and **4.x / trunk (development)** in the
C# port is **feasible and not prohibitively complex**. The two generations share the same
package/module structure, the same core PDF-processing algorithms, and the same public API
surface. The differences are concentrated in a small set of infrastructure areas.

**Recommendation:** Keep the primary development targeting trunk (4.x) but add generation
tracking via a new provenance-header field and maintain a delta reference document. A
3.x-aligned slice is practical to produce in future if a concrete user/consumer requirement
arises.

---

## 1. What was reviewed

Compared the `3.0` branch and `trunk` (4.x) branch of the upstream Apache PDFBox GitHub
repository across these areas:

| Area | Method |
|---|---|
| Logging dependency | `pdfbox/pom.xml`, `parent/pom.xml`, Java source imports |
| Core COS layer | `COSDocument.java`, `COSName.java`, `COSDictionary.java`, `COSArray.java`, `COSString.java` |
| Parser layer | `BaseParser.java`, `COSParser.java`, `PDFParser.java` |
| PDModel layer | `PDDocument.java`, `pdmodel/` directory listing |
| Utility layer | `util/` directory listing |
| Build / Java version | `parent/pom.xml` compiler settings |

---

## 2. Inventory of differences

### 2.1 Logging framework (cross-cutting, all files)

| | 3.x stable | 4.x trunk |
|---|---|---|
| Library | Apache Commons Logging 1.3.6 (`commons-logging`) | Log4j 2 (`log4j-core`) |
| Import pattern | `import org.apache.commons.logging.Log; import org.apache.commons.logging.LogFactory;` | `import org.apache.logging.log4j.Logger; import org.apache.logging.log4j.LogManager;` |
| Usage pattern | `private static final Log LOG = LogFactory.getLog(SomeClass.class);` | `private static final Logger LOG = LogManager.getLogger(SomeClass.class);` |

**C# impact: None.** Both Java logging frameworks map to `Microsoft.Extensions.Logging.ILogger`
in the C# port. The logging infrastructure change is already fully abstracted away. No ported
C# files need modification when tracking either generation.

### 2.2 Java source / compile level

| | 3.x stable | 4.x trunk |
|---|---|---|
| Compiler source | not pinned (de-facto Java 8) | Java 11 (`maven.compiler.source=11`) |
| New Java APIs used | None beyond Java 8 | `java.lang.ref.Cleaner` (Java 9+, see COSName) |

**C# impact: None.** The C# port targets .NET, not Java. Java version requirements only
matter when evaluating whether a Java API needs a .NET equivalent. The one confirmed new
Java API (`Cleaner`, used in `COSName`) already has a C# analogue (see §2.3).

### 2.3 COSName — interning mechanism refactored

This is the most substantive internal change found.

| | 3.x stable | 4.x trunk |
|---|---|---|
| Cache structure | Two maps: `nameMap` (`ConcurrentHashMap<ByteBuffer, COSName>`) + `commonNameMap` (`HashMap<ByteBuffer, COSName>`) | Single map: `NAME_MAP` (`ConcurrentHashMap<ByteBuffer, WeakReference<COSName>>`) + `CLEANER` (`java.lang.ref.Cleaner`) |
| Static constants | Created with `new COSName("...")` syntax | Created with `getPDFName("...")` factory |
| Memory model | Strong references for static constants and common names | Weak references + GC-driven cleanup via `Cleaner` |

**Motivation:** The trunk change reduces static memory footprint by allowing non-constant
`COSName` instances to be GC-collected when no longer referenced.

**C# impact: Low-moderate.** The C# port's `COSName.cs` was ported from trunk and already
uses `getPDFName`-style factory semantics. A 3.x backport would need a dual-map caching
strategy instead of the WeakReference approach. Both approaches are valid and correct; the
choice does not affect the public API surface of `COSName`.

### 2.4 BaseParser — logic redistribution

A significant size reduction in `BaseParser.java` between 3.x and trunk was observed:

| | 3.x stable | 4.x trunk |
|---|---|---|
| `BaseParser.java` size | ~48,915 bytes | ~19,863 bytes |
| `COSParser.java` size | ~70,631 bytes | ~72,771 bytes |

In 3.x, `BaseParser` contains low-level COS-object parsing methods (parsing
`COSArray`, `COSDictionary`, `COSString`, `COSBoolean`, recursive object resolution,
etc.). In trunk, those methods were moved into `COSParser`, leaving `BaseParser` with
only basic byte-level stream helpers.

**C# impact: Moderate.** The C# port was based on trunk, so `BaseParser.cs` already
reflects the slimmer trunk structure. A 3.x backport would require the opposite layout
(putting parse-object logic back into `BaseParser`). This is the most structurally
divergent area between the two generations in the parser layer.

### 2.5 SmallMap removed in trunk

| | 3.x stable | 4.x trunk |
|---|---|---|
| `util/SmallMap.java` | Present (10,790 bytes) | **Absent** |

`SmallMap` is a compact fixed-capacity `Map` optimisation for dictionaries with very
few entries. It was removed from trunk. The gap analysis in `reports/pdfbox-main-gap-analysis.md`
listed `SmallMap.java` as a remaining gap to port — this finding confirms it does not need
to be ported for trunk compatibility.

**C# impact: Low.** A 3.x-targeting port would need to add this class. For the current
trunk-targeted port it simply does not exist and is not required.

### 2.6 Module / package structure — identical

Both generations share the same published Maven artifact structure:

| Maven artifact | .NET project (this port) |
|---|---|
| `pdfbox-io` | `PdfBox.Net.IO` |
| `fontbox` | `PdfBox.Net.FontBox` |
| `pdfbox` | `PdfBox.Net` |

Directory / package structure within each artifact is **identical** between 3.x and trunk.
No class was found that exists in one generation but not the other (except `SmallMap`, §2.5).

### 2.7 PDModel layer — essentially unchanged

The `pdmodel/` tree (root files + all subdirectories) contains the same files in both
generations. File sizes differ by a few hundred bytes on several files, reflecting incremental
refinements and log-statement changes, not structural redesign.

### 2.8 Minor API / behavioural changes

Spot-check of `COSParser.java` imports reveals trunk adds:
- `java.io.ByteArrayOutputStream`
- `java.nio.ByteBuffer`
- `java.nio.charset.CharacterCodingException`, `Charset`, `CharsetDecoder`, `CodingErrorAction`

These imports were present in 3.x `BaseParser.java` and moved to trunk `COSParser.java` as
part of the BaseParser refactoring (§2.4), not as new functionality.

---

## 3. Summary table

| Difference | Scope | C# impact | Complexity to support both |
|---|---|---|---|
| Logging framework | All files with logging | None (already abstracted) | Zero |
| Java version (Java 11 target) | `COSName` Cleaner usage | None (C# targets .NET) | Zero |
| COSName WeakReference vs. dual-map | `COSName.cs` | Low | Low |
| BaseParser logic redistribution | `BaseParser.cs`, `COSParser.cs` | Moderate (structural layout) | Moderate |
| SmallMap absent in trunk | `SmallMap.cs` (not yet ported) | Low | Low |
| Module/package structure | All projects | **None — identical** | Zero |
| PDModel layer | All pdmodel files | None to low | Zero to low |

**Overall complexity: Low-to-moderate.** The differences are concentrated in two files
(`COSName` and `BaseParser/COSParser`). No new packages or public API surface changes
were found.

---

## 4. Strategy options

### Option A — Primary trunk (4.x) with documented 3.x delta

Continue tracking trunk as the primary source. Produce and maintain a
**3.x delta reference document** that records the known divergence points (COSName caching,
BaseParser layout, SmallMap) so a consumer who needs 3.x-aligned behaviour can apply those
adaptations manually.

**Pros:** No extra branches or parallel codebase. Minimal added process overhead.

**Cons:** 3.x-specific bug-fixes in the stable branch do not automatically flow in. Manual
cross-check required to catch 3.x-only fixes.

**Recommended when:** The primary audience is willing to use trunk/4.x when it ships as
stable, or there is no active user requirement for 3.x behavior.

### Option B — Separate `gen-3.x` Git branch

Fork the current port into a dedicated `gen-3.x` Git branch based on the `3.0` Java
source and keep `main` tracking trunk.

**Pros:** Cleanly separates 3.x and 4.x work. Each branch has a 1:1 relationship to
its upstream generation.

**Cons:** Doubles maintenance load. Structural divergence (BaseParser layout) means
cross-branch merges will be non-trivial. Team needs to decide which branch to stabilise
first.

**Recommended when:** There are confirmed end-users requiring stable-release 3.x
behavior in production, with a separate team able to own the `gen-3.x` branch.

### Option C — Single codebase with per-file generation annotations

Annotate files that have generation-specific divergence with inline comments or a light
"generation delta note". Skill B (upstream sync) is extended to handle two possible
upstream source references when a file differs between 3.x and 4.x.

**Pros:** No branch proliferation. Generation awareness is kept in the existing
traceability system.

**Cons:** Per-file overhead. Requires Skill A / Skill B updates. Dual-sync of divergent
files is more complex.

**Recommended when:** The team wants to be generation-aware without forking the
repository or managing a parallel branch.

---

## 5. Recommendation

**Adopt Option A — primary trunk (4.x) only, with this report as a reference if 3.x
ever becomes a concrete requirement.**

Rationale:
1. The port is ~83% complete tracking trunk. Introducing 3.x parallel work now splits
   focus and risks slowing down the remaining 17%.
2. The structural differences between 3.x and 4.x in the C# port are small (2–3 files
   with meaningful divergence).
3. A full 3.x backport slice (the files in §2.3, §2.4, §2.5) can be delivered in a
   focused PR of approximately **3–5 days effort** once a concrete 3.x user requirement
   is confirmed.

### Immediate actionable steps

1. Keep this feasibility assessment in `reports/` as a reference for the known delta
   points should a 3.x requirement arise.
2. Continue targeting trunk (4.x) exclusively until a concrete consumer requirement
   mandates 3.x behavioural parity.

---

## 6. Effort estimate for a 3.x backport slice

If a decision is made to port the 3.x generation:

| Area | Delta work | Effort estimate |
|---|---|---|
| `COSName.cs` — dual-map caching strategy | Replace WeakReference with dual-map + strong refs for static constants | 0.5 day |
| `BaseParser.cs` — move parse-object logic back in | Port COS-parse methods from 3.x `BaseParser.java` into C# `BaseParser.cs` | 1–2 days |
| `SmallMap.cs` — new file | Port `SmallMap.java` from 3.x | 0.5 day |
| Provenance header sync commit updates | Update sync commit SHAs for affected files | 0.5 day |
| Test parity validation | Run tests, update reports | 0.5 day |
| **Total** | | **3–4.5 days** |

This is well within a single focused sprint and does **not** introduce ridiculous complexity.

---

## 7. Answer to the issue question

> Is this feasible, or does it introduce a ridiculous amount of complexity?

**It is feasible. The complexity is low-to-moderate, not ridiculous.**

The two generations share the same module structure, package layout, and core PDF
processing algorithms. The C# port abstracts away the main cross-cutting difference
(logging framework) entirely. The remaining divergence affects only 2–3 files and
amounts to a manageable backport slice once a concrete requirement exists.

The single strongest reason **not** to support both simultaneously right now is
opportunity cost: the port is still completing the remaining ~17% of trunk, and splitting
focus before that baseline is stable would slow delivery without a confirmed consumer need.

The recommendation is to **complete the trunk-targeting port first, then produce a 3.x
generation slice on demand** if a concrete consumer requirement arises.
