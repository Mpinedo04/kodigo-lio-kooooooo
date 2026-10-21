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

Todo está en el namespace `CodeLyokoFanGame` (`.Data` para las definiciones).

| Archivo (`Assets/Game/Scripts/`) | Qué hace |
| --- | --- |
| `PlayerController.cs` | Movimiento, salto, esquiva y montar vehículo del guerrero activo |
| `PlayerCombat.cs` | Ataque básico y especial (cuerpo a cuerpo o proyectil) según `AttackStyle` |
| `CharacterSwitcher.cs` | Cambio entre Odd/Ulrich/Yumi con `1`/`2`/`3` |
| `AelitaCompanion.cs` | IA de Aelita: seguir al jugador e ir a la torre |
| `EnemyAI.cs` | IA de todos los monstruos en un solo script (~520 líneas; F06 lo divide en estados) |
| `Health.cs`, `WeakPoint.cs`, `Projectile.cs` | Vida y devirtualización, ojo de X.A.N.A. (multiplica el daño) y proyectiles |
| `LockOnSystem.cs`, `ThirdPersonCamera.cs` | Fijar objetivo y cámara |
| `VehicleRig.cs` | Vehículos (Overboard/Overbike/Overwing) |
| `MissionDirector.cs`, `MissionManager.cs`, `TowerObjective.cs`, `WaveSpawner.cs`, `Checkpoint.cs` | Fases de la misión, torre, oleadas y puntos de control |
| `DigitalSeaHazard.cs` | Caer al Mar Digital = devirtualización |
| `GameHud.cs` | HUD provisional |
| `GameEnums.cs` | Enums: personajes, estilos de ataque, vehículos, tipos y comportamientos de enemigos, fases de misión |
| `Data/*Definition.cs`, `Data/RosterDatabase.cs` | ScriptableObjects con los valores de personajes, enemigos y vehículos |

Los scripts de editor (`Assets/Game/Editor/`) crean contenido desde el menú **Tools > Code Lyoko Fan Game**: `Setup URP Pipeline` (crea y asigna el asset de URP en `Assets/Settings/`), `Create Prototype Data Assets`, `Create Full Roster Placeholder Prefabs` y `Build Vertical Slice Scene`. La escena (`Assets/Levels/ForestSectorVerticalSlice.unity`), los datos (`Assets/Game/Data/`), los prefabs (`Assets/Game/Generated/`) y la configuración de URP (`Assets/Settings/`) ya están generados y guardados en git. Si cambias un generador, vuelve a ejecutarlo y haz commit de lo que genere.

## Unity: reglas para no romper el proyecto

- **Unity 2022.3.56f1 está instalado en este PC** (`C:\Program Files\Unity\Hub\Editor\2022.3.56f1\Editor\Unity.exe`) y se puede usar desde la línea de comandos (modo batch). Guarda los logs en el scratchpad, nunca dentro del repo:
  - **Compilar y comprobar errores:** `Unity.exe -batchmode -quit -projectPath "<repo>" -logFile <log>`, y luego busca `error CS` en el log.
  - **Ejecutar un generador o cualquier método estático de editor:** añade `-executeMethod CodeLyokoFanGame.EditorTools.<Clase>.<Método>`.
  - **Prueba de humo:** `-buildWindows64Player <scratchpad>/build/CodeLyoko.exe`, ejecuta el `.exe` con `-batchmode -nographics -logFile <log>` durante unos 60 s y busca `Exception`, `[Mission]` y `[Lyoko]` en el log. Así se ve si la partida arranca y avanza sin errores, aunque nadie juegue.
  - **Si el usuario tiene el proyecto abierto en el Editor, el modo batch falla** (el proyecto queda bloqueado). Pídele que cierre Unity antes.
  - Lo que no puedes hacer es **jugar**: al terminar, explica qué tiene que probar el usuario con teclado y ratón y qué debería ver.
  - Si te pega errores de la consola, arregla siempre **el primero**, porque suele causar los demás.
- **Versión 2022.3.** No uses API de Unity 6 / 2023+: nada de `Rigidbody.linearVelocity` (usa `velocity`) ni `Awaitable`. Si dudas de si una API existe en 2022.3, usa la alternativa clásica.
- **Entrada:** hasta que se termine la F02, el proyecto usa el sistema antiguo (`Input.GetKey`, `Input.GetAxis`). Tras la F02 se usa el Input System con `InputReader`, y entonces no hay que volver a usar `Input.*`.
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
