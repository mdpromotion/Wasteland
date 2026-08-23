# 8. Chunk Streaming Pipeline: Prioritization and Cancellation

Date: 2026-08-23

## Status

Accepted

## Context

Generating and loading chunks around an infinite, moving player requires scheduling many background tasks. Without a clear strategy, the pipeline could waste CPU time on chunks that are no longer relevant, or delay chunks the player actually needs right now.

## Decision

We decided on the following scheduling strategy for the chunk streaming pipeline:

- **Prioritization:** Chunks closer to the player are loaded and computed first; farther chunks are processed afterward.
- **Cancellation:** If the player leaves the area of a chunk that is still being loaded, the scheduler cancels the corresponding task, avoiding wasted computation on data the player no longer needs.

## Consequences

- **Pros:** Reduces perceived latency near the player; avoids unnecessary CPU work on chunks that became irrelevant; keeps the streaming system responsive as the player moves quickly.
- **Cons:** Requires additional bookkeeping to track in-flight jobs per chunk and to safely cancel/cleanup partially-generated data.
- **Related:** See 0005 for the Jobs/Burst generation work being scheduled, and 0007 for the chunk system this pipeline streams.