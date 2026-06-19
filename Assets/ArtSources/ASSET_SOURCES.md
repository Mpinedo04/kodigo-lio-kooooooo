# Asset Sources And Modeling Notes

This project starts with procedural placeholder meshes generated in Unity. Fan models should only be used for private, non-commercial prototyping unless every license and IP permission is cleared.

## Canon Research

- Main characters: https://codelyoko.fandom.com/wiki/List_of_Characters
- Monsters: https://codelyoko.fandom.com/wiki/Monsters
- Vehicles: https://codelyoko.fandom.com/wiki/Category:Vehicles
- Model collection found during planning: https://sketchfab.com/floksy59/collections/code-lyoko-models-0d5f09590c9b4221a899584270e798ff

## Known Fan Model Leads

Record every downloaded asset here before import:

| Asset | Author | URL | License | Imported? | Notes |
| --- | --- | --- | --- | --- | --- |
| Odd 3D Model | ozberkozen | https://sketchfab.com/3d-models/code-lyoko-odd-3d-model-2bb21d8d6947400cb48c6e0d307420f9 | CC Attribution listed on Sketchfab at research time | No | High poly; retopo/LOD likely needed. |
| Aelita Model | ozberkozen | https://sketchfab.com/3d-models/code-lyoko-aelita-model-32e296e3be7b4751b8519582f3563b4b | CC Attribution listed on Sketchfab at research time | No | High poly; verify rig/materials. |
| Ulrich Model | ozberkozen | https://sketchfab.com/3d-models/code-lyoko-ulrich-model-7a203f32ce4d42ffbee48f39e3864ad4 | CC Attribution listed on Sketchfab at research time | No | High poly; katana setup needed. |
| Yumi 3D Model | ozberkozen | https://sketchfab.com/3d-models/code-lyoko-yumi-3d-model-810978d77b8c435e9b7fd4a7f3196075 | CC Attribution listed on Sketchfab at research time | No | High poly; fan/tessen sockets needed. |
| Blok | lolo_ivens | https://sketchfab.com/3d-models/blok-0c54c5947e6145f1a3bfccd4ba9358a8 | CC Attribution listed on Sketchfab at research time | No | Better monster prototype candidate. |

For the broader import queue, use `FAN_ASSET_IMPORT_MANIFEST.md`.

## Blender Import Checklist

1. Import `.fbx` or `.glb`.
2. Apply transforms and normalize scale to Unity meters.
3. Reduce polygons or create LOD0/LOD1/LOD2.
4. Rename armature bones clearly.
5. Bake or simplify materials into toon-friendly albedo/emission maps.
6. Export FBX with forward `-Z Forward`, up `Y Up`.
7. In Unity, verify scale, pivots, colliders, rig, animations and attribution.
