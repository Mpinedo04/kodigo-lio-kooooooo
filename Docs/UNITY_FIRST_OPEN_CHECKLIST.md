# Unity First Open Checklist

Use this on the PC that has Unity installed.

## Recommended Version

- Unity 2022.3 LTS or newer 2022 LTS.
- Universal Render Pipeline package will be imported from `Packages/manifest.json`.

## First Open Steps

1. Open Unity Hub.
2. Add this folder as an existing project.
3. Let Unity finish importing and compiling.
4. Open the Console window.
5. Run `Tools > Code Lyoko Fan Game > Setup URP Pipeline` (without it every material renders pink).
6. Run `Tools > Code Lyoko Fan Game > Create Prototype Data Assets`.
7. Run `Tools > Code Lyoko Fan Game > Create Full Roster Placeholder Prefabs`.
8. Run `Tools > Code Lyoko Fan Game > Build Vertical Slice Scene`.
9. Save the generated scene if Unity asks.
10. Press Play.

## What To Check

- No red Console errors after import.
- `Assets/Settings/URP_Pipeline.asset` exists and nothing renders pink.
- `Assets/Game/Data/RosterDatabase.asset` exists after creating prototype data.
- `Assets/Game/Generated/FullRosterPlaceholders` exists after creating roster placeholders.
- `Assets/Levels/ForestSectorVerticalSlice.unity` exists after building the scene.
- Play Mode starts with Odd active.
- `1`, `2`, `3` swaps characters.
- `V` mounts the character vehicle.
- `Mouse 0` and `Mouse 1` attack.
- Enemies take damage and disappear when defeated.
- Aelita follows and the tower progress changes when holding `E` nearby.
- The HUD shows remaining enemies while a phase needs them cleared.
- When the active warrior reaches 0 HP, control passes to the next one still alive.
- When all three are devirtualized, the HUD says so and `R` restarts the mission.

## If There Are Errors

Copy the first red error from Unity Console, including:

- file path,
- line number,
- full error message,
- whether it happened on import, scene generation or Play Mode.

Fix the first error first. Unity often cascades many later errors from one early compile issue.
