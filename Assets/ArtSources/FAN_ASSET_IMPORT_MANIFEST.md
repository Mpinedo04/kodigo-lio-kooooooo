# Fan Asset Import Manifest

Research date: 2026-06-19.

Use this file as the import queue. Before any model becomes a Unity prefab, verify the live Sketchfab page, download terms, author attribution, file format, rig state and whether it is suitable for a non-commercial fan prototype.

## Priority 0 - Scene Validation

| Target | Source | Author | License observed | URL | Notes |
| --- | --- | --- | --- | --- | --- |
| Tower interior | Sketchfab | Turbow5 | Verify before download | https://sketchfab.com/3d-models/code-lyoko-tower-inside-770d0620ca09407d9034d1feb9350c9a | Useful for tower objective prototype. |

## Priority 1 - Playable/Main Characters

| Target | Source | Author | License observed | URL | Notes |
| --- | --- | --- | --- | --- | --- |
| Odd | Sketchfab | ozberkozen | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-odd-3d-model-2bb21d8d6947400cb48c6e0d307420f9 | High poly, needs retopo/LOD. |
| Aelita | Sketchfab | ozberkozen | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-aelita-model-32e296e3be7b4751b8519582f3563b4b | High poly, needs rig/material verification. |
| Ulrich | Sketchfab | ozberkozen | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-ulrich-model-7a203f32ce4d42ffbee48f39e3864ad4 | High poly, katana socket needed. |
| Yumi | Sketchfab | ozberkozen | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-yumi-3d-model-810978d77b8c435e9b7fd4a7f3196075 | High poly, fan/tessen sockets needed. |
| William | Sketchfab | jackzerobear159 | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-quest-of-infinity-william-c43496f6cb6943aea9937ac68ed5e141 | Low-poly game-rip style candidate for prototype. |
| X.A.N.A. William | Sketchfab | ozberkozen | Verify before download | https://sketchfab.com/3d-models/code-lyoko-william-3d-model-50954bac5a6c43919d7780be49b77061 | Page may not expose download; verify manually. |

## Priority 2 - Vertical Slice Monsters

| Target | Source | Author | License observed | URL | Notes |
| --- | --- | --- | --- | --- | --- |
| Kankrelat | Sketchfab | lolo_ivens | CC Attribution observed | https://sketchfab.com/3d-models/kankrelat-aedb0e997f3649fe85babbccfeba1ce7 | Modeled/textured/rigged; strong first import candidate. |
| Blok | Sketchfab | lolo_ivens | CC Attribution observed | https://sketchfab.com/3d-models/blok-0c54c5947e6145f1a3bfccd4ba9358a8 | Existing rig likely; verify file contents. |
| Krab | Sketchfab | jackzerobear159 | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-quest-for-infinity-krab-111f0c636214444aa886323fff50d660 | Low-poly, good prototype candidate. |
| Megatank | Sketchfab | jackzerobear159 | CC Attribution observed | https://sketchfab.com/3d-models/code-lyoko-quest-for-infinity-megatank-0ec9bbeb0e884c3ca112fad4c8397289 | Low-poly, good prototype candidate. |
| Manta | Sketchfab | lolo_ivens | CC Attribution observed | https://sketchfab.com/3d-models/manta-080040b1b7fc4152853f20ed162d13cb | Modeled/textured/rigged. |

## Priority 3 - Expanded Enemies And Vehicles

| Target | Source | Author | License observed | URL | Notes |
| --- | --- | --- | --- | --- | --- |
| Scyphozoa | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Collection listing confirms downloadable entry. |
| Kalamar | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Digital Sea boss candidate. |
| Shark | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Digital Sea enemy. |
| Creeper | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Sector 5 enemy. |
| Tarantula | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Elite shooter. |
| Kongre | Sketchfab collection | lolo_ivens | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Digital Sea enemy. |
| Skidbladnir | Sketchfab collection | jackzerobear159 | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Digital Sea vehicle. |
| Nav-Skid | Sketchfab collection | jackzerobear159 | Verify page | https://sketchfab.com/amethyst_killah/collections/code-lyoko-00058ce8677646e69840c934f4f72483 | Individual pod vehicle. |

## Import Status Legend

- `Candidate`: found online, not downloaded.
- `Downloaded`: raw file saved outside `Assets`.
- `Blender Cleaned`: source normalized, scaled and exported.
- `Unity Imported`: imported to `Assets`.
- `Prefab Ready`: collider/material/rig checked in Unity.
- `Rejected`: license, quality, format or IP risk not acceptable.
