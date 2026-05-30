# Agent Instructions

## Project Overview
- This is a Unity project named `meteodefence`.
- Use Unity Editor `6000.3.10f1` when opening or validating the project.
- Runtime scripts live in `Assets/Scripts`.
- Editor-only tooling lives in `Assets/Scripts/Editor`.
- Main scenes are in `Assets/Scenes`: `MainMenu`, `MainScene`, and `GameOver`.
- Prefabs currently include `Assets/Prefabs/Meteor.prefab` and `Assets/Prefabs/Bullet.prefab`.
- The project uses URP, Unity UI/uGUI, the Input System, AI Navigation, Timeline, and the Unity Test Framework.

## Files And Folders
- Do not edit generated Unity folders such as `Library`, `Temp`, `Obj`, `Logs`, or package cache contents.
- Do not hand-edit `.meta` files unless the task specifically requires asset GUID or importer changes.
- Preserve existing `.meta` files when moving or renaming Unity assets.
- Keep third-party assets under `Assets/ThirdParty` intact unless the task explicitly asks for asset import or cleanup work.
- Check `Assets/ThirdParty/ASSET_SOURCES.md` and nearby license files before changing or redistributing bundled assets.

## Code Style
- Keep C# changes small and idiomatic for Unity MonoBehaviour code.
- Prefer serialized fields over public mutable fields for Inspector-configurable values.
- Cache component references when they are used repeatedly.
- Avoid expensive scene-wide lookups in `Update`, `FixedUpdate`, or tight gameplay loops.
- Keep gameplay logic in runtime scripts and editor automation in `Assets/Scripts/Editor`.
- Use clear Unity lifecycle methods (`Awake`, `Start`, `Update`, `FixedUpdate`, `OnEnable`, `OnDisable`) consistently with surrounding scripts.

## Unity Workflow
- After modifying scripts, verify that the project compiles in Unity.
- After modifying scenes, prefabs, materials, import settings, or assets, expect Unity to update related serialized files and `.meta` data.
- Prefer prefab or scene edits through Unity tooling when possible, because Unity serialized YAML is easy to corrupt by hand.
- Keep scene references, prefab references, and serialized field names stable unless the change intentionally migrates them.

## Testing
- Use the Unity Test Framework for new tests.
- Put edit-mode tests under `Assets/Tests/EditMode` and play-mode tests under `Assets/Tests/PlayMode` if tests are added.
- For gameplay changes, manually validate the affected scene flow in `MainScene` and related UI scenes.
- If command-line validation is available locally, run Unity in batch mode with the matching editor version before finalizing.

## Git And Generated Content
- This workspace may not always be a Git repository. Check before relying on Git commands.
- Keep generated build outputs out of source changes.
- Do not commit or modify Unity cache folders.

## Communication Notes
- Mention any Unity validation that could not be run locally.
- Call out risky serialized asset edits explicitly.
- Keep final summaries focused on changed files and verification performed.
