# 9. Use URP as the Render Pipeline

Date: 2026-07-10

## Status

Accepted

## Context

The project needs a Unity render pipeline capable of supporting a procedurally generated, performance-sensitive world across multiple target platforms.

## Decision

We decided to use the Universal Render Pipeline (URP), as it is the most modern and actively supported pipeline, offers strong cross-platform compatibility, and provides a good balance of visual quality and high frame rates across a wide range of hardware.

## Consequences

- **Pros:** Broad platform support; strong performance characteristics on lower-end hardware; actively maintained and aligned with Unity's long-term direction.
- **Cons:** Some visual features available in HDRP (e.g. advanced lighting/rendering effects) are not available or require custom implementation in URP.