# 3. Use MVP Pattern for the Presentation Layer

Date: 2026-08-10

## Status

Accepted

## Context

With Clean Architecture adopted in ADR 0002, we need a specific pattern for organizing the UI layer that keeps UI components free from business logic while enabling rapid prototyping without excessive boilerplate.

### Why not MVVM or other approaches?

The UI was not expected to reach a level of complexity that would require more elaborate patterns. For simplicity of implementation, MVP was chosen as the foundation for the UI-presentation layer of the project.

## Decision

We decided to use the MVP (Model-View-Presenter) pattern, where the **View** is a passive executor, the **Presenter** contains UI logic and communicates with the Application layer, and the **Model** stores some state.

## Consequences

- **Pros:** The UI layer becomes straightforward for developers to work with.
- **Cons:** In complex interfaces, special care must be taken to prevent the Presenter from becoming a god object, which adds complexity.
- **Related:** See 0002 to understand how the rest of the project works with this architecture.