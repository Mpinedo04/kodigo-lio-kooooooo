# F06 — Enemigos de la Temporada 1

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 4 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F05](F05_habilidades.md) · [F07 →](F07_aelita_torres_mision.md)

**Objetivo:** los 5 monstruos de la T1 con IA de verdad, comportamientos distintos y su punto débil.

**Requisitos:** Fase 4 (se puede hacer a la vez que la Fase 5).

**Tareas — sistema común**

- [ ] **F6.1** Dividir `EnemyAI.cs` (hoy 521 líneas) en una **máquina de estados**: `EnemyBrain` + estados `Idle`, `Patrol`, `Alert`, `Chase`, `Attack`, `Stagger`, `Dead`.
- [ ] **F6.2** Usar **NavMesh** (paquete *AI Navigation*) para que los terrestres caminen por el sector sin caerse.
- [ ] **F6.3** Percepción: rango de visión, ángulo, línea de visión y aviso a los cercanos.
- [ ] **F6.4** **Director de combate** (`CombatDirector.cs`): limita cuántos enemigos atacan a la vez (para no ser injusto) y reparte objetivos entre el jugador, los compañeros y Aelita.
- [ ] **F6.5** Aviso visual antes de cada ataque fuerte (el ojo brilla, carga de láser).
- [ ] **F6.6** Devirtualización del monstruo: explota en fragmentos y suelta Datos.
- [ ] **F6.7** Ampliar `WaveSpawner.cs`: oleadas definidas como assets (`WaveDefinition`), aparición con efecto de “materialización”.

**Tareas — por monstruo**

- [ ] **F6.8** **Kankrelat:** grupo de 3–6, se reparten alrededor del objetivo, láser débil, muere de un golpe al ojo.
- [ ] **F6.9** **Blok:** torreta; 4 tipos de disparo (láser, hielo que ralentiza, anillos de fuego, rayo prisión); se gira para no dejar el ojo a la vista.
- [ ] **F6.10** **Krabe:** camina y patea; las patas reciben daño aparte y al romper dos cae al suelo y deja el ojo expuesto.
- [ ] **F6.11** **Hornet:** vuela en círculos, baja para disparar en ráfaga, sube otra vez; huye si está solo.
- [ ] **F6.12** **Megatank:** rueda hacia el jugador, se para, se abre (aviso de 1 s), dispara un rayo horizontal o vertical; solo es vulnerable abierto.
- [ ] **F6.13** Ficha (`EnemyDefinition`) de cada uno con los valores de la Biblia (sección 8.1).

**Tareas — pruebas**

- [ ] **F6.14** Escena `Assets/Scenes/Test/EnemyGym.unity` con botones de depuración para hacer aparecer cada monstruo y grupos mixtos.
- [ ] **F6.15** Equilibrado: anotar en una tabla el tiempo que tarda en morir cada monstruo con cada personaje y ajustar.

**Terminado cuando:** cada monstruo se distingue por cómo se mueve y ataca, y un grupo mixto (3 Kankrelats + Blok + Hornet) es un reto pero se gana.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `EnemyBrain` + estados, `CombatDirector`, `WaveDefinition` | IA de monstruos |

---

[← F05](F05_habilidades.md) · [Índice](00_INDICE.md) · [F07 →](F07_aelita_torres_mision.md)
