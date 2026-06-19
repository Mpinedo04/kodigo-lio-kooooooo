# Modeling Pipeline

## Target Style

The game should read as faithful Code Lyoko fan art, but the production pipeline must stay game-friendly:

- clear silhouettes before detail,
- toon/cel-friendly materials,
- readable weak points,
- low-to-mid poly runtime models,
- LODs for large enemies,
- separate source files from Unity-ready exports.

## Folder Plan

- `Assets/ArtSources`: source notes, links, license records and Blender files.
- `Assets/Characters`: imported character meshes, rigs, materials and animations.
- `Assets/Enemies`: imported enemy meshes, rigs, materials, animation clips and prefabs.
- `Assets/Vehicles`: overvehicles and Digital Sea vehicles.
- `Assets/Game/Generated`: generated prototype prefabs/materials.
- `Assets/Game/Data`: generated or hand-authored gameplay definitions.

## Character Modeling Passes

1. Blockout mesh: silhouette only, correct height and proportions.
2. Gameplay mesh: clean topology, UVs, simple materials, Unity scale.
3. Rig pass: humanoid rig for humans; custom rigs for monsters.
4. Animation pass: idle, run, jump, attack, hit, devirtualize, vehicle pose.
5. Polish pass: toon normals, emission details, outlines if used.

## Enemy Modeling Notes

- Kankrelat: small low body, four short legs, obvious front eye.
- Blok: cube body, four visible eye faces, six crab-like legs.
- Krab: tall crab silhouette, exposed top/front eye, leg damage support later.
- Hornet: flying insect body, wings as separate mesh for animation.
- Megatank: armored sphere; weak eye should only be exposed during attack timing.
- Scyphozoa: translucent dome plus tentacles; needs shader/VFX support.
- Kolossus: separate body chunks, lava material, giant blade arm.

## Import Rules

- Keep raw downloads outside `Assets` until license and attribution are recorded.
- Prefer `.blend` source plus Unity `.fbx` export.
- Apply transforms in Blender before export.
- Use meters and a consistent human height around 1.6-1.75 Unity units.
- Keep pivots at feet/base for characters and center-of-mass for vehicles.
- Add colliders in Unity prefabs, not in the art mesh where possible.
