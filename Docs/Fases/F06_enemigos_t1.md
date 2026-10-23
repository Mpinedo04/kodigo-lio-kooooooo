# F06 — Enemigos de la Temporada 1

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 4 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F05](F05_habilidades.md) · [F07 →](F07_aelita_torres_mision.md)

**Objetivo:** los 5 monstruos de la T1 con IA de verdad, comportamientos distintos y su punto débil.

**Requisitos:** Fase 4 (se puede hacer a la vez que la Fase 5).

**Tareas — sistema común**

- [x] **F6.1** Dividir `EnemyAI.cs` (hoy 521 líneas) en una **máquina de estados**: `EnemyBrain` + estados `Idle`, `Patrol`, `Alert`, `Chase`, `Attack`, `Stagger`, `Dead`.
- [x] **F6.2** Usar **NavMesh** (paquete *AI Navigation*) para que los terrestres caminen por el sector sin caerse.
- [x] **F6.3** Percepción: rango de visión, ángulo, línea de visión y aviso a los cercanos.
- [x] **F6.4** **Director de combate** (`CombatDirector.cs`): limita cuántos enemigos atacan a la vez (para no ser injusto) y reparte objetivos entre el jugador, los compañeros y Aelita.
- [x] **F6.5** Aviso visual antes de cada ataque fuerte (el ojo brilla, carga de láser).
- [x] **F6.6** Devirtualización del monstruo: explota en fragmentos y suelta Datos.
- [x] **F6.7** Ampliar `WaveSpawner.cs`: oleadas definidas como assets (`WaveDefinition`), aparición con efecto de “materialización”.

**Tareas — por monstruo**

- [x] **F6.8** **Kankrelat:** grupo de 3–6, se reparten alrededor del objetivo, láser débil, muere de un golpe al ojo.
- [x] **F6.9** **Blok:** torreta; 4 tipos de disparo (láser, hielo que ralentiza, anillos de fuego, rayo prisión); se gira para no dejar el ojo a la vista.
- [x] **F6.10** **Krabe:** camina y patea; las patas reciben daño aparte y al romper dos cae al suelo y deja el ojo expuesto.
- [x] **F6.11** **Hornet:** vuela en círculos, baja para disparar en ráfaga, sube otra vez; huye si está solo.
- [x] **F6.12** **Megatank:** rueda hacia el jugador, se para, se abre (aviso de 1 s), dispara un rayo horizontal o vertical; solo es vulnerable abierto.
- [x] **F6.13** Ficha (`EnemyDefinition`) de cada uno con los valores de la Biblia (sección 8.1).

**Tareas — pruebas**

- [x] **F6.14** Escena `Assets/Scenes/Test/EnemyGym.unity` con botones de depuración para hacer aparecer cada monstruo y grupos mixtos.
- [ ] **F6.15** Equilibrado: anotar en una tabla el tiempo que tarda en morir cada monstruo con cada personaje y ajustar. *(Tabla provisional abajo; falta el ajuste a mano jugando.)*

**Terminado cuando:** cada monstruo se distingue por cómo se mueve y ataca, y un grupo mixto (3 Kankrelats + Blok + Hornet) es un reto pero se gana.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `EnemyBrain` + estados, `CombatDirector`, `WaveDefinition` | IA de monstruos |
| `Enemies/*Behaviour.cs` | Reglas de cada monstruo (Kankrelat, Blok, Krab, Hornet, Megatank y uno genérico para el resto) |
| `Enemies/EnemyMotor`, `EnemyPerception`, `EnemyTelegraph` | Movimiento (NavMesh o simple), vista y aviso de ataque |
| `Enemies/StatusProjectile`, `FireRingWave`, `BeamAttack` | Disparos de hielo/prisión, anillos de fuego y rayos del Megatank |
| `Enemies/EnemyDevirtualize`, `DataPickup`, `EnemyMaterialize` | Fragmentos y Datos al morir, aparición en columna de luz |
| `Combat/StatusEffects`, `Combat/AggroTarget` | Ralentizar/inmovilizar a cualquiera y prioridad de objetivo (Aelita, clones) |

## Notas de diseño

- **Dónde se ajusta cada cosa:** fichas en `Assets/Game/Data/Enemies/` (PV, daño, ataques con su tiempo de aviso `windup`, visión, reacciones), oleadas en `Assets/Game/Data/Waves/` y reglas de grupo en `Assets/Game/Data/CombatDirectorProfile.asset`. *Rebuild Everything* vuelve a escribir los valores de los 5 monstruos de la T1 (salen de `EnemyDefaults.cs`); el perfil del director se crea una vez y conserva lo ajustado.
- **Director:** como mucho 3 monstruos atacando a la vez, 2 sobre el mismo objetivo y 0,8 s entre ataques al mismo objetivo. Prefiere objetivos cercanos y con más prioridad (`AggroTarget`), da un ×1,5 al guerrero que maneja el jugador y, a partir de 2 monstruos sobre el mismo objetivo, los demás buscan otro (compañeros, Aelita, clones).
- **Golpes pesados** (`DamageInfo.isHeavy`): aturden a Kankrelat, Hornet, Blok y Krab; al Megatank cerrado y al Blok que esconde el ojo les **rompen la guardia** (ojo expuesto 1,2–1,6 s).
- **Monstruos sin proyectil** (los muñecos de fijado del MovementGym) nunca atacan.

## F6.15 — Tiempo hasta matar (provisional, pendiente de ajuste jugando)

Cálculo teórico con los valores actuales: todos los golpes aciertan, ataque básico repetido sin parar.
Odd dispara 18 cada 0,28 s (64 de daño por segundo), Yumi lanza 20 cada 0,28 s (71/s) y Ulrich encadena el combo 24 + 26,4 + 40,8 cada 0,69 s (132/s; solo el tercer golpe, que es pesado, puede dar en el ojo). El ojo multiplica ×2,5 y mata de un golpe a Kankrelat y Hornet. El Megatank solo recibe daño abierto (unos 2,6 s de cada 4,1 s cuando ataca sin parar).

| Monstruo (PV) | Odd: cuerpo / ojo | Yumi: cuerpo / ojo | Ulrich: combo |
| --- | --- | --- | --- |
| Kankrelat (20) | 2 disparos, 0,28 s / 1 disparo | 1 disparo / 1 disparo | 1 golpe |
| Hornet (30) | 2 disparos, 0,28 s / 1 disparo | 2 disparos, 0,28 s / 1 disparo | 2 golpes, 0,12 s (solo cuando baja a disparar) |
| Blok (50) | 3 disparos, 0,56 s / 2, 0,28 s | 3 disparos, 0,56 s / 1 disparo | 2 golpes, 0,12 s |
| Krabe (80) | 5 disparos, 1,12 s / 2, 0,28 s | 4 disparos, 0,84 s / 2, 0,28 s | 3 golpes, 0,24 s |
| Krabe: derribarlo (2 patas de 25) | 4 disparos, 0,84 s | 4 disparos, 0,84 s | 4 golpes, 0,6 s |
| Megatank (150, solo abierto) | 9 disparos, 2,24 s abierto (≈1 ciclo) / 4, 0,84 s | 8 disparos, 1,96 s abierto / 3, 0,56 s | 6 golpes, 0,93 s abierto |

**Daño que reciben los guerreros (100 PV):** Kankrelat 10 por disparo cada ~3 s; Hornet ráfaga de 3 × 10 (la invulnerabilidad de 0,5 s tras un golpe deja pasar ~1); Blok 10–15 más ralentizar 2,5 s o inmovilizar 1,5 s; Krabe 20 (láser o patada con empuje); Megatank 40 (horizontal, saltarlo) o 60 (vertical, apartarse). Con el director, un guerrero quieto recibe como mucho un ataque cada 0,8 s.

**Conclusión provisional:** con los PV de la Biblia y el daño actual de los guerreros, todos los monstruos de la T1 caen en menos de un segundo de golpes seguidos (el Megatank, en una apertura). Hay que probarlo jugando: lo más probable es **subir los PV de los monstruos (×2–3) o bajar el daño básico de los guerreros** (`CharacterDefinition.basicDamage`) antes de dar la F6.15 por terminada.

---

[← F05](F05_habilidades.md) · [Índice](00_INDICE.md) · [F07 →](F07_aelita_torres_mision.md)
