# Code Lyoko 3D Fan Game - Vertical Slice

Proyecto Unity no comercial para prototipar una aventura 3D en tercera persona inspirada en Code Lyoko.

## Abrir y generar la escena

1. Abre esta carpeta con Unity Hub usando exactamente Unity 2022.3.56f1 (no una version mas nueva).
2. Espera a que Unity importe los paquetes y compile.
3. Todo lo generado ya esta en git. Si cambias un generador, usa `Tools > Code Lyoko Fan Game > Rebuild Everything` (URP, controles, eventos, datos, prefabs, escena `Boot` y escena del Bosque).
4. Abre `Assets/Levels/ForestSectorVerticalSlice.unity` (o `Assets/Scenes/Boot.unity`) y pulsa Play. Si empiezas desde el Bosque, `Boot` se carga solo.

## Controles

Teclado y raton / mando (Xbox o PlayStation). Tabla completa en `Docs/BIBLIA_DEL_JUEGO.md`, seccion 16.

- Moverse: `WASD` / stick izquierdo
- Camara: raton / stick derecho
- Correr: `Shift` / L3
- Saltar: `Space` / A
- Esquivar: `Ctrl` / B
- Guardia / desvio: `C` / LT justo antes de que te den
- Odd: salta contra una pared y mantén adelante para trepar; doble salto en el aire. Yumi: corre y salta para un salto largo.
- Ataque basico: clic izquierdo / X (Ulrich encadena combos; mantener y soltar = ataque cargado)
- Ataque especial: clic derecho / Y
- Fijar enemigo: `Tab` / R3
- Montar/desmontar vehiculo: `V` / cruceta arriba (montado: `Space`/`Ctrl` suben y bajan, `Shift` turbo)
- Cambiar personaje: `1` `2` `3` / cruceta izquierda y derecha
- Torre (con Aelita cerca): mantener `E` / RT
- Reintentar cuando todos estan devirtualizados: `R` / Y
- Menu de depuracion (solo editor): `F1`

## Contenido implementado

- Sector Bosque gris/procedural con plataformas, Mar Digital, ruta y torre activada.
- Odd, Ulrich y Yumi jugables con prototipos visuales y vehiculos personales.
- Aelita como companion con IA sencilla que sigue al jugador y ayuda en la torre.
- Enemigos Kankrelat, Blok, Krab, Hornet y Megatank con IA, laser, vida y punto debil.
- Combate en tercera persona, proyectiles, melee, ataques especiales y devirtualizacion.
- Input System con teclado y mando, escena `Boot` con carga de niveles, misiones definidas como datos, eventos, pool de proyectiles, menu de depuracion y tests (F02).
- Movimiento con aceleracion, salto variable, esquiva, parada en bordes, plataformas moviles y rasgos por personaje; camara Cinemachine con colision, fijado, vehiculo y sacudida; escena de pruebas `Assets/Scenes/Test/MovementGym.unity` (F03).
- Combate: dano con equipos y capas de fisicas, combos, ataques cargados, desvio, energia especial, hit-stop, empuje, invulnerabilidad tras golpe, ojo de X.A.N.A. con muerte instantanea, numeros de dano, Mar Digital mortal y escena de pruebas `Assets/Scenes/Test/CombatArena.unity` (F04).
- Documento de pipeline/licencias de modelos fan en `Assets/ArtSources/ASSET_SOURCES.md`.
- Catalogo de personajes/enemigos/vehiculos en `Assets/ArtSources/CANON_CATALOG.md`.
- Datos editables generables en `Assets/Game/Data`.
- Prefabs placeholder generables para el roster principal completo.
- Documentos de diseno y modelado en `Docs/`.
- Biblia completa del juego (historia, personajes, habilidades, enemigos, mapas y plan) en `Docs/BIBLIA_DEL_JUEGO.md`.
- Fases de desarrollo (un archivo por fase, con tareas marcables) en `Docs/Fases/`; empieza por `Docs/Fases/00_INDICE.md`.

## Siguiente paso sin perderse

Cuando abras el proyecto en el PC con Unity, sigue `Docs/UNITY_FIRST_OPEN_CHECKLIST.md`. Si aparece un error rojo en consola, copia el primero entero; normalmente arreglar el primero limpia varios de golpe.

## Nota legal

Esto esta preparado como fan project local/no comercial. No publiques ni monetices materiales fieles a Code Lyoko ni modelos de fans sin permiso de los titulares y autores correspondientes.
