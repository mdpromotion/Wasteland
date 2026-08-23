# 2. Use Clean Architecture for Core Features

Date: 2026-07-15

## Status

Accepted

## Context

As the project and codebase grow, future changes could become increasingly time-consuming and difficult due to the high coupling between components typical of common Unity project architecture patterns.

### Why not ECS/DOTS?

The goal of this project was to prototype an OOP-based approach in order to understand how far it could realistically be taken and what fundamental limitations would be encountered along the way. That said, the project isn't purely OOP: the procedural-world-feature leans heavily on a data-driven approach, where GameObjects are not the source of truth but merely a way of presenting world data.

## Decision

We decided to establish a solid Clean Architecture foundation (Use Cases, Domain, Infrastructure, Presentation, etc.) for the core features from the very beginning, in order to facilitate refactoring and the introduction of new features.

## Consequences

- **Pros:** The architecture is more change-friendly, resulting in greater efficiency and faster implementation of new systems and features.
- **Cons:** Increased boilerplate for simple features. Increased complexity for new contributors who are unfamiliar with the pattern.
- **Related:** See 0003 to understand how the Presentation layer works with this architecture.