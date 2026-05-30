# Garden Defender

Garden Defender is a small Unity survival game where the player protects a garden until dawn. Bugs spawn around the arena, attack vegetables, and become more dangerous over time. The player moves through the garden in first person and uses a fly swatter weapon to keep the plants alive.

## Project

- Unity version: `6000.3.10f1`
- Main scenes: `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/MainScene.unity`, `Assets/Scenes/GameOver.unity`
- Runtime scripts: `Assets/Scripts`
- Prefabs: `Assets/Prefabs/Meteor.prefab`, `Assets/Prefabs/Bullet.prefab`
- Render pipeline: Universal Render Pipeline

The normal gameplay flow starts from `MainMenu` and loads `MainScene`. `MainScene` is configured for the regular single-camera game view.

## Gameplay

- Survive until dawn while keeping at least one vegetable alive.
- Bugs spawn at the arena edge and move toward vegetables or the player.
- Different bug types have different health, speed, and damage.
- The HUD shows time until dawn, kills, alive bugs, vegetable count, and player health.
- The game supports offline play and basic host/client launch buttons from the main menu.

## Controls

- Move: `WASD`
- Look: mouse
- Jump: `Space`
- Attack: left mouse button
- Pause: `Esc`

Gamepad movement and look input are also supported through Unity Input System.

## How to Run

1. Open the project in Unity `6000.3.10f1`.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press Play.
4. Choose `Play` for an offline session, or use `Host` / `Client` for the network launch flow.

## Repository Notes

Generated Unity folders such as `Library`, `Temp`, `Obj`, `Logs`, and local editor files are ignored. `.docx` files and local agent instruction files are intentionally excluded from the repository.
