# World Rebase Feature

Date: 2026-08-12

## Overview

World Rebase is the system responsible for keeping world-space coordinates numerically stable in an infinite, chunk-based world, without relying on Unity's `Transform` as the source of positional truth (see ADR 0007).

## How it works

- **No stored world position.** The player's absolute world coordinate is never stored directly. Instead, it is derived on demand from the player's current `ChunkCoordinate` combined with the current local `Transform` position — effectively an "offset within the current rebase zone."
- **Rebase trigger.** Once the player crosses a defined number of chunks from the last rebase point, the entire world (including the player) is shifted by a delta so that local coordinates stay close to the origin. This keeps all `float`-based physics and rendering calculations within safe precision ranges.
- **Double precision for generation.** The X and Z coordinates used for noise-based generation are stored as `double`, rather than `float`, to avoid precision loss and generation artifacts as the distance from spawn increases.

## Rationale

Standard Unity `Transform`-based positioning loses precision as values grow large, causing jittering, physics instability, and generation artifacts far from the world origin. World Rebase decouples "where the player visually is" (local, `float`-precision, always near zero) from "where the player actually is in the generated world" (chunk coordinate + double-precision offset), solving both problems without imposing a hard world-size limit.

## Related

- See ADR 0007 for the decision context behind chunk size and World Rebase.