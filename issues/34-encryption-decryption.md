### Title
Implement StandardSecurityHandler decryption flow (RC4 / AES)

### Background
`StandardSecurityHandler.PrepareForDecryption()` currently throws `NotSupportedException`.
The surrounding data model (PDEncryption, AccessPermission, AESKeyLength, MessageDigests,
PDCryptFilterDictionary) is fully ported, but the actual cryptographic decryption algorithm
(password key derivation, RC4/AES stream decryption) is not implemented.

Without this, loading any password-protected PDF will throw.

### Depends on
- PDEncryption, AccessPermission, PDCryptFilterDictionary (all ported)
- Issue #31 (full PDF loading) — encrypted PDFs require the xref/object loading pipeline

### Scope

Port the decryption flow from:
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/StandardSecurityHandler.java`
  — specifically `prepareForDecryption(PDEncryption, COSArray, DecryptionMaterial)`,
  key derivation (`computeEncryptionKey()`), and per-object stream decryption
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/SecurityHandler.java`
  — `encryptData()`, `decryptStream()`, `decryptString()` methods (base class)
- `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/RC4Cipher.java`
  — RC4 encryption (used for PDF 1.1–1.5 standard security)

.NET replacements:
- `System.Security.Cryptography.Aes` for AES-128 / AES-256 (PDF 1.6+)
- `System.Security.Cryptography.MD5` for MD5-based key derivation (revision 2/3/4)
- RC4 has no BCL equivalent; implement using `RC4Cipher.java` port or a tiny custom class

### Expected test scope
- Open a password-protected PDF (user password "test") and verify page count.
- Open a PDF with owner-only restriction and verify `AccessPermission` flags.
- Verify AES-128 decryption on a PDF 1.6 fixture.

### Entry criteria
- `dotnet build` passes.
- Issue #31 (PDF loading) functional for unencrypted PDFs.

### Exit criteria
- Standard password-protected PDFs can be loaded with the correct user password.
- `AccessPermission` is correctly populated after successful decryption.
- `PublicKeySecurityHandler` deferred (out of scope for this issue).

### Risk register
- PDF encryption specification has many revision variants (rev 2, 3, 4, 5, 6);
  implement in revision order, starting with rev 2/3 (most common).
- Key derivation involves MD5 / SHA hashing of document IDs and passwords.
- RC4 implementation must match Java byte-by-byte; use test vectors from the spec.

### Definition of done
- `dotnet build` passes.
- At least one encrypted PDF fixture test passes.
- No raw key material logged or stored beyond decryption scope.
