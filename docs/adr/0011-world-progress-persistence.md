# 11. World & Progress Persistence

Date: 2026-08-23

## Status

Proposed

## Context

The project currently has no world or progress save system. As the procedural generator evolves over time, regenerating the world from a seed alone risks producing a different landscape than what the player previously explored (e.g. a former plain with a player-built house turning into a mountain or forest after a generator update).

## Decision

We are planning to persist world data as binary records per chunk region (e.g. 8x8 chunks), storing:

- Heightmap
- River mask
- Vegetation mask

This allows a future, updated generator to reference the previously saved maps and attempt to reconstruct the original landscape shape, rather than regenerating it from scratch. In addition, we plan to persist gameplay deltas (e.g. a tree being chopped down, planks being placed) on top of the reconstructed base landscape.

## Consequences

- **Pros:** Protects player progress and builds from being invalidated by future generator changes; keeps previously explored areas visually consistent across generator updates.
- **Cons:** Adds storage overhead per explored region; requires careful versioning/migration logic if the save format itself changes; reconstruction logic adds complexity to the generator.
- **Related:** This decision is pending and may be revised once implementation begins.