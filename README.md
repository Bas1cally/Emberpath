# Emberpath

A 2D action platformer roguelite for iOS — a Mario-style overworld map feeding
into hand-crafted, *Blasphemous*-flavoured combat stages. Built in **Unity 6 LTS
(2D / URP)**.

## Status

**Step 1 — Movement & Combat Vertical Slice** (placeholder art only):
run, double jump, dash, and a melee attack with hit feedback (knockback,
hitstop, sprite flash) against a dummy enemy in a small test arena.

## Getting started

Open the project with **Unity 6 LTS (`6000.0.x`)** via Unity Hub, open
`Assets/Scenes/TestArena.unity` and press **Play**.

Full setup notes, controls and architecture: see
[`Assets/Scripts/README_Setup.md`](Assets/Scripts/README_Setup.md).

## Controls (desktop test build)

`A`/`D` or arrows to move · `Space`/`W` to jump (double jump in air) ·
`Shift`/`K` to dash · `J`/Left Mouse to attack.

## Continuous integration

Every push is compiled and EditMode-tested via GitHub Actions (game-ci). To
enable it, do the one-time Unity Personal license setup in
[`.github/CI_SETUP.md`](.github/CI_SETUP.md).

## Project layout

```
Assets/
  Scenes/        TestArena.unity — the vertical-slice test scene
  Scripts/
    Core/        shared interfaces, hitstop, runtime placeholder art, scene bootstrap
    Player/      PlayerController, PlayerCombat
    Enemy/       DummyEnemy
    Editor/      test-scene generator menu
  Settings/      reserved for URP / render assets
```
