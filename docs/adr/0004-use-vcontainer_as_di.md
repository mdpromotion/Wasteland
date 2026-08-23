# 4. Use VContainer as DI

Date: 2026-07-14

## Status

Accepted

## Context

With Clean Architecture adopted in ADR 0002, the project requires a Dependency Injection framework to wire dependencies between layers (Domain, Infrastructure, Presentation, etc.) without introducing tight coupling.

### Why not Zenject?

Zenject was considered as an option, being one of the most established DI frameworks for Unity. However, its extensive feature set introduces overhead that isn't necessary for this project. Additionally, there was interest in adopting a more modern and lightweight tool.

## Decision

We decided to use VContainer as the Dependency Injection framework for the project, due to its lightweight design and modern approach compared to alternatives like Zenject.

## Consequences

- **Pros:** Lightweight and fast DI resolution with minimal overhead; a modern API that fits well with the project's architecture.
- **Cons:** Smaller ecosystem and community compared to Zenject, which may mean fewer ready-made examples and integrations.