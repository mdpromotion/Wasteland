# 7. Chunk-Based World with World Rebase

Date: 2026-08-12

## Status

Accepted

## Context

The world is infinite and procedurally generated, which requires deciding how the world is divided into loadable units, and how spatial precision is preserved as the player moves far from the origin.

## Decision

We decided on a chunk size of 512 units with a 257x257 resolution per chunk. These values were chosen through testing on multiple devices, balancing the number of objects loaded at any given time against an acceptable player view distance.

We also decided to move away from transform-based position authority. Instead, we introduced a custom **World Rebase** system: once the player crosses a defined number of chunks, the entire world and the player are shifted by a delta, keeping physics-relevant values within a safe and precise `float` range.

## Consequences

- **Pros:** Prevents floating-point precision issues at large distances from the origin; keeps physics and rendering stable regardless of how far the player travels; chunk size/resolution is tuned for a good performance-to-view-distance ratio.
- **Cons:** Adds architectural complexity, since no single source of truth stores absolute world position directly — it must be derived; systems interacting with world-space coordinates need to be rebase-aware.
- **Related:** See `world-rebase.md` for a detailed explanation of the World Rebase feature.