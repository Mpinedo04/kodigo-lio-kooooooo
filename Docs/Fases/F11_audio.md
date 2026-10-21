# F11 — Audio

> **Bloque C** — Presentación · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 1–2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F10](F10_hud_menus_ui.md) · [F12 →](F12_vertical_slice_terminada.md)

**Objetivo:** sonido completo de la vertical slice.

**Requisitos:** Fase 4.

**Tareas**

- [ ] **F11.1** `AudioManager` con **mezclador** (`AudioMixer`): grupos Música, Efectos, Voces, UI, Ambiente. Conectado a las opciones.
- [ ] **F11.2** **Música propia** (sin usar la oficial): tema de Lyoko/combate, tema de exploración del Bosque, tema de jefe, tema del menú. Fuentes: componer, o música libre con licencia que lo permita (anotarla en `ASSET_SOURCES.md`).
- [ ] **F11.3** Música **dinámica**: cambia a combate cuando hay enemigos cerca y vuelve a exploración al acabar.
- [ ] **F11.4** Efectos: pasos, salto, esquiva, flecha, katana, abanico, campo de energía, escudo, láser de monstruo, devirtualización, materialización, vehículos, torre, UI.
- [ ] **F11.5** Sonido 3D en monstruos (se oyen más cerca o más lejos).
- [ ] **F11.6** Ambiente de cada sector (viento digital, zumbido).
- [ ] **F11.7** Voces: de momento “voces” sin palabras o pitidos; si alguien graba voces de fan, añadir líneas de Jeremie.

**Terminado cuando:** jugar la misión sin mirar la pantalla permite saber qué está pasando (ataque que viene, golpe recibido, torre desactivada).

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `AudioManager`, `DynamicMusic` | Sonido y música |

**Assets de esta fase**

| Modelos | Animaciones | Efectos | Audio |
| --- | --- | --- | --- |
| — | — | — | 4 temas musicales, ~40 efectos |

---

[← F10](F10_hud_menus_ui.md) · [Índice](00_INDICE.md) · [F12 →](F12_vertical_slice_terminada.md)
