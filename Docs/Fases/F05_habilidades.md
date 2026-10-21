# F05 — Habilidades de los personajes

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 4 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F04](F04_combate_base.md) · [F06 →](F06_enemigos_t1.md)

**Objetivo:** cada guerrero tiene su kit completo según la Biblia (sección 5).

**Requisitos:** Fase 4.

**Tareas — sistema común**

- [ ] **F5.1** Crear `AbilityController.cs`: gestiona las ranuras Habilidad 1, Habilidad 2 y Definitiva, sus tiempos de espera y costes.
- [ ] **F5.2** Crear la clase base `Ability` (con `CanUse()`, `Activate()`, `Tick()`, `End()`) y que cada habilidad sea una subclase configurada por un `AbilityDefinition`.
- [ ] **F5.3** Iconos y tiempo de espera visibles en el HUD (provisional).

**Tareas — Odd** (`Abilities/Odd/`)

- [ ] **F5.4** Ráfaga de flechas láser (básico) + flecha cargada perforante (especial).
- [ ] **F5.5** **Escudo**: bloquea láseres frontales, tiene su propia vida, se rompe y se recarga.
- [ ] **F5.6** **Visión del futuro**: cámara lenta 3 s y líneas que muestran por dónde irán los disparos enemigos.
- [ ] **F5.7** Pasivas: doble salto y trepar paredes.
- [ ] **F5.8** Definitiva **Lluvia felina**.

**Tareas — Ulrich** (`Abilities/Ulrich/`)

- [ ] **F5.9** Combo de katana de 3 golpes + tajo cargado que rompe escudos.
- [ ] **F5.10** **Supersprint**: carrera muy rápida que atraviesa disparos y golpea al chocar.
- [ ] **F5.11** **Triplicado**: dos clones con IA que atraen a los enemigos y hacen poco daño; desaparecen al recibir un golpe.
- [ ] **F5.12** **Triangular**: si se activa Supersprint con los clones activos, los tres rodean al objetivo y el golpe del Ulrich real es crítico.
- [ ] **F5.13** Desvío de láseres con la katana (usando F4.12).
- [ ] **F5.14** Definitiva **Impacto** (bloqueada hasta el Capítulo 4).

**Tareas — Yumi** (`Abilities/Yumi/`)

- [ ] **F5.15** Abanico bumerán: trayectoria de ida y vuelta, golpea en ambos sentidos.
- [ ] **F5.16** Doble lanzamiento en abanico ancho.
- [ ] **F5.17** **Barra de Concentración** (recurso de la telequinesis).
- [ ] **F5.18** **Telequinesis: agarrar**: elegir objeto/enemigo pequeño, levantarlo y lanzarlo hacia donde apunta la cámara.
- [ ] **F5.19** **Telequinesis: muro**: barrera temporal de terreno que para los láseres.
- [ ] **F5.20** Objetos de escenario “movibles” (`TelekinesisTarget.cs`) para puzles.
- [ ] **F5.21** Definitiva **Danza de abanicos**.

**Tareas — Aelita** (preparar ya, se usa en la Fase 7 y es jugable desde el Capítulo 3)

- [ ] **F5.22** Campo de energía (proyectil teledirigido) + versión cargada en área.
- [ ] **F5.23** **Creatividad: puente** y **Creatividad: muro**, solo en zonas marcadas (`CreativityAnchor.cs`) para controlar el diseño de niveles.
- [ ] **F5.24** **Alas**: planear y vuelo con energía limitada.

**Tareas — sinergias**

- [ ] **F5.25** Sistema de **combos de equipo** (`TeamSynergy.cs`): detecta si dos personajes están cerca y si se cumple la condición (ej. enemigo levantado por Yumi + ataque de Ulrich).
- [ ] **F5.26** Implementar las 4 sinergias de la Biblia (13.2).

**Tareas — compañeros con IA**

- [ ] **F5.27** IA de compañero (`CompanionAI.cs`): seguir al jugador, atacar al enemigo más cercano, usar habilidades con tiempo de espera, no tirarse al Mar Digital.
- [ ] **F5.28** Cambiar de personaje deja al anterior con IA (no quieto).

**Terminado cuando:** en el `CombatArena` los 3 guerreros usan todas sus habilidades, la IA de los compañeros ayuda y al menos 2 sinergias funcionan.

**Riesgos:** la telequinesis y los clones son las habilidades más complicadas. Si se atascan, hacer primero una versión simple (agarrar solo rocas; clones que solo se quedan quietos) y mejorarla luego.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `AbilityController`, `Ability` (+ una subclase por habilidad) | Habilidades |
| `TelekinesisTarget`, `CreativityAnchor` | Objetos del escenario para Yumi y Aelita |
| `TeamSynergy`, `CompanionAI` | Combos de equipo e IA de compañeros |

---

[← F04](F04_combate_base.md) · [Índice](00_INDICE.md) · [F06 →](F06_enemigos_t1.md)
