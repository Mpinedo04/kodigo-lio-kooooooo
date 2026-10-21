# F02 — Arquitectura base

> **Bloque A** — Base técnica · **Depende de:** [F01](F01_recuperar_prototipo.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F01](F01_recuperar_prototipo.md) · [F03 →](F03_movimiento_camara.md)

**Objetivo:** preparar el “esqueleto” del juego para que soporte muchos personajes, escenas y sistemas sin convertirse en un lío. Es la fase más técnica pero la más importante para no tener que rehacer todo más adelante.

**Requisitos:** Fase 1.

### 2A — Entrada (controles)

Hoy se usa el sistema antiguo de entrada de Unity (`Input.GetKey`). Para soportar mando y cambiar controles hay que pasar al **Input System**.

- [ ] **F2.1** Añadir el paquete `com.unity.inputsystem` en `Packages/manifest.json` y activar *Active Input Handling: Both* (luego solo *Input System*).
- [ ] **F2.2** Crear `Assets/Game/Input/LyokoControls.inputactions` con los mapas:
  - `Lyoko`: Move, Look, Jump, Sprint, Dodge, Attack, Special, Ability1, Ability2, Ultimate, LockOn, Vehicle, Interact, SwitchCharacter (1–4), OrderAelita, OpenOperator, SwapFront.
  - `Vehicle`: Steer, Ascend, Descend, Boost, Fire, Dismount.
  - `RealWorld`: Move, Look, Run, Crouch, Interact, Talk.
  - `Operator`: Navigate, Confirm, Cancel, Tabs.
  - `UI`: Navigate, Submit, Cancel, Pause.
- [ ] **F2.3** Generar la clase C# del `.inputactions` y crear `InputReader.cs` (ScriptableObject que expone eventos: `OnAttack`, `OnJump`…).
- [ ] **F2.4** Cambiar `PlayerController`, `PlayerCombat`, `CharacterSwitcher`, `LockOnSystem`, `VehicleRig`, `TowerObjective` para leer de `InputReader` en vez de `Input.GetKey`.
- [ ] **F2.5** Probar con teclado y con mando (Xbox/PlayStation).

### 2B — Núcleo del juego

- [ ] **F2.6** Crear `GameManager.cs` (persistente entre escenas, `DontDestroyOnLoad`) con el estado global: capítulo actual, misión actual, Nivel de X.A.N.A., personajes desbloqueados.
- [ ] **F2.7** Crear un **bus de eventos** con ScriptableObjects (`GameEvent.cs`, `GameEventListener.cs`) para que los sistemas se comuniquen sin referencias directas. Eventos iniciales: `OnPlayerDevirtualized`, `OnEnemyKilled`, `OnTowerActivated`, `OnTowerDeactivated`, `OnMissionFailed`, `OnMissionComplete`.
- [ ] **F2.8** Crear `SceneLoader.cs` con **carga aditiva** de escenas:
  - `Boot` (siempre cargada: GameManager, audio, UI global).
  - Escena de nivel (`Lyoko_Forest`, `Factory`, `Kadic`…).
  - Pantalla de carga entre escenas.
- [ ] **F2.9** Crear la escena `Assets/Scenes/Boot.unity` y ponerla como primera en *Build Settings*.
- [ ] **F2.10** Separar `MissionDirector` / `MissionManager` en: `MissionDefinition` (ScriptableObject con los pasos de la misión) + `MissionRunner` (ejecuta los pasos). Así cada misión se crea como un asset, sin código nuevo.
- [ ] **F2.11** Crear `ObjectPool.cs` genérico y usarlo para proyectiles y efectos (hoy probablemente se hace `Instantiate`/`Destroy` cada disparo).

### 2C — Datos

- [ ] **F2.12** Ampliar `CharacterDefinition` con: lista de habilidades (`AbilityDefinition`), traje por temporada, vehículo, iconos, voz.
- [ ] **F2.13** Crear `AbilityDefinition.cs` (ScriptableObject): nombre, icono, tiempo de espera, coste, prefab de efecto, tipo (dash, escudo, clon, telequinesis, creatividad…).
- [ ] **F2.14** Ampliar `EnemyDefinition` con: fases (para jefes), tabla de ataques, sonidos, efecto de devirtualización.
- [ ] **F2.15** Crear `SectorDefinition.cs` (nombre, color, escena, música, torres) y `ChapterDefinition.cs` (lista de misiones, desbloqueos).

### 2D — Depuración

- [ ] **F2.16** Crear un **menú de depuración** (tecla `F1` solo en el editor): invencible, matar a todos, saltar a la fase X de la misión, aparecer enemigo Y, ver PV de todos.
- [ ] **F2.17** Añadir *assembly definitions* (`.asmdef`) para `Game.Runtime`, `Game.Editor` y `Game.Tests` (compila más rápido).
- [ ] **F2.18** Crear las primeras **pruebas automáticas** (Unity Test Framework): `Health` recibe daño y muere a 0, `WeakPoint` multiplica el daño, el cambio de personaje mantiene la vida.

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
