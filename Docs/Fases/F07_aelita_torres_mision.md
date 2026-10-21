# F07 — Aelita, torres y misión

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F05](F05_habilidades.md), [F06](F06_enemigos_t1.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F06](F06_enemigos_t1.md) · [F08 →](F08_vehiculos.md)

**Objetivo:** el núcleo de cada misión: llevar a Aelita a la torre.

**Requisitos:** Fases 5 y 6.

**Tareas**

- [ ] **F7.1** Mejorar `AelitaCompanion.cs`: búsqueda de camino con NavMesh, evitar enemigos, esconderse detrás de los aliados.
- [ ] **F7.2** **Órdenes a Aelita** (tecla `Z`, rueda rápida): *Seguirme / Esperar aquí / Esconderse / Ir a la torre*.
- [ ] **F7.3** Los enemigos **la eligen como objetivo** a menudo (el `CombatDirector` le da prioridad).
- [ ] **F7.4** Indicador de la vida de Aelita siempre visible y aviso cuando está en peligro.
- [ ] **F7.5** Si Aelita se devirtualiza o cae al Mar Digital → misión fallida.
- [ ] **F7.6** **Torre**: prefab final con halo de color según estado (azul neutral, rojo X.A.N.A., verde Jeremie/Aelita, blanco Franz Hopper).
- [ ] **F7.7** **Interior de la torre**: escena pequeña. Aelita entra → flota hasta la plataforma superior → panel → escribe `CODE: LYOKO` → la torre se desactiva. Primero como secuencia jugable sencilla y luego con Timeline (Fase 17).
- [ ] **F7.8** **Torre de paso**: entrar en una torre neutral y salir en otro sector.
- [ ] **F7.9** Pasar la misión del Bosque a `MissionDefinition` con estos pasos: *Intro → Llegar al puente → Combate en la meseta → Escoltar a Aelita → Jefe Megatank → Desactivar torre → Final*.
- [ ] **F7.10** Puntos de control (`Checkpoint.cs`) al principio de cada paso.
- [ ] **F7.11** Pantalla de **misión fallida** con *Reintentar desde el último punto de control*.

**Terminado cuando:** la misión del Bosque se juega completa con Aelita como compañera creíble, órdenes, torre con interior y puntos de control.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `TowerInterior`, `AelitaOrders` | Torres y órdenes a Aelita |

---

[← F06](F06_enemigos_t1.md) · [Índice](00_INDICE.md) · [F08 →](F08_vehiculos.md)
