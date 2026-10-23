# F04 — Combate base

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F03](F03_movimiento_camara.md) · [F05 →](F05_habilidades.md)

**Objetivo:** un sistema de combate sólido y común a todos los personajes, sobre el que luego se montan las habilidades.

**Requisitos:** Fase 3.

**Tareas — daño y vida**

- [x] **F4.1** Crear `DamageInfo` (struct): cantidad, origen, tipo (láser, cuerpo a cuerpo, caída), punto de impacto, si es crítico, fuerza de empuje.
- [x] **F4.2** Cambiar `Health.cs` para usar `DamageInfo` y lanzar eventos `OnDamaged`, `OnHealed`, `OnDied`.
- [x] **F4.3** Crear `Hurtbox.cs` (dónde recibe golpes cada personaje/enemigo) y `Hitbox.cs` (dónde golpea un ataque). Capas de físicas separadas: `Player`, `Enemy`, `PlayerAttack`, `EnemyAttack`, `Environment`, `DigitalSea`.
- [x] **F4.4** **Invulnerabilidad** breve tras recibir golpe (0,5 s) con parpadeo.
- [x] **F4.5** **Devirtualización:** al llegar a 0 PV → animación/efecto → el personaje sale del grupo → evento `OnPlayerDevirtualized`. Si caen todos (o Aelita) → misión fallida.
- [x] **F4.6** Caer al Mar Digital = devirtualización instantánea (ya hay `DigitalSeaHazard.cs`, conectarlo a esto).
  - Hecho: `Health` usa `DamageInfo` (equipo, tipo, dirección, crítico, empuje) y tiene `Heal`, `Kill` y el evento estático `AnyDamaged`. `Hurtbox` + `WeakPoint` reciben golpes; `HitResolver` aplica un solo golpe por objetivo, prefiere el ojo expuesto y nunca daña a tu propio equipo. Capas `Player`, `Enemy`, `PlayerAttack`, `EnemyAttack`, `Environment` y `DigitalSea` con su matriz de colisiones (`PhysicsLayersSetup`). Tras un golpe, 0,5 s de invulnerabilidad con parpadeo (`DamageFlicker`). Al morir, efecto provisional de polígonos (`DevirtualizeEffect`). La misión falla si cae Aelita, y Aelita ya no se tira por los bordes. El Mar Digital devirtualiza cualquier cosa; en las escenas de prueba, a los guerreros los devuelve al punto de reaparición.

**Tareas — ataques**

- [x] **F4.7** Sistema de **combos** cuerpo a cuerpo: lista de golpes con ventana de entrada (pulsar durante el final de un golpe enlaza el siguiente).
- [x] **F4.8** Ataques a distancia: proyectiles con velocidad, gravedad opcional, perforación y **ayuda de puntería** hacia el objetivo fijado.
- [x] **F4.9** **Ataque cargado** (mantener el botón).
- [x] **F4.10** **Barra de energía especial**: se llena al golpear y al recibir daño; se gasta en la definitiva.
- [x] **F4.11** **Hit-stop** (congelar 0,05 s al dar un golpe fuerte) y retroceso de los enemigos.
- [x] **F4.12** **Desvío/parada**: pulsar bloquear justo antes del impacto anula el daño (base para Ulrich).
  - Hecho: combos con `ComboDefinition` (Ulrich: 3 tajos con remate; William: 2 golpes), con buffer de pulsaciones (`ComboTracker`). Ataque cargado manteniendo Atacar 0,5 s (cuerpo a cuerpo: golpe en área con empuje; a distancia: disparo grande que perfora 3 enemigos). Proyectiles con gravedad opcional, perforación, choque con paredes y ayuda de puntería (cono de 12°). `SpecialMeter` se llena al dar y recibir daño. `HitStop` en golpes fuertes y empuje (los Megatanks no se mueven). Nueva acción **Guardia** (`C` / LT) con una ventana de 0,15 s (`PlayerGuard`); la definitiva del mando pasa a **LB + RB**. Todos los valores están en `Assets/Game/Data/CombatProfile.asset` y en `Assets/Game/Data/Combos/`.

**Tareas — objetivo y enemigos**

- [x] **F4.13** Mejorar `LockOnSystem`: cambiar de objetivo con el stick derecho, priorizar al más cercano al centro de la cámara, soltar si el objetivo se aleja o muere.
- [x] **F4.14** `WeakPoint` (ojo de X.A.N.A.): daño ×2,5 y, en enemigos pequeños, muerte instantánea. Indicador visual cuando el ojo queda expuesto.
- [x] **F4.15** Números de daño flotantes (se pueden desactivar en opciones).
  - Hecho: el fijado elige al enemigo más centrado en pantalla y cambia de objetivo con un golpe rápido de stick o ratón (`TargetingMath`, con tests). El ojo de X.A.N.A. crece un instante al abrirse y hace morir al instante a Kankrelats y Hornets. Los números de daño (`DamageNumbers`, en `Boot`) salen amarillos en golpes al ojo y rojos cuando el daño lo recibes tú; se activan o desactivan en el menú `F1`.

**Archivos:** `Health.cs`, `PlayerCombat.cs`, `Projectile.cs`, `LockOnSystem.cs`, `WeakPoint.cs`, `DigitalSeaHazard.cs`; nuevos: `Combat/DamageInfo.cs`, `Combat/Hitbox.cs`, `Combat/Hurtbox.cs`, `Combat/ComboDefinition.cs`, `Combat/SpecialMeter.cs`, `Combat/HitStop.cs`.

**Terminado cuando:** se puede luchar contra un Kankrelat y un Krabe en una escena de pruebas (`Assets/Scenes/Test/CombatArena.unity`) con combos, carga, esquiva, desvío, fijado y devirtualización, todo ajustable desde assets.

- [x] **F4.16** Escena `Assets/Scenes/Test/CombatArena.unity` (`CombatArenaBuilder`, incluida en *Rebuild Everything*): arena cerrada con coberturas, dos Kankrelats, un Krabe y un Blok que dispara para practicar el desvío.
- [x] **F4.18** Revisión independiente del código (un subagente, solo lectura): 9 problemas encontrados y corregidos, con 2 tests nuevos (33 en total):
  - Un guerrero caído al Mar Digital ya no hace aparecer al siguiente dentro del mar.
  - El fijado ya guía los disparos (antes no llegaba a `PlayerCombat`).
  - El ojo y el punto de disparo de los monstruos estaban en la espalda; ahora están delante.
  - La carga no sobrevive a un cambio de personaje.
  - El cambio de objetivo funciona igual a cualquier velocidad de fotogramas.
  - El daño por contacto es por golpes con espera.
  - El escudo de Aelita golpea una vez por monstruo.
  - Los monstruos creados en partida tienen punto de disparo.
  - Los monstruos del Bosque están en el suelo y sobre plataformas.
  - Un tajo normal ya no cuenta como acierto en el ojo (solo los golpes fuertes).
- [x] **F4.17** (Lo tiene que hacer una persona) Luchar en la CombatArena con los tres guerreros: combo de Ulrich, ataques cargados, esquiva, desvío (`C` / LT justo antes del láser), fijado y cambio de objetivo. Anotar qué se siente mal. — Probado por el usuario en Unity (octubre de 2026).

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `DamageInfo`, `Hitbox`, `Hurtbox` | Sistema de daño |
| `ComboDefinition`, `SpecialMeter`, `HitStop` | Combos, energía y sensación de golpe |

---

[← F03](F03_movimiento_camara.md) · [Índice](00_INDICE.md) · [F05 →](F05_habilidades.md)
