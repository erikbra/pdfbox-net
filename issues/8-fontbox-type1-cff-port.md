### Title
Complete `fontbox/cff/**` — charstring interpreter, full charset/encoding, and CID support

### Depends on
- #6 `fontbox/util` follow-up
- #7 `fontbox/encoding` port

### Current state
11/26 files ported (42%). The following 15 files are missing:

**Charstring interpreter infrastructure (required for glyph outline rendering):**
- `CFFOperator.java` — Operator enum/type for charstring opcodes
- `CharStringCommand.java` — Parsed charstring command
- `Type1CharStringParser.java` — Type 1 charstring parser
- `Type2CharStringParser.java` — Type 2 charstring parser
- `DataInput.java` — Low-level byte-stream interface for CFF parser
- `DataInputByteArray.java` — Byte-array backed DataInput
- `DataInputRandomAccessRead.java` — RandomAccessRead-backed DataInput

**CID-keyed font support:**
- `CFFCharsetCID.java` — CID-keyed font charset
- `FDSelect.java` — CFF FD-Select structure (maps glyph IDs to Private DICT)
- `CIDKeyedType2CharString.java` — CID-keyed Type 2 charstring
- `EmbeddedCharset.java` — Embedded CFF charset wrapper

**Expert charset/encoding tables:**
- `CFFExpertCharset.java` — Expert charset
- `CFFExpertSubsetCharset.java` — Expert subset charset
- `CFFISOAdobeCharset.java` — ISO Adobe charset
- `CFFExpertEncoding.java` — Expert encoding

### Scope
- Port all 15 missing files in dependency order.
- Expand `CFFParser.cs` to support the full charstring parser infrastructure.
- Add parser tests for Type1 and Type2 charstrings, CID-keyed fonts, and expert charsets.

### Exit criteria
- All 26 CFF files are ported and compile against util/encoding layers.
- Type1 and Type2 charstring parsing tests pass for included fixtures.
- CID-keyed font parsing is tested end-to-end.
- `dotnet test` remains green.
