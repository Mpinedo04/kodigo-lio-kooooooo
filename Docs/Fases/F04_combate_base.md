# F04 — Combate base

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F03](F03_movimiento_camara.md) · [F05 →](F05_habilidades.md)

**Objetivo:** un sistema de combate sólido y común a todos los personajes, sobre el que luego se montan las habilidades.

**Requisitos:** Fase 3.

**Tareas — daño y vida**

- [ ] **F4.1** Crear `DamageInfo` (struct): cantidad, origen, tipo (láser, cuerpo a cuerpo, caída), punto de impacto, si es crítico, fuerza de empuje.
- [ ] **F4.2** Cambiar `Health.cs` para usar `DamageInfo` y lanzar eventos `OnDamaged`, `OnHealed`, `OnDied`.
- [ ] **F4.3** Crear `Hurtbox.cs` (dónde recibe golpes cada personaje/enemigo) y `Hitbox.cs` (dónde golpea un ataque). Capas de físicas separadas: `Player`, `Enemy`, `PlayerAttack`, `EnemyAttack`, `Environment`, `DigitalSea`.
- [ ] **F4.4** **Invulnerabilidad** breve tras recibir golpe (0,5 s) con parpadeo.
- [ ] **F4.5** **Devirtualización:** al llegar a 0 PV → animación/efecto → el personaje sale del grupo → evento `OnPlayerDevirtualized`. Si caen todos (o Aelita) → misión fallida.
- [ ] **F4.6** Caer al Mar Digital = devirtualización instantánea (ya hay `DigitalSeaHazard.cs`, conectarlo a esto).

**Tareas — ataques**

- [ ] **F4.7** Sistema de **combos** cuerpo a cuerpo: lista de golpes con ventana de entrada (pulsar durante el final de un golpe enlaza el siguiente).
- [ ] **F4.8** Ataques a distancia: proyectiles con velocidad, gravedad opcional, perforación y **ayuda de puntería** hacia el objetivo fijado.
- [ ] **F4.9** **Ataque cargado** (mantener el botón).
- [ ] **F4.10** **Barra de energía especial**: se llena al golpear y al recibir daño; se gasta en la definitiva.
- [ ] **F4.11** **Hit-stop** (congelar 0,05 s al dar un golpe fuerte) y retroceso de los enemigos.
- [ ] **F4.12** **Desvío/parada**: pulsar bloquear justo antes del impacto anula el daño (base para Ulrich).

**Tareas — objetivo y enemigos**

- [ ] **F4.13** Mejorar `LockOnSystem`: cambiar de objetivo con el stick derecho, priorizar al más cercano al centro de la cámara, soltar si el objetivo se aleja o muere.
- [ ] **F4.14** `WeakPoint` (ojo de X.A.N.A.): daño ×2,5 y, en enemigos pequeños, muerte instantánea. Indicador visual cuando el ojo queda expuesto.
- [ ] **F4.15** Números de daño flotantes (se pueden desactivar en opciones).

**Archivos:** `Health.cs`, `PlayerCombat.cs`, `Projectile.cs`, `LockOnSystem.cs`, `WeakPoint.cs`, `DigitalSeaHazard.cs`; nuevos: `Combat/DamageInfo.cs`, `Combat/Hitbox.cs`, `Combat/Hurtbox.cs`, `Combat/ComboDefinition.cs`, `Combat/SpecialMeter.cs`, `Combat/HitStop.cs`.

**Terminado cuando:** se puede luchar contra un Kankrelat y un Krabe en una escena de pruebas (`Assets/Scenes/Test/CombatArena.unity`) con combos, carga, esquiva, desvío, fijado y devirtualización, todo ajustable desde assets.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `DamageInfo`, `Hitbox`, `Hurtbox` | Sistema de daño |
| `ComboDefinition`, `SpecialMeter`, `HitStop` | Combos, energía y sensación de golpe |

---

[← F03](F03_movimiento_camara.md) · [Índice](00_INDICE.md) · [F05 →](F05_habilidades.md)
