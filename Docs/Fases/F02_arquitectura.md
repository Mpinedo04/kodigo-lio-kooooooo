# F02 — Arquitectura base

> **Bloque A** — Base técnica · **Depende de:** [F01](F01_recuperar_prototipo.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F01](F01_recuperar_prototipo.md) · [F03 →](F03_movimiento_camara.md)

**Objetivo:** preparar el “esqueleto” del juego para que soporte muchos personajes, escenas y sistemas sin convertirse en un lío. Es la fase más técnica pero la más importante para no tener que rehacer todo más adelante.

**Requisitos:** Fase 1.

### 2A — Entrada (controles)

Hoy se usa el sistema antiguo de entrada de Unity (`Input.GetKey`). Para soportar mando y cambiar controles hay que pasar al **Input System**.

- [x] **F2.1** Añadir el paquete `com.unity.inputsystem` en `Packages/manifest.json` y activar *Active Input Handling: Both* (luego solo *Input System*).
- [x] **F2.2** Crear `Assets/Game/Input/LyokoControls.inputactions` con los mapas:
  - `Lyoko`: Move, Look, Jump, Sprint, Dodge, Attack, Special, Ability1, Ability2, Ultimate, LockOn, Vehicle, Interact, SwitchCharacter (1–4), OrderAelita, OpenOperator, SwapFront.
  - `Vehicle`: Steer, Ascend, Descend, Boost, Fire, Dismount.
  - `RealWorld`: Move, Look, Run, Crouch, Interact, Talk.
  - `Operator`: Navigate, Confirm, Cancel, Tabs.
  - `UI`: Navigate, Submit, Cancel, Pause.
  - Hecho con 45 acciones. Además: `NextCharacter`/`PreviousCharacter` (cruceta del mando), `Retry` y `ToggleDebug` (F1) en `UI`. Se genera con un script, porque cada acción necesita un id único.
- [x] **F2.3** Generar la clase C# del `.inputactions` y crear `InputReader.cs` (ScriptableObject que expone eventos: `OnAttack`, `OnJump`…).
  - Cambio respecto al plan: no hace falta generar la clase C#. `InputReader` busca las acciones por nombre y ofrece propiedades de lectura (`JumpPressed`, `Move`, `InteractHeld`…), que encajan con el código actual basado en `Update`. Asset: `Assets/Game/Input/InputReader.asset`.
- [x] **F2.4** Cambiar `PlayerController`, `PlayerCombat`, `CharacterSwitcher`, `LockOnSystem`, `VehicleRig`, `TowerObjective` para leer de `InputReader` en vez de `Input.GetKey`.
  - Hecho en `PlayerController`, `CharacterSwitcher`, `ThirdPersonCamera`, `TowerObjective`, `MissionRunner` y `DebugMenu` (`PlayerCombat`, `LockOnSystem` y `VehicleRig` no leían teclas). Queda `Active Input Handling: Both`; no queda ningún `Input.*` en el código.
- [x] **F2.5** Probar con teclado y con mando (Xbox/PlayStation). Probado por el usuario antes de empezar la F03.

### 2B — Núcleo del juego

- [x] **F2.6** Crear `GameManager.cs` (persistente entre escenas, `DontDestroyOnLoad`) con el estado global: capítulo actual, misión actual, Nivel de X.A.N.A., personajes desbloqueados.
- [x] **F2.7** Crear un **bus de eventos** con ScriptableObjects (`GameEvent.cs`, `GameEventListener.cs`) para que los sistemas se comuniquen sin referencias directas. Eventos iniciales: `OnPlayerDevirtualized`, `OnEnemyKilled`, `OnTowerActivated`, `OnTowerDeactivated`, `OnMissionFailed`, `OnMissionComplete`.
- [x] **F2.8** Crear `SceneLoader.cs` con **carga aditiva** de escenas:
  - `Boot` (siempre cargada: GameManager, audio, UI global).
  - Escena de nivel (`Lyoko_Forest`, `Factory`, `Kadic`…).
  - Pantalla de carga entre escenas.
  - Hecho: `SceneLoader` + `BootLoader` (si das a Play desde un nivel, añade `Boot` solo). Pantalla de carga “Virtualizando...”. Reintentar recarga el nivel sin tocar `Boot`.
- [x] **F2.9** Crear la escena `Assets/Scenes/Boot.unity` y ponerla como primera en *Build Settings*.
- [x] **F2.10** Separar `MissionDirector` / `MissionManager` en: `MissionDefinition` (ScriptableObject con los pasos de la misión) + `MissionRunner` (ejecuta los pasos). Así cada misión se crea como un asset, sin código nuevo.
  - Hecho: `MissionRunner` + `MissionMarker`; la misión del Bosque es `Assets/Game/Data/Missions/M01_ForestTower.asset` (7 pasos). `MissionDirector` y `MissionManager` borrados; el panel superior pasó a `GameHud`.
- [x] **F2.11** Crear `ObjectPool.cs` genérico y usarlo para proyectiles y efectos (hoy probablemente se hace `Instantiate`/`Destroy` cada disparo).
  - Hecho como `PrefabPool` (+ `PooledObject`); los proyectiles de jugadores y monstruos ya se reciclan.

### 2C — Datos

- [x] **F2.12** Ampliar `CharacterDefinition` con: lista de habilidades (`AbilityDefinition`), traje por temporada, vehículo, iconos, voz.
- [x] **F2.13** Crear `AbilityDefinition.cs` (ScriptableObject): nombre, icono, tiempo de espera, coste, prefab de efecto, tipo (dash, escudo, clon, telequinesis, creatividad…).
- [x] **F2.14** Ampliar `EnemyDefinition` con: fases (para jefes), tabla de ataques, sonidos, efecto de devirtualización.
- [x] **F2.15** Crear `SectorDefinition.cs` (nombre, color, escena, música, torres) y `ChapterDefinition.cs` (lista de misiones, desbloqueos).
  - Datos generados: 17 habilidades (Biblia, sección 5), 6 sectores, la misión del Bosque y los capítulos 0 y 1.

### 2D — Depuración

- [x] **F2.16** Crear un **menú de depuración** (tecla `F1` solo en el editor): invencible, matar a todos, saltar a la fase X de la misión, aparecer enemigo Y, ver PV de todos.
  - Hecho en la escena `Boot`. Funciona en el editor y en builds de desarrollo.
- [x] **F2.17** Añadir *assembly definitions* (`.asmdef`) para `Game.Runtime`, `Game.Editor` y `Game.Tests` (compila más rápido).
- [x] **F2.18** Crear las primeras **pruebas automáticas** (Unity Test Framework): `Health` recibe daño y muere a 0, `WeakPoint` multiplica el daño, el cambio de personaje mantiene la vida.
  - 10 tests en `Assets/Tests/EditMode/` (Health, WeakPoint, PrefabPool, CharacterSwitcher): **10/10 pasan**.

**Archivos nuevos:** `Assets/Game/Input/*`, `Assets/Game/Scripts/Core/GameManager.cs`, `SceneLoader.cs`, `ObjectPool.cs`, `Assets/Game/Scripts/Events/*`, `Assets/Game/Scripts/Data/AbilityDefinition.cs`, `SectorDefinition.cs`, `ChapterDefinition.cs`, `MissionDefinition.cs`, `Assets/Game/Scripts/Debug/DebugMenu.cs`, `Assets/Scenes/Boot.unity`, `Assets/Tests/*`.

**Terminado cuando:** la misión del Bosque funciona igual que antes pero arrancando desde `Boot`, con mando, sin `Input.GetKey` en ningún script, y con la misión definida como asset.

**Riesgos:** es fácil pasarse “sobrediseñando”. Regla: solo crear lo que la Fase 4–7 vaya a usar de verdad.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `InputReader` | Leer los controles del Input System y avisar con eventos |
| `GameManager` | Estado global del juego |
| `SceneLoader` | Carga aditiva de escenas y pantalla de carga |
| `GameEvent`, `GameEventListener` | Bus de eventos con ScriptableObjects |
| `ObjectPool` | Reutilizar proyectiles y efectos |
| `AbilityDefinition`, `SectorDefinition`, `ChapterDefinition`, `MissionDefinition` | Datos editables |
| `MissionRunner` | Ejecuta los pasos de una misión |
| `DebugMenu` | Trucos de depuración |

---

[← F01](F01_recuperar_prototipo.md) · [Índice](00_INDICE.md) · [F03 →](F03_movimiento_camara.md)
