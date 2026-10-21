# F10 — HUD, menús y UI

> **Bloque C** — Presentación · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F09](F09_arte_vertical_slice.md) · [F11 →](F11_audio.md)

**Objetivo:** toda la interfaz de la vertical slice.

**Requisitos:** Fase 4.

**Tareas — HUD en Lyoko** (estilo pantalla de Jeremie)

- [ ] **F10.1** Retratos de los guerreros con **PV numéricos** (100) y barra.
- [ ] **F10.2** PV de Aelita destacados.
- [ ] **F10.3** Iconos de habilidades con tiempo de espera, barra de energía especial, barra de Concentración (Yumi).
- [ ] **F10.4** Retícula de fijado de objetivo y vida del enemigo fijado.
- [ ] **F10.5** Minimapa o brújula del sector con la torre activada marcada.
- [ ] **F10.6** Avisos: “¡Torre activada!”, “¡Aelita en peligro!”, “PV bajos”.
- [ ] **F10.7** Subtítulos de Jeremie (caja de diálogo con su retrato).
- [ ] **F10.8** Barra de vida de jefe.

**Tareas — menús**

- [ ] **F10.9** Pantalla de título (logo propio de fan game, no el oficial).
- [ ] **F10.10** Menú principal: Nueva partida, Continuar, Capítulos, Extras, Opciones, Salir.
- [ ] **F10.11** Menú de **pausa**: Continuar, Reintentar, Opciones, Salir al menú.
- [ ] **F10.12** **Opciones**: gráficos (resolución, calidad, pantalla completa), audio (general, música, efectos, voces), controles (sensibilidad, invertir Y, reasignar teclas con el Input System), accesibilidad (tamaño de subtítulos, números de daño, vibración).
- [ ] **F10.13** Pantalla de **resultados** de misión: tiempo, monstruos devirtualizados, PV perdidos, Datos ganados, nota (S/A/B/C).
- [ ] **F10.14** Pantalla de carga con consejos.
- [ ] **F10.15** Navegación completa con mando (foco visible en los botones).

**Archivos:** `GameHud.cs` (dividir en componentes), nuevos en `Assets/Game/Scripts/UI/`, prefabs en `Assets/Prefabs/UI/`, fuente con estilo digital (licencia libre).

**Terminado cuando:** se puede jugar del menú principal a la misión, pausar, cambiar opciones y ver los resultados solo con mando.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| Componentes de `UI/` (HUD, menús, opciones, resultados) | Interfaz |

---

[← F09](F09_arte_vertical_slice.md) · [Índice](00_INDICE.md) · [F11 →](F11_audio.md)
