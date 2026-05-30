# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Bug Survivor — first-person melee survival game built in **Unity 6000.3.10f1** (Unity 6). The player is shrunk to insect scale and must survive waves of bugs (ants, beetles, spiders) crawling toward them. The player fights with a stick (melee weapon). The environment is a "macro world" — giant grass blades, pebbles as boulders, twigs, mushrooms.

## Key Packages

- **Universal Render Pipeline (URP)** 17.3.0 — rendering via `Assets/Settings/PC_RPAsset` and `Mobile_RPAsset`
- **Input System** 1.18.0 — all input is read directly from `Keyboard.current`, `Mouse.current`, `Gamepad.current` (no InputAction asset wiring)
- **AI Navigation** 2.0.10

## Architecture

Scripts in `Assets/Scripts/` communicate through direct references, `FindFirstObjectByType`, and static events. No event bus or DI framework.

### Gameplay

| Script | Role |
|---|---|
| `MeteorSpawner` | Singleton. Spawns bug GameObjects from arena edges on a timer. Difficulty ramps over time. |
| `MeteorMover` | Bug AI. Crawls toward player, deals contact damage on collision. 3 types: Ant, Beetle, Spider. |
| `PlayerController` | FPS controller with melee combat. Creates `StickWeapon` on camera. Fires `OnHealthChanged` event. |
| `StickWeapon` | Melee weapon. Swing animation, `OverlapSphere` hit detection, attached to player camera. |
| `BugModelBuilder` | Procedural 3D bug models from primitives. Builds Ant, Beetle, or Spider on a GameObject. |
| `EnvironmentBuilder` | Procedural micro-world: giant grass blades, pebbles, twigs, mushrooms, fallen leaves. |

### UI & Scenes

| Script | Role |
|---|---|
| `GameData` | Static class. Persists kills, settings (volume, sensitivity via PlayerPrefs) across scenes. |
| `UIHelper` | Static utility for creating Canvas, panels, buttons, sliders, text in code. |
| `GameHUD` | In-game HUD: health bar, kill counter, alive meteor counter, crosshair. Subscribes to `PlayerController.OnHealthChanged`. |
| `PauseMenuUI` | Esc toggles pause overlay. Resume / Restart / Settings / Main Menu. Sets `TimeScale=0`. |
| `SettingsUI` | Reusable settings panel (volume + mouse sensitivity sliders). Used by pause menu and main menu. |
| `MainMenuUI` | Main menu scene: Play / Settings / Quit. |
| `GameOverUI` | Game over scene: shows kills, Play Again / Main Menu. |

**Scene flow:** `MainMenu` → (Play) → `MainScene` → (death) → `GameOver` → (Play Again or Main Menu).

All UI is built programmatically in code (no prefabs needed for UI).

## Conventions

- All gameplay objects set themselves to layer 2 (Ignore Raycast) so they don't interfere with ground raycasts.
- Physics components (`Rigidbody`, colliders) are ensured in code via `GetComponent` + `AddComponent` fallback, not just via the Inspector.
- UI is created entirely in C# (no Canvas prefabs). `UIHelper` provides factory methods.
- Cross-scene data lives in the static `GameData` class; settings are backed by `PlayerPrefs`.

## Scenes

Three scenes in `Assets/Scenes/`, registered in Build Settings via `Tools > MeteoDefence > Setup All Scenes`:

| Index | Scene | Key GameObjects |
|---|---|---|
| 0 | `MainMenu` | MainMenu (MainMenuUI) |
| 1 | `MainScene` | Player, Ground, MeteorSpawner, GameHUD, PauseMenu |
| 2 | `GameOver` | GameOver (GameOverUI) |

## Prefabs

- `Assets/Prefabs/Meteor.prefab` — meteor/bug enemy
- `Assets/Prefabs/Bullet.prefab` — player projectile

## Editor Tools

- **Tools > MeteoDefence > Setup All Scenes** — creates MainMenu and GameOver scenes, adds GameHUD/PauseMenu to MainScene, and configures Build Settings.
