# Emberpath — Roadmap / TODO

Living checklist. Updated as we build. The game: 2D action-platformer roguelite
(Mario-style overworld map + Blasphemous-flavoured combat), iOS target.

## ✅ Done — Step 1 + foundations
- Unity 6 LTS (2D / URP) project + GitHub Actions CI (game-ci compiles every push)
- Movement: run, **single jump** (double jump reserved as a later unlock), dash
- **Dash = dodge/parry** (i-frames during the short dash) + `PerfectDash` hook
- Player melee attack: hitbox, hitstop, knockback, visible swing FX
- Shared **`Health`** (`IDamageable`) for player and enemies (i-frames, knockback, events)
- **Real enemy AI**: ground melee (patrol → chase → wind-up attack, won't walk off
  ledges/into walls) and flying chaser; contact + attack damage; hurt stun; death
- Player health + hit flash + death freeze & respawn
- **Frame animation system** (player & enemy), state-driven; no Animator graph
- Art slots on the bootstrap: player/enemy/ground/background + per-state frame lists
- **Spell foundation**: `SpellCaster` + `SpellProjectile`; perfect dash auto-casts
  (placeholder projectile), plus a manual cast key for testing
- **Ability/unlock system** (`PlayerAbilities`): double jump / dash / spell as
  unlockable abilities (double jump off by default); `Unlock()` for runtime grants
- **Camera follow**: smooth follow with optional level bounds
- **Debug HUD** (IMGUI): player HP, live enemy count, controls
- **Pause/equipment menu** (uGUI, placeholder): Esc to open & pause; framed panel —
  weapon/armor/2 rings (left), spells (right), idle character (centre), resume

## 🎮 Needs your test (when you're home)
- Assign remaining hero frames (Run/Jump/Attack/Hurt) and ground-enemy (hell-gato)
  frames; flyer uses the ghost frames
- Feel pass: jump height/gravity, dash, knockback, hitstop
- Combat balance: enemy speed/damage/HP, detection/attack ranges, parry window
- Confirm: dodging works (no damage mid-dash) and "Perfect dash!" logs; spell fires

## 🔨 Next — foundation I can build without your test
- [ ] Spells: make them **data-driven & equippable** (projectile / AoE burst / buff),
      an equip slot, cooldown/resource; perfect-dash casts the *equipped* one
- [ ] **GameManager / run flow**: start, win/lose, restart, stage transitions; enemy
      registry → "arena cleared" detection
- [ ] More enemy types for challenge: **ranged shooter**, heavy/armored, fast swarmer
- [ ] Enemy awareness polish: line-of-sight, alert/search/give-up states
- [ ] In-game **HUD polish**: real uGUI health hearts, spell icon + cooldown (HUD is IMGUI for now)
- [ ] Pause menu: real icons in slots, equip/select interaction, controller/touch nav
- [ ] **Camera**: follow + room bounds + small shake on hit (juice)
- [ ] Hit feedback/VFX: damage flashes, dust, hit sparks
- [ ] Save/persistence for unlocks

## 🗺️ Later — needs your decisions / art
- [ ] Overworld map (Mario-style): node graph, path navigation, node → loads arena
- [ ] Spell mechanic details: equipped spell list, costs, perfect-dash vs manual cast
- [ ] Roguelite structure: stage selection, rewards, meta-progression
- [ ] iOS: touch controls + build pipeline (Mac/Xcode/signing)
- [ ] Final art integration: tilemaps, parallax backgrounds, polish

## Notes
- Enemies/player are still built at runtime by `TestArenaBootstrap`; once the test
  feels right we can bake a proper authored scene / prefabs.
- AI tuning values currently live as code defaults (runtime-spawned). Can be moved
  to the inspector if you want to tune them yourself.
