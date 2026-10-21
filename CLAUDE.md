# CLAUDE.md

Fan game 3D no comercial de **Code Lyoko**: acción en tercera persona hecha con **Unity 2022.3.56f1** y **URP 14**. Ahora mismo es un prototipo con una sola misión (Sector Bosque) en la que todo usa formas provisionales.

## Idioma y trato

- El usuario escribe en español. **Responde en español**, con explicaciones sencillas y sin jerga innecesaria. Cuando haya que hacer algo en el Editor de Unity, dale los pasos uno a uno (menú > opción).
- **El código va en inglés** (clases, métodos, variables, comentarios); **la documentación de `Docs/` va en español**.

## Antes de trabajar: lee solo lo necesario

Los documentos están divididos para no gastar contexto. Orden de lectura:

1. [`Docs/Fases/00_INDICE.md`](Docs/Fases/00_INDICE.md): estado de todas las fases y cuál es la **fase activa** (~50 líneas).
2. **Solo** el archivo de la fase en la que vayas a trabajar (`Docs/Fases/FNN_*.md`, entre 20 y 75 líneas).
3. [`Docs/Fases/00_REGLAS.md`](Docs/Fases/00_REGLAS.md) si vas a cerrar una tarea, hacer commit o crear ramas.
4. [`Docs/BIBLIA_DEL_JUEGO.md`](Docs/BIBLIA_DEL_JUEGO.md) (~700 líneas): **no la leas entera**. Busca la sección con `Grep "^## "` y lee solo ese rango:

| Necesitas… | Sección de la Biblia |
| --- | --- |
| Historia por temporada / capítulo | `## 4.` |
| Habilidades y estadísticas de un guerrero | `## 5.` (subsecciones `### 5.1` Odd, `5.2` Ulrich, `5.3` Yumi, `5.4` Aelita, `5.5` William, `5.6` Jeremie) |
| Secundarios (Sissi, Jim, Franz…) | `## 6.` |
| X.A.N.A., colores de las torres | `## 7.` |
| Monstruos: PV, daño, punto débil | `## 8.` |
| Jefes | `## 9.` |
| Ataques en el mundo real | `## 10.` |
| Sectores y lugares | `## 11.` |
| Vehículos | `## 12.` |
| Reglas de juego (PV, devirtualización, Retorno al pasado, sinergias) | `## 13.` |
| Controles | `## 16.` |
| Estilo visual y sonido | `## 17.` |

**No leas** (no aportan nada y gastan contexto): `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, archivos `*.meta`, `ProjectSettings/*.asset` (salvo que la tarea sea de configuración) ni `Assets/Game/Generated/` (son assets regenerables).

## Al terminar una tarea

1. Marca la tarea `[x]` en su archivo de fase.
2. Si la fase cambia de estado, actualiza la tabla y la línea **Fase activa** de `00_INDICE.md`.
3. Commit con el código de la tarea delante: `F4.2: add dodge with i-frames`.
4. Si una tarea resultó demasiado grande, divídela en `F4.3a`, `F4.3b`… dentro de su archivo antes de seguir.

## Código actual (para no tener que abrir cada archivo)

Todo está en el namespace `CodeLyokoFanGame` (`.Data` para las definiciones, `.EditorTools` para el editor). Hay tres ensamblados: `CodeLyokoFanGame.Runtime` (`Assets/Game/Scripts/`), `CodeLyokoFanGame.Editor` (`Assets/Game/Editor/`) y `CodeLyokoFanGame.Tests.EditMode` (`Assets/Tests/EditMode/`).

| Archivo (`Assets/Game/Scripts/`) | Qué hace |
| --- | --- |
| `Input/InputReader.cs` | **Única puerta a los controles.** Propiedades como `Move`, `JumpPressed`, `InteractHeld`; mapas en `Assets/Game/Input/LyokoControls.inputactions` |
| `Core/GameManager.cs` | Estado global (capítulo, misión, Nivel de X.A.N.A., desbloqueos) y `GameManager.RaiseEvent(GameEventId)` |
| `Core/SceneLoader.cs`, `Core/BootLoader.cs` | Carga aditiva de niveles sobre `Boot`; `BootLoader` añade `Boot` si das a Play desde un nivel |
| `Core/PrefabPool.cs`, `Core/PooledObject.cs` | Reciclado de proyectiles y efectos (`PrefabPool.Spawn` / `Despawn`) |
| `Events/GameEvent*.cs` | Canales de eventos (ScriptableObject) y `GameEventListener` para responder desde el Inspector |
| `Mission/MissionRunner.cs`, `Mission/MissionMarker.cs` | Ejecuta una `MissionDefinition` paso a paso; marcadores con id para pasos “llega a X” |
| `DebugTools/DebugMenu.cs` | Menú `F1`: invencible, matar enemigos, saltar paso, crear monstruos, ver PV |
| `PlayerController.cs` | Movimiento del guerrero activo: aceleración, salto variable con coyote/buffer, esquiva con invulnerabilidad, parada en bordes, plataformas móviles, rasgos (doble salto, trepar, salto largo) y vehículo. Valores en `Data/MovementProfile` |
| `Player/JumpAssist.cs`, `Player/MovementMath.cs` | Lógica pura del salto y la aceleración (con tests) |
| `Camera/CameraRig.cs` | Cámaras Cinemachine: FreeLook a pie y en vehículo, cámara de fijado, FOV al correr y sacudida (`CameraRig.Shake`) |
| `Core/GameSettings.cs` | Opciones guardadas (sensibilidad, invertir Y) |
| `Level/MovingPlatform.cs` | Plataforma que va y vuelve y arrastra a quien está encima |
| `PlayerCombat.cs` | Ataque básico (combos cuerpo a cuerpo o disparo), cargado (mantener) y especial; ayuda de puntería. Valores en `Data/CombatProfile` y `Data/ComboDefinition` |
| `Combat/DamageInfo.cs`, `Combat/HitResolver.cs`, `Combat/Hitbox.cs`, `Combat/Hurtbox.cs` | **Todo daño pasa por aquí**: `DamageInfo` describe el golpe; `HitResolver` lo aplica una vez por objetivo, respeta equipos (`Team`) y prefiere el ojo expuesto |
| `Combat/PlayerGuard.cs`, `Combat/ParryWindow.cs`, `Combat/ComboTracker.cs`, `Combat/SpecialMeter.cs`, `Combat/HitStop.cs` | Desvío, combos, energía especial y congelado de golpe |
| `Combat/GameLayers.cs`, `Combat/TargetingMath.cs` | Nombres de capas de físicas y matemáticas de fijado/puntería (con tests) |
| `Combat/DamageFlicker.cs`, `Combat/DamageNumbers.cs`, `Combat/DevirtualizeEffect.cs` | Parpadeo tras golpe, números de daño y efecto provisional de devirtualización |
| `CharacterSwitcher.cs` | Cambio de guerrero (1–4, cruceta) y paso al siguiente al devirtualizarse |
| `AelitaCompanion.cs` | IA de Aelita: seguir al jugador e ir a la torre |
| `EnemyAI.cs`, `EnemyFactory.cs` | IA de todos los monstruos en un solo script (F06 lo divide en estados) y creación de monstruos provisionales |
| `Health.cs`, `WeakPoint.cs`, `Projectile.cs` | Vida (equipo, invulnerabilidad, `Kill`, `AnyDamaged`), ojo de X.A.N.A. (×2,5 o muerte instantánea) y proyectiles (gravedad, perforación, chocan con `Environment`) |
| `LockOnSystem.cs` | Fijar objetivo (la cámara lo lee desde `CameraRig`) |
| `VehicleRig.cs` | Vehículos (Overboard/Overbike/Overwing) |
| `TowerObjective.cs`, `WaveSpawner.cs`, `Checkpoint.cs` | Torre, oleadas y puntos de control |
| `DigitalSeaHazard.cs` | Caer al Mar Digital = devirtualización |
| `GameHud.cs` | HUD provisional (IMGUI) |
| `GameEnums.cs` | Todos los enums (personajes, enemigos, fases y condiciones de misión, habilidades, sectores, eventos) |
| `Data/*Definition.cs`, `Data/RosterDatabase.cs` | ScriptableObjects: personajes, enemigos, vehículos, habilidades, sectores, misiones y capítulos |

**Generadores** (`Assets/Game/Editor/`, menú **Tools > Code Lyoko Fan Game**): usa **`Rebuild Everything`** (`ProjectRebuild.RebuildEverything`), que ejecuta en orden URP, assets centrales (controles y eventos), datos, prefabs, escena `Boot`, escenas de pruebas `Assets/Scenes/Test/MovementGym.unity` y `CombatArena.unity`, y escena del Bosque. Antes ejecuta `PhysicsLayersSetup` (capas y colisiones). El orden de *Build Settings* lo decide `ProjectScenes` (`Boot` siempre primero). Todo lo generado está en git: si cambias un generador, vuelve a ejecutarlo y haz commit del resultado.

**Tests:** `Unity.exe -batchmode -projectPath "<repo>" -runTests -testPlatform EditMode -testResults <xml> -logFile <log>` (sin `-quit`). Añade tests en `Assets/Tests/EditMode/` para la lógica nueva que se pueda probar sin jugar.

## Unity: reglas para no romper el proyecto

- **Unity 2022.3.56f1 está instalado en este PC** (`C:\Program Files\Unity\Hub\Editor\2022.3.56f1\Editor\Unity.exe`) y se puede usar desde la línea de comandos (modo batch). Guarda los logs en el scratchpad, nunca dentro del repo:
  - **Compilar y comprobar errores:** `Unity.exe -batchmode -quit -projectPath "<repo>" -logFile <log>`, y luego busca `error CS` en el log.
  - **Ejecutar un generador o cualquier método estático de editor:** añade `-executeMethod CodeLyokoFanGame.EditorTools.<Clase>.<Método>`.
  - **Prueba de humo:** `-buildWindows64Player <scratchpad>/build/CodeLyoko.exe`, ejecuta el `.exe` con `-batchmode -nographics -logFile <log>` durante unos 60 s y busca `Exception`, `[Mission]` y `[Lyoko]` en el log. Así se ve si la partida arranca y avanza sin errores, aunque nadie juegue.
  - **Si el usuario tiene el proyecto abierto en el Editor, el modo batch falla** (el proyecto queda bloqueado). Pídele que cierre Unity antes.
  - Lo que no puedes hacer es **jugar**: al terminar, explica qué tiene que probar el usuario con teclado y ratón y qué debería ver.
  - Si te pega errores de la consola, arregla siempre **el primero**, porque suele causar los demás.
- **Versión 2022.3.** No uses API de Unity 6 / 2023+: nada de `Rigidbody.linearVelocity` (usa `velocity`) ni `Awaitable`. Si dudas de si una API existe en 2022.3, usa la alternativa clásica.
- **Entrada:** lee siempre los controles a través de `InputReader`; **nunca uses `Input.*`**. Para una acción nueva: añádela al JSON de `LyokoControls.inputactions` (con un `id` UUID nuevo para la acción y para cada binding, con teclado y mando) y expón una propiedad en `InputReader`. No regeneres el archivo entero: los ids existentes deben mantenerse.
- **Eventos:** para avisar de algo global usa `GameManager.RaiseEvent(GameEventId.X)`; no hace falta que exista `Boot` (en tests no hace nada).
- **Daño:** nunca restes vida a mano. Crea un `DamageInfo` (con `source`, `sourceTeam`, `type` y `direction`) y pásalo a `HitResolver` (o a `Health.TakeDamage`). Las capas de físicas las crea `PhysicsLayersSetup`; el escenario va en `Environment`, los guerreros en `Player`, los monstruos en `Enemy` y los disparos en `PlayerAttack`/`EnemyAttack`.
- **Archivos `.meta`:** cada asset tiene un `.meta` con su GUID. No los borres ni los inventes. Si mueves o renombras un asset, mueve también su `.meta` (o mejor, pide al usuario que lo haga desde el Editor). Unity crea los `.meta` de los scripts nuevos al abrir el proyecto; hay que hacerles commit.
- **No edites a mano** archivos `.unity`, `.prefab` ni `.asset` (son YAML con referencias por GUID y se rompen fácilmente). Para crear o cambiar escenas y prefabs, amplía los generadores de `Assets/Game/Editor/` o da al usuario los pasos en el Editor.
- **Una clase `MonoBehaviour` o `ScriptableObject` por archivo**, y el archivo con el mismo nombre que la clase.
- **El código de editor** (`using UnityEditor`) solo puede ir en carpetas `Editor/`; si no, la build falla.
- **Renombrar un campo `[SerializeField]`** borra el valor guardado en escenas y prefabs. Usa `[FormerlySerializedAs("nombreViejo")]`.
- Los valores ajustables van en ScriptableObjects, nunca escritos fijos en el código (ver `00_REGLAS.md`).
- La vida canon de los guerreros es **100 PV**.
- Cuando crees un script o asset nuevo, ejecuta Unity en modo batch para que genere su `.meta`, y haz commit de los dos juntos.

## Subagentes

- **Máximo 2 subagentes a la vez.**
- Hasta terminar la F03, mejor ninguno: las fases van en cadena y tocan los mismos scripts.
- Dos subagentes que escriben código no pueden tocar los mismos archivos. Ejemplo válido después de la F04: uno en F05 (`Abilities/`) y otro en F06 (`Enemies/`).
- Para tareas de solo lectura (buscar en el código o en los docs) sí se pueden usar sin esperar a la F03, pero también con un máximo de 2.

## Canon y aspectos legales

- Para nombres, habilidades y monstruos sigue la Biblia. Lo marcado **[Canon]** viene de la serie; lo marcado **[Juego]** es diseño nuestro y se puede cambiar.
- Es un **fan game no comercial**. No metas música, voces, logos ni modelos oficiales de Code Lyoko. Cualquier modelo, sonido o fuente externos se registra (autor, licencia, URL) en `Assets/ArtSources/ASSET_SOURCES.md`.

## Útil

- Plantillas de issue para errores de Unity y tareas de assets: `.github/ISSUE_TEMPLATE/`.
- Primer arranque en Unity: `Docs/UNITY_FIRST_OPEN_CHECKLIST.md`.
- Proceso de modelado e importación: `Docs/MODELING_PIPELINE.md`.
- Catálogo de reparto con su estado: `Assets/ArtSources/CANON_CATALOG.md`.
