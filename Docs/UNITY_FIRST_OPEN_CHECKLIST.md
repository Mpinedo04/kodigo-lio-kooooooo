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
5. Generated content is already in git. Only if a generator changed, run `Tools > Code Lyoko Fan Game > Rebuild Everything`.
6. Open `Assets/Levels/ForestSectorVerticalSlice.unity` and press Play (`Boot` loads automatically).

## What To Check

- No red Console errors after import.
- `Assets/Settings/URP_Pipeline.asset` exists and nothing renders pink.
- `Assets/Game/Data/RosterDatabase.asset` exists after creating prototype data.
- `Assets/Game/Generated/FullRosterPlaceholders` exists after creating roster placeholders.
- `Assets/Levels/ForestSectorVerticalSlice.unity` and `Assets/Scenes/Boot.unity` exist, with `Boot` first in Build Settings.
- Play Mode starts with Odd active.
- `1`, `2`, `3` (or D-pad left/right on a gamepad) swaps characters.
- `V` mounts the character vehicle.
- `Mouse 0` and `Mouse 1` attack.
- Enemies take damage and disappear when defeated.
- Aelita follows and the tower progress changes when holding `E` nearby.
- The HUD shows remaining enemies while a phase needs them cleared.
- When the active warrior reaches 0 HP, control passes to the next one still alive.
- When all three are devirtualized, the HUD says so and `R` (gamepad Y) restarts the mission.
- `F1` opens the debug menu (god mode, kill enemies, jump to a mission step, spawn monsters).

## If There Are Errors

Copy the first red error from Unity Console, including:

- file path,
- line number,
- full error message,
- whether it happened on import, scene generation or Play Mode.

Fix the first error first. Unity often cascades many later errors from one early compile issue.
