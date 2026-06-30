# PdfBox.Net 3.0 Branch Plan

This plan tracks the work needed to create and maintain a long-lived
`release/3.0` branch of PdfBox.Net against Apache PDFBox 3.0.

## Upstream

- Source repository: `/Users/erik/src/Repos/apache/pdfbox`
- Upstream ref: `origin/3.0`
- Upstream mode: living branch, not a fixed tag
- Initial observed upstream commit: `ea68b6feae80e671b3d26565b12eccc79e74d967`

The 3.0 branch should be retargeted to the current Apache `origin/3.0`
state whenever parity reports are regenerated. Tags such as `3.0.7` may be
used as comparison points, but they are not the branch source of truth.

## Scope

The `release/3.0` branch should aim for Apache PDFBox 3.0 core parity across
the non-Preflight modules:

- `pdfbox`
- `fontbox`
- `io`
- `xmpbox`
- `tools`
- `examples`
- `app`
- `debugger`
- `debugger-app`
- `benchmark`

The target is practical .NET parity: users should be able to follow Apache
PDFBox 3.0 documentation and examples with PdfBox.Net, modulo documented .NET
adaptations such as stream/file conventions, package split points, graphics
backends, cryptography backends, and property facade sidecar files.

## Explicit Exclusion

The following Apache 3.0 modules are excluded from the first 3.0 branch effort:

- `preflight`
- `preflight-app`

Preflight is Apache PDFBox's PDF/A validation implementation. It was removed
from Apache PDFBox trunk/4.0 and is specialized validation functionality rather
than core PDF load/render/extract/save behavior. PdfBox.Net may add PDF/A
validation later, but that should be planned as a separate package or milestone
rather than blocking the core 3.0 line.

Any source coverage or API report for `release/3.0` must show this as an
intentional exclusion instead of an accidental missing module.

## Work Queue

The work should proceed issue-by-issue in numeric order. Each issue should land
through a PR targeting `release/3.0`, include validation evidence, and use a
closing reference in the commit body.

1. [#585](https://github.com/erikbra/pdfbox-net/issues/585) - Establish this
   branch plan and Preflight exclusion policy.
2. [#586](https://github.com/erikbra/pdfbox-net/issues/586) - Retarget upstream
   comparison tooling to Apache `origin/3.0`.
3. [#587](https://github.com/erikbra/pdfbox-net/issues/587) - Generate 3.0
   source coverage reports excluding Preflight.
4. [#588](https://github.com/erikbra/pdfbox-net/issues/588) - Port or reconcile
   missing non-Preflight 3.0 source files.
5. [#589](https://github.com/erikbra/pdfbox-net/issues/589) - Review changed
   common APIs against Apache `origin/3.0`.
6. [#590](https://github.com/erikbra/pdfbox-net/issues/590) - Retarget runtime
   parity corpus and Java probes to 3.0.
7. [#591](https://github.com/erikbra/pdfbox-net/issues/591) - Align CI and
   package metadata for `release/3.0`.
8. [#592](https://github.com/erikbra/pdfbox-net/issues/592) - Review tools,
   examples, app, and debugger parity for 3.0.
9. [#593](https://github.com/erikbra/pdfbox-net/issues/593) - Define and
   satisfy the release-readiness gate for core 3.0 parity.

## Branch Rules

- Base work branches on `release/3.0`.
- Open PRs against `release/3.0`, not `main`.
- Keep `main` tracking Apache trunk/future 4.0.
- Keep issue work scoped to one GitHub issue at a time.
- Include `Closes #<issue>` in the commit message body for issue work.
- Preserve Java-shaped mechanical conversions where possible.
- Add idiomatic .NET property facades only in sibling partial sidecar files.
- Keep third-party dependencies behind backend packages or narrow internal
  adapters.

## Reporting Expectations

The branch should maintain separate 3.0 reports rather than overwriting the
trunk/4.0 reports used on `main`. Expected report files include:

- `reports/pdfbox-3.0-source-coverage.md`
- `reports/pdfbox-3.0-source-coverage.json`
- `reports/pdfbox-3.0-api-surface-analysis.md`
- `reports/pdfbox-3.0-runtime-parity.md`
- `reports/pdfbox-3.0-accepted-adaptations.md`
- `reports/pdfbox-3.0-release-readiness.md`

Report generators may share implementation with the main branch tooling, but
their inputs and outputs must make the upstream ref and Preflight exclusion
plain.

## Definition of Done

The 3.0 branch is ready to call "core 3.0 parity" when:

- all non-Preflight Apache `origin/3.0` production Java files are mapped,
  ported, or explicitly excluded;
- no unreviewed non-Preflight API surface remains in the 3.0 reports;
- the runtime parity corpus runs against Apache 3.0 artifacts with documented
  accepted differences;
- CI passes for PRs targeting `release/3.0`;
- package/versioning behavior for the 3.0 line is documented;
- README or release documentation states that Preflight/PDF-A validation is not
  included in the initial 3.0 branch.
