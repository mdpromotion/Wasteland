# 6. Native Memory Ownership Strategy

Date: 2026-07-21

## Status

Accepted

## Context

Jobs and Burst-compiled code (see ADR 0005) rely on native containers (e.g. `NativeArray`) instead of managed collections. Native memory is not garbage-collected and must be explicitly disposed, so a clear ownership model is required to avoid leaks and dangling allocations.

## Decision

We decided that persistent native containers are owned and stored at the Domain layer, while their disposal is triggered by the Application layer through a dedicated `Dispose()` method exposed on the domain object, once the data is no longer needed.

## Consequences

- **Pros:** Clear, predictable ownership boundaries; disposal responsibility is explicit rather than implicit, reducing the risk of memory leaks.
- **Cons:** Requires discipline from contributors to always call `Dispose()` at the correct point in the Application layer; missing a call can silently leak native memory.
- **Related:** See 0005 for the Jobs/Burst systems that produce this native data.