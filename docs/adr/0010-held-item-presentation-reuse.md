# 10. Reuse World Prefab Visuals for Held-Item Presentation

Date: 2026-10-30

## Status

Accepted

## Context

The first-person view had only a placeholder cube in the player's hands. The domain is
presentation-agnostic (items are `ItemDefinition` DTOs), so equipping a real visual must
stay entirely inside the presentation layer. At the same time the project already ships
per-item world prefabs (`ItemDefinitionAsset.WorldPrefab`) with clean mesh-only roots.

## Decision

Held-item view-models are sourced by a presentation-layer factory
(`HeldItemVisualFactory`) with the priority:

1. `ItemDefinitionAsset.HeldPrefab` — optional dedicated first-person model
   (extension point for hand-anchor/bone-rigged models);
2. `ItemDefinitionAsset.WorldPrefab` — reuse of the existing dropped-item visuals;
3. colored cube fallback, shared with the dropped-item spawner.

Visuals are created once per item and cached; equipping only toggles activity
(no destroy/instantiate churn). World-entity behaviour (colliders, rigidbodies,
pickup components) is stripped from the held copy. Pose/equip-motion math is a pure,
unit-testable helper (`HeldItemEquipMotion`); Unity-facing state lives in a passive
`HeldItemView`; `HeldItemPresenter` maps `Inventory.Changed` to view commands.

## Consequences

- **Pros:** No duplicated assets are required to show items in hands; adding dedicated
  first-person models is pure data (fill `heldPrefab`); animations (sway, bob) can be
  layered later without touching inventory reactivity; the idle held item performs no
  per-frame script work.
- **Cons:** World prefab pivots may look imperfect in hands — tune via `heldPrefab`
  per item or via `HotbarHudSettings` held-item pose fields; re-used meshes cast
  shadows in the viewmodel, disable per item model later if unwanted.
