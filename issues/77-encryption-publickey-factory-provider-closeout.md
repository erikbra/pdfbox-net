### Title
Close encryption parity with public-key, factory, provider, and password-support backfill

### Depends on
- #24 interactive/encryption baseline or equivalent merged security foundation
- #67 digital signatures and visible-signature support

### Background
The remaining encryption files are concentrated in public-key support, security factories/providers,
and password-handling helpers. They should land as a single security closeout milestone instead of
being reopened across later feature slices.

### Scope
- Port the remaining encryption support files:
  - `InvalidPasswordException`
  - `PublicKeyDecryptionMaterial`
  - `PublicKeyProtectionPolicy`
  - `PublicKeyRecipient`
  - `PublicKeySecurityHandler`
  - `SaslPrep`
  - `SecurityHandlerFactory`
  - `SecurityProvider`
- Keep standard-security and signature interactions regression tested in the same milestone.

### Expected test scope
- Protected-document tests for password and public-key paths where feasible.
- Security-factory/provider dispatch tests.
- Regression tests that keep signature/security integration green.

### Entry criteria
- Existing security baseline is functional and green.

### Exit criteria
- `pdmodel.encryption` reaches 19 / 19 mapped for the current parity target.
- Security support types are no longer split across partial follow-up slices.

### Risk register
- Public-key handling may depend on certificate fixtures and platform crypto behavior.
- `SaslPrep` and password normalization can introduce subtle cross-platform differences.

### Definition of done
- `dotnet build` passes.
- Encryption/security targeted tests pass.
- Coverage and traceability artifacts are refreshed.
