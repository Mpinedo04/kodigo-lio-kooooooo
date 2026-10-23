# F07 — Aelita, torres y misión

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F05](F05_habilidades.md), [F06](F06_enemigos_t1.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F06](F06_enemigos_t1.md) · [F08 →](F08_vehiculos.md)

**Objetivo:** el núcleo de cada misión: llevar a Aelita a la torre.

**Requisitos:** Fases 5 y 6.

**Tareas**

- [x] **F7.1** Mejorar `AelitaCompanion.cs`: búsqueda de camino con NavMesh, evitar enemigos, esconderse detrás de los aliados. — `AelitaCompanion` sigue el camino del NavMesh cuando el nivel tiene uno (`NavMesh.CalculatePath`) y si no va en línea recta. Se aparta de los monstruos cercanos, se esconde detrás del guerrero más cercano y nunca da un paso sin suelo.
- [x] **F7.2** **Órdenes a Aelita** (tecla `Z`, rueda rápida): *Seguirme / Esperar aquí / Esconderse / Ir a la torre*. — `Squad/AelitaOrders.cs`. Pulsar `Z` (cruceta abajo) pasa a la siguiente orden. Mantenerlo abre la rueda: se elige con el stick o `WASD` (arriba Seguirme, derecha Esperar, abajo Esconderse, izquierda Ir a la torre) y se suelta. Montado no hace nada, porque en el mando ese botón es el de pasajera.
- [x] **F7.3** Los enemigos **la eligen como objetivo** a menudo (el `CombatDirector` le da prioridad). — Aelita lleva `AggroTarget` con prioridad 2 (el guerrero controlado cuenta ×1,5 y los compañeros ×1).
- [x] **F7.4** Indicador de la vida de Aelita siempre visible y aviso cuando está en peligro. — Barra rosa arriba a la derecha con la orden actual y el aviso parpadeante «AELITA EN PELIGRO!», que sale con poca vida, al recibir un golpe o con un monstruo a menos de 5 m.
- [x] **F7.5** Si Aelita se devirtualiza o cae al Mar Digital → misión fallida. — Ya funcionaba: `MissionRunner` falla si Aelita muere y `DigitalSeaHazard` la mata si cae.
- [x] **F7.6** **Torre**: prefab final con halo de color según estado (azul neutral, rojo X.A.N.A., verde Jeremie/Aelita, blanco Franz Hopper). — Prefab `Generated/Prefabs/LyokoTower.prefab` (lo crea `Editor/TowerBuilder`) con `Level/TowerVisual`: halo azul (neutral), rojo (X.A.N.A.), verde (Jeremie/Aelita) o blanco (Franz Hopper).
- [x] **F7.7** **Interior de la torre**: escena pequeña. Aelita entra → flota hasta la plataforma superior → panel → escribe `CODE: LYOKO` → la torre se desactiva. Primero como secuencia jugable sencilla y luego con Timeline (Fase 17). — `Level/TowerInterior.cs`: sala aparte con su propia cámara. Aelita flota hasta arriba y, manteniendo `E`, escribe CODE: LYOKO letra a letra. Mientras tanto los guerreros luchan solos con IA y Aelita está a salvo dentro.
- [x] **F7.8** **Torre de paso**: entrar en una torre neutral y salir en otro sector. — `Level/PassageTower.cs`: una torre neutral al sur de la salida lleva a una isla de prueba y la de la isla devuelve al Bosque. También puede cargar un nivel (`sectorScene`).
- [x] **F7.9** Pasar la misión del Bosque a `MissionDefinition` con estos pasos: *Intro → Llegar al puente → Combate en la meseta → Escoltar a Aelita → Jefe Megatank → Desactivar torre → Final*. — Los 7 pasos ya estaban en `M01_ForestTower` desde F02. Ahora cada paso tiene su punto de control (`checkpointId`).
- [x] **F7.10** Puntos de control (`Checkpoint.cs`) al principio de cada paso. — Puntos de control `start`, `bridge`, `plateau` y `tower`; cada paso activa el suyo al empezar.
- [x] **F7.11** Pantalla de **misión fallida** con *Reintentar desde el último punto de control*. — `Mission/MissionFailScreen.cs`. «Retorno de emergencia» (`R` / Y) revive al equipo y a Aelita en el último punto de control, repite ese paso y sube el Nivel de X.A.N.A.; los monstruos ya vencidos no vuelven. «Reiniciar» recarga la misión.

- [x] **F7.12** Probar en Unity: jugar la misión del Bosque entera dando órdenes a Aelita, entrar en la torre, morir a propósito y usar el Retorno de emergencia. — Probado por el usuario en Unity (octubre de 2026).

**Terminado cuando:** la misión del Bosque se juega completa con Aelita como compañera creíble, órdenes, torre con interior y puntos de control.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `TowerInterior`, `AelitaOrders` | Torres y órdenes a Aelita |
| `Level/TowerVisual`, `Level/PassageTower`, `Squad/TeamTeleport`, `Mission/MissionFailScreen` | Halo de las torres, torres de paso, teletransporte seguro del equipo y pantalla de misión fallida |

---

[← F06](F06_enemigos_t1.md) · [Índice](00_INDICE.md) · [F08 →](F08_vehiculos.md)
