# F05 — Habilidades de los personajes

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F04](F04_combate_base.md) · **Estimación:** 4 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F04](F04_combate_base.md) · [F06 →](F06_enemigos_t1.md)

**Objetivo:** cada guerrero tiene su kit completo según la Biblia (sección 5).

**Requisitos:** Fase 4.

**Tareas — sistema común**

- [x] **F5.1** Crear `AbilityController.cs`: gestiona las ranuras Habilidad 1, Habilidad 2 y Definitiva, sus tiempos de espera y costes. — Hecho en `Abilities/AbilityController.cs`: desbloqueo por capítulo (sin capítulo = todo desbloqueado), costes (energía especial, Concentración, vuelo) y lectura de `Q`/`F`/`X`. En mando, LB+RB gana a LB o RB sueltos.
- [x] **F5.2** Crear la clase base `Ability` (con `CanUse()`, `Activate()`, `Tick()`, `End()`) y que cada habilidad sea una subclase configurada por un `AbilityDefinition`. — Hecho: `Abilities/Ability.cs`. `AbilityFactory` elige la subclase por `abilityId`; los valores están en cada `AbilityDefinition` (daño, radio, alcance, velocidad, número, vida, coste por segundo, cámara lenta).
- [x] **F5.3** Iconos y tiempo de espera visibles en el HUD (provisional). — Provisional en `GameHud`: estado de cada ranura (lista, segundos que faltan, bloqueada, sin energía), Concentración, vuelo, escudo y mensajes. Los iconos de verdad llegan en F10.

**Tareas — Odd** (`Abilities/Odd/`)

- [x] **F5.4** Ráfaga de flechas láser (básico) + flecha cargada perforante (especial). — Ráfaga de 2 flechas (`basicBurst`); el especial es una flecha cargada que perfora y aturde 1,2 s (`specialStagger`).
- [x] **F5.5** **Escudo**: bloquea láseres frontales, tiene su propia vida, se rompe y se recarga. — 60 puntos y 4 s; para láseres y disparos frontales. Si se rompe, se recarga en 8 s; si lo bajas antes, en menos.
- [x] **F5.6** **Visión del futuro**: cámara lenta 3 s y líneas que muestran por dónde irán los disparos enemigos. — Cámara lenta (`SlowMotion`, compatible con el hit-stop) y líneas rojas por donde irán los disparos enemigos, más la dirección en la que apunta cada monstruo.
- [x] **F5.7** Pasivas: doble salto y trepar paredes. — Ya estaban desde F03 (rasgos de `CharacterDefinition`).
- [x] **F5.8** Definitiva **Lluvia felina**. — Salto alto y 18 flechas que caen en un círculo de 6 m alrededor del objetivo fijado (o delante).

**Tareas — Ulrich** (`Abilities/Ulrich/`)

- [x] **F5.9** Combo de katana de 3 golpes + tajo cargado que rompe escudos. — El combo de 3 golpes es de F04. El especial y el ataque cargado son golpes pesados (`isHeavy`): la F06 hace que rompan guardias.
- [x] **F5.10** **Supersprint**: carrera muy rápida que atraviesa disparos y golpea al chocar. — Carrera de 0,45 s a 28 m/s, invulnerable, que golpea una vez a cada monstruo con el que choca.
- [x] **F5.11** **Triplicado**: dos clones con IA que atraen a los enemigos y hacen poco daño; desaparecen al recibir un golpe. — Clones de 1 PV con IA sencilla (`UlrichClone`): persiguen al monstruo más cercano y hacen 5 de daño.
- [x] **F5.12** **Triangular**: si se activa Supersprint con los clones activos, los tres rodean al objetivo y el golpe del Ulrich real es crítico. — Con clones fuera, el Supersprint los coloca en triángulo alrededor del objetivo y el golpe del Ulrich real es crítico (×3).
- [x] **F5.13** Desvío de láseres con la katana (usando F4.12). — Habilidad pasiva nueva `ulrich_katana_deflect`: un láser desviado con la guardia vuelve teledirigido contra quien lo disparó.
- [x] **F5.14** Definitiva **Impacto** (bloqueada hasta el Capítulo 4). — Golpe en área de 6 m que empuja y aturde. Bloqueada hasta el Capítulo 4, salvo sin capítulo o con «Desbloquear todas las habilidades» del menú `F1`.

**Tareas — Yumi** (`Abilities/Yumi/`)

- [x] **F5.15** Abanico bumerán: trayectoria de ida y vuelta, golpea en ambos sentidos. — `BoomerangFan`: va, vuelve a la mano y golpea en los dos trayectos. Yumi tiene 2 abanicos.
- [x] **F5.16** Doble lanzamiento en abanico ancho. — El especial lanza los dos abanicos en arco ancho; hace falta tener los dos en la mano.
- [x] **F5.17** **Barra de Concentración** (recurso de la telequinesis). — `ResourcePool` en `AbilityController`. Si se agota mientras levanta algo, Yumi queda aturdida 2 s.
- [x] **F5.18** **Telequinesis: agarrar**: elegir objeto/enemigo pequeño, levantarlo y lanzarlo hacia donde apunta la cámara. — Levanta rocas (`TelekinesisTarget`) o Kankrelats/Hornets. Clic = lanzar hacia el centro de la pantalla; `Q` otra vez = soltar suave.
- [x] **F5.19** **Telequinesis: muro**: barrera temporal de terreno que para los láseres. — Muro de terreno en la capa `Environment` (para los láseres) con `NavMeshObstacle` (corta el paso a los monstruos).
- [x] **F5.20** Objetos de escenario “movibles” (`TelekinesisTarget.cs`) para puzles. — `TelekinesisTarget.cs` con el evento `Released` para los puzles. En la `CombatArena` hay 3 rocas.
- [x] **F5.21** Definitiva **Danza de abanicos**. — Torbellino de 2,5 s que atrae a los monstruos y termina con una explosión que los empuja.

**Tareas — Aelita** (preparar ya, se usa en la Fase 7 y es jugable desde el Capítulo 3)

- [x] **F5.22** Campo de energía (proyectil teledirigido) + versión cargada en área. — Estilo `Support`: campo de energía lento y teledirigido; el especial y el cargado explotan en área (radio 4).
- [x] **F5.23** **Creatividad: puente** y **Creatividad: muro**, solo en zonas marcadas (`CreativityAnchor.cs`) para controlar el diseño de niveles. — `CreativityAbility` + `CreativityAnchor`. La `CombatArena` tiene una zona de puente (rampa entre dos bloques) y una de muro frente al Blok.
- [x] **F5.24** **Alas**: planear y vuelo con energía limitada. — Mantener Saltar al caer = planear; Saltar en el aire = aletazo. Gasta energía de vuelo, que se recarga en el suelo.

**Tareas — sinergias**

- [x] **F5.25** Sistema de **combos de equipo** (`TeamSynergy.cs`): detecta si dos personajes están cerca y si se cumple la condición (ej. enemigo levantado por Yumi + ataque de Ulrich). — `Squad/TeamSynergy.cs`: cercanía entre guerreros y condiciones. Avisa en el HUD y en el log (`[Synergy]`).
- [x] **F5.26** Implementar las 4 sinergias de la Biblia (13.2). — Yumi + Ulrich (golpe a un monstruo levantado: +150 %), Odd + Aelita (escudo de Odd cerca: campo de energía doble), Yumi + Aelita (Creatividad gigante) y Ulrich + William (dos golpes pesados seguidos rompen la armadura). Esta última no se puede probar hasta que William sea jugable.

**Tareas — compañeros con IA**

- [x] **F5.27** IA de compañero (`CompanionAI.cs`): seguir al jugador, atacar al enemigo más cercano, usar habilidades con tiempo de espera, no tirarse al Mar Digital. — `Squad/CompanionAI.cs`: sigue al guerrero controlado, ataca al monstruo más cercano, usa habilidades de vez en cuando, nunca da un paso sin suelo y reaparece al lado si se queda lejos o atascado. Valores en `Data/SquadProfile`.
- [x] **F5.28** Cambiar de personaje deja al anterior con IA (no quieto). — `CharacterSwitcher` ya no teletransporta ni desactiva a nadie: la cámara salta al otro guerrero y el anterior sigue con IA.

**Tareas — prueba**

- [x] **F5.29** Probar en la `CombatArena`: las habilidades de los 4 guerreros (Aelita es la tecla `4`), la IA de los compañeros y al menos 2 sinergias (Yumi + Ulrich y Odd + Aelita). — Probado por el usuario en Unity (octubre de 2026). La sinergia Ulrich + William no se puede probar hasta que William sea jugable.

**Terminado cuando:** en el `CombatArena` los 3 guerreros usan todas sus habilidades, la IA de los compañeros ayuda y al menos 2 sinergias funcionan.

**Riesgos:** la telequinesis y los clones son las habilidades más complicadas. Si se atascan, hacer primero una versión simple (agarrar solo rocas; clones que solo se quedan quietos) y mejorarla luego.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `AbilityController`, `Ability` (+ una subclase por habilidad) | Habilidades |
| `TelekinesisTarget`, `CreativityAnchor` | Objetos del escenario para Yumi y Aelita |
| `TeamSynergy`, `CompanionAI` | Combos de equipo e IA de compañeros |
| `AbilityFactory`, `ResourcePool`, `AbilityMath`, `AbilityVisuals`, `TemporaryStructure`, `ShockwaveEffect` | Piezas comunes de las habilidades |
| `UlrichClone`, `BoomerangFan`, `ThrownBody`, `TelekinesisState`, `TrajectoryLines`, `ShieldMath` | Piezas de las habilidades de cada guerrero |
| `Combat/SlowMotion`, `Combat/IDamageInterceptor`, `Combat/HitReactions`, `Data/SquadProfile` | Cámara lenta, bloqueo de golpes, aturdir con `DamageInfo.stagger`, ajustes de IA y sinergias |

---

[← F04](F04_combate_base.md) · [Índice](00_INDICE.md) · [F06 →](F06_enemigos_t1.md)
