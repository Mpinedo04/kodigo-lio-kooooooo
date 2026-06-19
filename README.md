# Code Lyoko 3D Fan Game - Vertical Slice

Proyecto Unity no comercial para prototipar una aventura 3D en tercera persona inspirada en Code Lyoko.

## Abrir y generar la escena

1. Abre esta carpeta con Unity Hub usando Unity 2022.3 LTS o superior.
2. Espera a que Unity importe URP y compile scripts.
3. En el menu superior usa `Tools > Code Lyoko Fan Game > Build Vertical Slice Scene`.
4. Usa tambien `Tools > Code Lyoko Fan Game > Create Prototype Data Assets` para generar fichas editables de personajes, enemigos y vehiculos.
5. Abre `Assets/Levels/ForestSectorVerticalSlice.unity` si no se abre solo.
6. Pulsa Play.

## Controles

- `WASD`: moverse
- `Mouse`: camara
- `Shift`: correr
- `Space`: saltar
- `Mouse 0`: ataque basico
- `Mouse 1`: ataque especial
- `Tab`: lock-on al enemigo cercano
- `V`: montar/desmontar vehiculo
- `1`, `2`, `3`: cambiar entre Odd, Ulrich y Yumi
- `E`: interactuar con la torre cuando Aelita este cerca

## Contenido implementado

- Sector Bosque gris/procedural con plataformas, Mar Digital, ruta y torre activada.
- Odd, Ulrich y Yumi jugables con prototipos visuales y vehiculos personales.
- Aelita como companion con IA sencilla que sigue al jugador y ayuda en la torre.
- Enemigos Kankrelat, Blok, Krab, Hornet y Megatank con IA, laser, vida y punto debil.
- Combate en tercera persona, proyectiles, melee, ataques especiales y devirtualizacion.
- Documento de pipeline/licencias de modelos fan en `Assets/ArtSources/ASSET_SOURCES.md`.
- Catalogo de personajes/enemigos/vehiculos en `Assets/ArtSources/CANON_CATALOG.md`.
- Datos editables generables en `Assets/Game/Data`.
- Documentos de diseno y modelado en `Docs/`.

## Nota legal

Esto esta preparado como fan project local/no comercial. No publiques ni monetices materiales fieles a Code Lyoko ni modelos de fans sin permiso de los titulares y autores correspondientes.
