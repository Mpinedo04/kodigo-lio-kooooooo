# F17 — Historia: diálogos, cinemáticas y guardado

> **Bloque D** — Sistemas de la serie · **Depende de:** [F12](F12_vertical_slice_terminada.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F16](F16_misiones_divididas.md) · [F18 →](F18_progresion_retorno.md)

**Objetivo:** las herramientas para contar la historia.

**Requisitos:** Fase 12 (en paralelo con 13–16).

**Tareas — diálogo**

- [ ] **F17.1** Sistema de diálogos con `DialogueDefinition` (asset): líneas, personaje, retrato, expresión, opciones de respuesta.
- [ ] **F17.2** Caja de diálogo para el mundo real y subtítulos de radio de Jeremie para Lyoko.
- [ ] **F17.3** Variables de historia (`StoryFlags`): “Aelita materializada”, “Sector 5 descubierto”… para que los diálogos cambien.
- [ ] **F17.4** **Localización**: textos en tablas (paquete *Localization*) desde el principio → español primero, inglés y francés más adelante.

**Tareas — cinemáticas**

- [ ] **F17.5** Usar **Timeline** (ya está en el manifest) para las escenas: cámaras, animaciones, diálogos.
- [ ] **F17.6** Cinemáticas reutilizables: **virtualización**, **desactivación de torre (CODE: LYOKO)**, **Retorno al pasado**, **devirtualización**.
- [ ] **F17.7** Poder saltar las cinemáticas.

**Tareas — guardado**

- [ ] **F17.8** `SaveSystem.cs`: guarda en JSON en `Application.persistentDataPath`. Datos: capítulo, misiones completadas, desbloqueos, Datos, Nivel de X.A.N.A., coleccionables, opciones.
- [ ] **F17.9** 3 ranuras de partida + autoguardado al terminar cada misión.
- [ ] **F17.10** Versión del archivo de guardado (para no romper partidas al actualizar el juego).

**Terminado cuando:** se puede ver la cinemática de entrada, jugar, ver la cinemática de CODE: LYOKO y del Retorno al pasado, salir del juego y continuar desde el menú.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `DialogueDefinition`, `DialogueRunner`, `StoryFlags`, `SaveSystem` | Historia y guardado |

---

[← F16](F16_misiones_divididas.md) · [Índice](00_INDICE.md) · [F18 →](F18_progresion_retorno.md)
