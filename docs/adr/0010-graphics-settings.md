# 10. Default Graphics Settings

Date: 2026-08-10

## Status

Accepted

## Context

The in-game graphics options menu allows players to tune visual quality for their hardware. A sensible default is needed for a player's very first launch, before any manual configuration.

## Decision

We decided to apply a fixed default graphics preset (Medium) on first launch. In the future, we plan to replace this fixed default with an automatic hardware benchmark that selects an appropriate preset per device.

## Consequences

- **Pros:** Simple to implement now; gives every player a reasonable out-of-the-box experience without requiring immediate manual configuration.
- **Cons:** A fixed "Medium" default is not tailored to the player's actual hardware, and may be too demanding or too conservative until the benchmark-based approach is implemented.