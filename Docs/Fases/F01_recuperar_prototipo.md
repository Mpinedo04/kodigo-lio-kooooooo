# F01 — Recuperar y estabilizar el prototipo

> **Bloque A** — Base técnica · **Depende de:** [F00](F00_entorno.md) · **Estimación:** 1–2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F00](F00_entorno.md) · [F02 →](F02_arquitectura.md)

**Objetivo:** que lo que ya existe funcione de verdad en Unity y quede guardado (hoy la escena y los prefabs **solo se generan desde menús**, no están en el repo).

**Requisitos:** Fase 0.

**Tareas**

- [x] **F1.1** Abrir el proyecto y esperar a que importe URP y compile.
- [x] **F1.2** Apuntar **todos** los errores rojos de la consola. Arreglar siempre **el primero** y volver a compilar.
  - Compilado en modo batch con Unity 2022.3.56f1: **0 errores**.
- [x] **F1.2b** Ejecutar `Tools > Code Lyoko Fan Game > Setup URP Pipeline` → comprobar que existen `Assets/Settings/URP_Pipeline.asset` y `URP_Renderer.asset`, y que `Edit > Project Settings > Graphics` los usa. (Los generadores de F1.4 y F1.5 también lo ejecutan solos.) Sin esto todo se ve **rosa**.
- [x] **F1.3** Ejecutar `Tools > Code Lyoko Fan Game > Create Prototype Data Assets` → comprobar que existe `Assets/Game/Data/RosterDatabase.asset`.
- [x] **F1.4** Ejecutar `Tools > Code Lyoko Fan Game > Create Full Roster Placeholder Prefabs` → comprobar `Assets/Game/Generated/FullRosterPlaceholders/`.
- [x] **F1.5** Ejecutar `Tools > Code Lyoko Fan Game > Build Vertical Slice Scene` y guardar `Assets/Levels/ForestSectorVerticalSlice.unity`.
- [x] **F1.6** Pasar la lista de `Docs/UNITY_FIRST_OPEN_CHECKLIST.md` → apartado *What To Check* punto por punto.
  - Comprobado sin jugar: assets de URP, `RosterDatabase.asset`, prefabs, escena y tag `Enemy` existen. Prueba automática (build ejecutada 60 s sin nadie a los mandos): la misión arranca, Odd → Ulrich → Yumi se devirtualizan por turnos y la misión pasa a *Failed*, sin excepciones. Probado jugando por el usuario: funciona.
- [x] **F1.7** (Lo tiene que hacer una persona) Jugar la misión entera de principio a fin y anotar en un issue todo lo que falle o se sienta mal (movimiento, cámara, enemigos, torre…).
- [x] **F1.8** Corregir los fallos que **impidan terminar la misión** (los de “se siente mal” van a fases posteriores).
  - Ya corregidos antes de abrir Unity: URP sin configurar (todo rosa); al morir un personaje se rompía el cambio de personaje (ahora pasa al siguiente vivo); Aelita no contaba como “cerca de la torre” por la altura; no había forma de reintentar (ahora, si caen todos, `R` reinicia); el HUD muestra los enemigos restantes.
- [x] **F1.9** Ajustar la vida base a **100 PV** para que coincida con la serie: `CharacterDefinition`, `PrototypeDataAssetBuilder` y los jugadores de la escena generada.
- [x] **F1.10** Hacer commit de la escena, los datos, los prefabs generados, `Assets/Settings/`, **todos los `.meta`** (hoy no hay ninguno en git) y **`ProjectSettings/`** (hoy solo está `ProjectVersion.txt`; Unity creará el resto al abrir, incluido el tag `Enemy`).
- [x] **F1.11** Crear la etiqueta git `v0.1-prototipo` para tener un punto de vuelta seguro.

**Archivos:** `Assets/Levels/ForestSectorVerticalSlice.unity`, `Assets/Game/Data/*.asset`, `Assets/Game/Generated/**`, `Assets/Settings/*`, `ProjectSettings/*`, `Assets/Game/Editor/UrpPipelineSetup.cs` (nuevo), `CharacterSwitcher.cs`, `MissionDirector.cs`, `AelitaCompanion.cs`, `TowerObjective.cs`, `GameHud.cs`, `CharacterDefinition.cs`.

**Terminado cuando:** con el repo recién clonado se abre la escena, se pulsa Play y se completa la misión sin errores.

**Riesgos:** los generadores del editor (`Assets/Game/Editor/*.cs`) pueden fallar con la versión exacta de URP. Solución: arreglar el generador, no la escena a mano.

---

[← F00](F00_entorno.md) · [Índice](00_INDICE.md) · [F02 →](F02_arquitectura.md)
