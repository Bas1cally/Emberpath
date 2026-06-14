# Emberpath – Prototyp Setup (Schritt 1: Movement & Combat Feel)

Diese drei Skripte sind der erste spielbare Baustein: Bewegung, Angriff, Treffer-Feedback.
Noch komplett ohne Kunst – Ziel ist, ob sich das Grundgefühl gut anfühlt.

## 1. Unity-Projekt vorbereiten

1. Neues Projekt mit Template **"2D (URP)"** erstellen (Unity 6 LTS oder aktuelle 2022 LTS)
2. Diesen `Scripts`-Ordner nach `Assets/Scripts` kopieren (Struktur ist bereits korrekt: `Player/`, `Enemy/`)

## 2. Input einrichten (altes Input-System, für Prototyp ausreichend)

Edit → Project Settings → Input Manager – Standardwerte reichen:
- `Horizontal` (A/D bzw. Pfeiltasten) – bereits vorhanden
- `Jump` (Leertaste) – bereits vorhanden
- `Fire1` (Strg/Maus-Links) – bereits vorhanden
- Dash ist im Skript fix auf **Linke Shift-Taste** gelegt

## 3. Player-GameObject bauen

1. Leeres GameObject erstellen, "Player" nennen
2. Komponenten hinzufügen:
   - `SpriteRenderer` (Platzhalter: weißes Quadrat-Sprite, Scale z.B. 1x1.5)
   - `Rigidbody2D` (Gravity Scale: 3, Constraints: Freeze Rotation Z)
   - `BoxCollider2D` (passend zur Sprite-Größe)
   - `PlayerController` (Skript)
   - `PlayerCombat` (Skript)
3. Im `PlayerController`:
   - `groundCheck`: leeres Child-GameObject "GroundCheck" am unteren Rand des Sprites erstellen und hier referenzieren
   - `groundLayer`: neuen Layer "Ground" anlegen und zuweisen
4. Im `PlayerCombat`:
   - `hitboxOrigin`: leeres Child-GameObject "HitboxOrigin" leicht vor dem Sprite (in Blickrichtung) erstellen und referenzieren
   - `enemyLayer`: neuen Layer "Enemy" anlegen und zuweisen

## 4. Test-Arena bauen

1. 3–4 flache GameObjects mit `BoxCollider2D` als Plattformen, Layer = "Ground"
2. Unterschiedliche Höhen/Lücken für Sprung- und Dash-Tests

## 5. Dummy-Gegner bauen

1. GameObject "DummyEnemy", Layer = "Enemy"
2. Komponenten:
   - `SpriteRenderer` (Platzhalter: rotes Quadrat)
   - `Rigidbody2D` (Gravity Scale: 3, Freeze Rotation Z)
   - `BoxCollider2D`
   - `DummyEnemy` (Skript) – `spriteRenderer` referenzieren
3. Auf einer Plattform platzieren

## 6. Testen

- Laufen (A/D), Springen (Leertaste, 2x für Doppelsprung), Dash (Shift)
- Mit Strg/Maus-Links angreifen, wenn der Gegner in Reichweite der gelben Hitbox-Gizmo-Kugel ist (im Scene-View sichtbar)
- Bei Treffer: kurzer Freeze (Hitstop), Gegner wird zurückgestoßen und blitzt weiß auf

## 7. Was als Nächstes anzupassen ist (Tuning-Pass)

Alle Werte sind als `[Tooltip]`-Felder im Inspector einstellbar – hier zuerst experimentieren:
- `moveSpeed`, `jumpForce`, `fallGravityMultiplier` → Sprung-/Lauf-Gefühl
- `dashSpeed`, `dashDuration`, `dashCooldown` → Dash-Timing
- `hitstopDuration`, `hitstopTimeScale`, `knockbackForce` → "Wucht" der Treffer

## 8. Nächster Schritt nach diesem Prototyp

Sobald sich Bewegung + Combat gut anfühlen:
- Scenario.gg Custom-Style-Modell für Emberpath trainieren (siehe vorheriges Gespräch)
- Platzhalter-Sprites durch erste KI-generierte Pixel-Art ersetzen (Spielfigur, Gegner, Tileset für Aschefelder-Region)
- Erste Region "Aschefelder" als Run-Modul-Sammlung bauen (siehe GDD Abschnitt 6)
