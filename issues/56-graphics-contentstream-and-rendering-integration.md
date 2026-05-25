### Title
Integrate completed graphics model with contentstream execution and rendering pipeline

### Background
After PDModel graphics types and integration points are complete, execution and rendering layers
must consume the finalized model to remove remaining no-op graphics paths.

### Depends on
- #55 Graphics-state and XObject integration
- Existing rendering backlog context from #33

### Scope
- Align content-stream graphics operator processors with completed PDModel graphics behavior.
- Remove/replace milestone-specific rendering no-op branches caused by incomplete graphics types.
- Keep backend choice in #33, but ensure current rendering pipeline uses completed graphics model
  consistently regardless of backend.

### Expected test scope
- Fixture-driven content-stream execution tests covering shading/pattern/inline-image operators.
- Rendering smoke tests verifying non-error execution for graphics-heavy fixtures.

### Entry criteria
- #55 complete and green.

### Exit criteria
- Graphics operators execute against real PDModel graphics types without stub fallbacks.
- Rendering/contentstream tests for graphics-heavy fixtures are stable.

### Risk register
- Rendering behavior differs by backend; avoid coupling milestone logic to one backend prematurely.
- Operator integration can expose latent parser/resource-resolution assumptions.

### Definition of done
- Build + targeted contentstream/rendering tests pass.
- Traceability and normalization artifacts updated for integration changes.
