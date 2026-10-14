# Copper Story

A 2D platformer prototype built for a Unity course assignment (Unity 6000.6.0f1).

## Current Status

**Early prototype — one playable level.** The project contains:

- **1 level scene**: `Assets/Levels/Lvl1.unity`
- **Player controller** with movement (run, jump, double jump, wall jump) and attack
- **Player animations**: Idle, Run, Jump, Double Jump, Fall, Hit, Wall Jump (32x32 sprites from Pixel Adventure 1 by Pixel Frog, CC0)
- **Input System** configured with keyboard/gamepad bindings
- **Core scripts**: `GameManager`, `PlayerHealth`, `PlayerAttack`, `GameUI`, `DamageZone`, `projectile`
- **Basic sprites**: Terrain blocks (purple), player character frames
- **Project settings** pinned for reproducible builds

## What Is Not Implemented

- Menu scenes (Main Menu, Level Select, Controls)
- Win / GameOver overlays
- Levels 2–4
- Editor tooling (LevelSceneBuilder, MenuSceneBuilder, ArtImportSettings, ProjectValidator, BuildScript)
- Test suite
- Windows/Linux build scripts
- Design spec document (`docs/superpowers/specs/...`)

## Setup

**Requirements**
- Unity 6000.6.0f1 (see `ProjectSettings/ProjectVersion.txt`)

**Clone and open**
```bash
git clone <repo-url>
```
Open with Unity Hub. Let the first import finish, then open `Assets/Levels/Lvl1.unity` and press Play.

## Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move   | A / D    | Left Stick |
| Jump   | Space    | A (Cross) |
| Attack | J        | X (Square) |

Input deadzone: 0.25 on both stick axes.

## Project Structure

```
Assets/
├── Animations/          # Animator controller + animation clips
├── Levels/
│   └── Lvl1.unity       # Single playable level
├── Scripts/
│   ├── player.cs        # Player movement & state
│   ├── PlayerAttack.cs  # Attack hitbox + cooldown
│   ├── PlayerHealth.cs  # 3 hearts, invulnerability, respawn
│   ├── GameManager.cs   # Scene flow, win/lose reporting
│   ├── GameUI.cs        # Health display
│   ├── DamageZone.cs    # Hazard trigger
│   └── projectile.cs    # Player projectile
├── Sprites/
│   ├── Blocks/          # Terrain tiles (16x16, PPU 16)
│   └── CupperGuy/       # Player frames (32x32, PPU 16)
└── InputSystem_Actions.inputactions
```

## Art Attribution

- **Pixel Adventure 1** by Pixel Frog — CC0 1.0 Universal  
  Source: https://pixelfrog-assets.itch.io/pixel-adventure-1  
  Used: player animation frames (CupperGuy/), terrain tiles (Blocks/Purple.png)

All art in this project is CC0. No attribution required by licence, but credit is given above.

## License

Course assignment — no licence declared.