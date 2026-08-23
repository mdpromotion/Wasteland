# 5. Use Unity Jobs + Burst for World Generation

Date: 2026-07-14

## Status

Accepted

## Context

Procedural world generation requires a substantial amount of computation per frame (terrain, hydrology, vegetation placement). Running this workload on the main thread using standard managed C# would not scale and would cause noticeable frame drops during chunk generation.

## Decision

We decided to use Unity's built-in Jobs System combined with the Burst Compiler for performance-critical generation code, in order to significantly increase landscape generation throughput via multithreading and native-code compilation.

## Consequences

- **Pros:** Substantially faster terrain and world-data generation; better CPU utilization across cores; scales well as generation complexity grows.
- **Cons:** Burst-compatible code imposes restrictions (no managed types, careful memory handling), which increases implementation complexity for generation-related systems.
- **Related:** See 0006 for how native memory produced by these jobs is managed and disposed.