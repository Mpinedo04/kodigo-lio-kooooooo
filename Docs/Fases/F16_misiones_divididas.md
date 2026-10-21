# F16 — Misiones divididas y ataques de X.A.N.A.

> **Bloque D** — Sistemas de la serie · **Depende de:** [F13](F13_modo_operador.md), [F15](F15_mundo_real_kadic.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F15](F15_mundo_real_kadic.md) · [F17 →](F17_historia_dialogos_guardado.md)

**Objetivo:** la mecánica estrella: luchar en Lyoko **y** sobrevivir en la Tierra a la vez (Biblia 3 y 10).

**Requisitos:** Fases 13 y 15.

**Tareas**

- [ ] **F16.1** Tener **dos escenas cargadas a la vez** (Lyoko + mundo real) y alternar el control con `G`. La que no se controla sigue con IA.
- [ ] **F16.2** **Barra de amenaza** del ataque en la Tierra: sube con el tiempo; al llenarse, misión fallida.
- [ ] **F16.3** Avisos cuando el otro frente está en peligro (“¡Ulrich necesita ayuda en Kadic!”).
- [ ] **F16.4** Al devirtualizarse en Lyoko, el personaje **pasa al frente de la Tierra** (conectado con F14.7).
- [ ] **F16.5** Ataque tipo **Espectro**: enemigo que persigue; se le aturde con extintores o electricidad, no se le puede matar.
- [ ] **F16.6** Ataque tipo **Poseídos**: PNJ con el ojo de X.A.N.A., más fuertes y rápidos; hay que esquivarlos o encerrarlos.
- [ ] **F16.7** Ataque tipo **Clon polimórfico**: minijuego de detectar al impostor.
- [ ] **F16.8** Ataque tipo **Máquinas/electricidad**: puzles de entorno (cortar la corriente, escapar de un coche).
- [ ] **F16.9** Ataque tipo **Clima/naturaleza**: niebla que daña, enjambres, tormenta.
- [ ] **F16.10** Ataques definidos como `XanaAttackDefinition` (asset) para combinarlos con cualquier misión.

**Terminado cuando:** una misión con Odd y Yumi en Lyoko y Ulrich en Kadic contra un Espectro se puede ganar alternando entre ambos frentes.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `SplitMissionController`, `ThreatMeter`, `XanaAttackDefinition`, `Specter`, `PossessedNpc` | Misiones divididas |

**Assets de esta fase**

| Modelos | Animaciones | Efectos | Audio |
| --- | --- | --- | --- |
| Espectro, poseídos | Poseído | Espectro, ojo de X.A.N.A. | Tema de ataque |

---

[← F15](F15_mundo_real_kadic.md) · [Índice](00_INDICE.md) · [F17 →](F17_historia_dialogos_guardado.md)
