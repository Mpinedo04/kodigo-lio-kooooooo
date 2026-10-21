# F14 — Mundo real: la fábrica

> **Bloque D** — Sistemas de la serie · **Depende de:** [F12](F12_vertical_slice_terminada.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F13](F13_modo_operador.md) · [F15 →](F15_mundo_real_kadic.md)

**Objetivo:** el primer escenario del mundo real y el ciclo “correr a la fábrica → escáneres → Lyoko”.

**Requisitos:** Fase 12.

**Tareas**

- [ ] **F14.1** **Controlador de personaje del mundo real** (`RealWorldController.cs`): andar, correr, agacharse, interactuar, sin ataques. Reutiliza la base de la Fase 3.
- [ ] **F14.2** Shader de cómic plano para el mundo real (que se distinga de Lyoko).
- [ ] **F14.3** Modelos “humanos” de la banda (ropa de calle) → **otra versión** de cada personaje, distinta del avatar de Lyoko.
- [ ] **F14.4** Escena **Fábrica**: exterior y puente, zona de entrada (cuerdas para bajar), **ascensor**, **laboratorio** (silla de Jeremie, pantallas, el terminal del superordenador), **sala de escáneres** (3 escáneres), **sala del superordenador** (la columna, en el nivel más bajo).
- [ ] **F14.5** **Ascensor** funcional con panel de código.
- [ ] **F14.6** **Escáneres**: entrar → secuencia de *Transferencia → Escáner → Virtualización* → carga de la escena de Lyoko con el personaje apareciendo desde arriba.
- [ ] **F14.7** **Salida del escáner** al devirtualizarse (el personaje aparece de rodillas en el escáner).
- [ ] **F14.8** La fábrica como **base**: punto de guardado, mejoras (Fase 18), elegir misión.
- [ ] **F14.9** Transición alcantarillas (corta, en monopatín/patinete).

**Terminado cuando:** se empieza en la fábrica, se baja en ascensor, se entra en el escáner, se juega la misión del Bosque y al terminar se vuelve a la fábrica.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `RealWorldController`, `Elevator`, `Scanner` | Fábrica |

**Assets de esta fase**

| Modelos | Animaciones | Efectos | Audio |
| --- | --- | --- | --- |
| Fábrica; banda en ropa de calle | Andar/correr/interactuar | Escáner | Ambiente de la fábrica |

---

[← F13](F13_modo_operador.md) · [Índice](00_INDICE.md) · [F15 →](F15_mundo_real_kadic.md)
