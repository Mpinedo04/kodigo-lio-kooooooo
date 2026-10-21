# F01 — Recuperar y estabilizar el prototipo

> **Bloque A** — Base técnica · **Depende de:** [F00](F00_entorno.md) · **Estimación:** 1–2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F00](F00_entorno.md) · [F02 →](F02_arquitectura.md)

**Objetivo:** que lo que ya existe funcione de verdad en Unity y quede guardado (hoy la escena y los prefabs **solo se generan desde menús**, no están en el repo).

**Requisitos:** Fase 0.

**Tareas**

- [ ] **F1.1** Abrir el proyecto y esperar a que importe URP y compile.
- [ ] **F1.2** Apuntar **todos** los errores rojos de la consola. Arreglar siempre **el primero** y volver a compilar.
- [ ] **F1.3** Ejecutar `Tools > Code Lyoko Fan Game > Create Prototype Data Assets` → comprobar que existe `Assets/Game/Data/RosterDatabase.asset`.
- [ ] **F1.4** Ejecutar `Tools > Code Lyoko Fan Game > Create Full Roster Placeholder Prefabs` → comprobar `Assets/Game/Generated/FullRosterPlaceholders/`.
- [ ] **F1.5** Ejecutar `Tools > Code Lyoko Fan Game > Build Vertical Slice Scene` y guardar `Assets/Levels/ForestSectorVerticalSlice.unity`.
- [ ] **F1.6** Pasar la lista de `Docs/UNITY_FIRST_OPEN_CHECKLIST.md` → apartado *What To Check* punto por punto.
- [ ] **F1.7** Jugar la misión entera de principio a fin y anotar en un issue todo lo que falle o se sienta mal (movimiento, cámara, enemigos, torre…).
- [ ] **F1.8** Corregir los fallos que **impidan terminar la misión** (los de “se siente mal” van a fases posteriores).
- [ ] **F1.9** Ajustar la vida base a **100 PV** (hoy `CharacterDefinition.maxHealth` vale 120 por defecto) para que coincida con la serie.
- [ ] **F1.10** Hacer commit de la escena, los datos y los prefabs generados.
- [ ] **F1.11** Crear la etiqueta git `v0.1-prototipo` para tener un punto de vuelta seguro.

**Archivos:** `Assets/Levels/ForestSectorVerticalSlice.unity`, `Assets/Game/Data/*.asset`, `Assets/Game/Generated/**`, `Assets/Game/Scripts/Data/CharacterDefinition.cs`.

**Terminado cuando:** con el repo recién clonado se abre la escena, se pulsa Play y se completa la misión sin errores.

**Riesgos:** los generadores del editor (`Assets/Game/Editor/*.cs`) pueden fallar con la versión exacta de URP. Solución: arreglar el generador, no la escena a mano.

---

[← F00](F00_entorno.md) · [Índice](00_INDICE.md) · [F02 →](F02_arquitectura.md)
