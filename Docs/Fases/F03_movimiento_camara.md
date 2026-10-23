# F03 — Movimiento y cámara

> **Bloque A** — Base técnica · **Depende de:** [F02](F02_arquitectura.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F02](F02_arquitectura.md) · [F04 →](F04_combate_base.md)

**Objetivo:** que moverse por Lyoko **se sienta bien** (es lo que más se nota al jugar).

**Requisitos:** Fase 2.

**Tareas — movimiento**

- [x] **F3.1** Aceleración y frenado suaves (no ir de 0 a máxima velocidad de golpe).
- [x] **F3.2** Giro del personaje suave hacia la dirección de movimiento.
- [x] **F3.3** Salto con **altura variable** (soltar el botón antes = salto más bajo), **coyote time** (0,1 s para saltar justo después de salir de un borde) y **buffer de salto** (pulsar un poco antes de tocar el suelo también cuenta).
- [x] **F3.4** Gravedad más fuerte al caer que al subir (salto menos “flotante”).
- [x] **F3.5** **Esquiva** con 0,2 s de invulnerabilidad y 0,5 s de espera.
- [x] **F3.6** Correr (`Sprint`) con aumento de campo de visión de la cámara.
- [x] **F3.7** Detección de borde para no caer sin querer al Mar Digital caminando (opcional: el personaje se para al borde si no se está corriendo).
- [x] **F3.8** Movimiento específico por personaje: doble salto y trepar paredes (Odd), salto largo (Yumi), planear (Aelita, más adelante).
- [x] **F3.9** Plataformas móviles: el personaje se mueve con ellas.
  - Hecho en `PlayerController` con un `MovementProfile` (`Assets/Game/Data/MovementProfile.asset`) que tiene todos los valores ajustables (aceleración, gravedad, coyote time, esquiva, borde...). El salto usa `JumpAssist` y `MovementMath`, que tienen tests. Rasgos por personaje en `CharacterDefinition` (`canDoubleJump`, `canWallClimb`, `canLongJump`, `canGlide`): Odd trepa (salta contra una pared y mantén adelante) y hace doble salto; Yumi hace salto largo (correr + saltar). Los vehículos voladores ya no caen: suben y bajan con `Space` / `Ctrl`.

**Tareas — cámara**

- [x] **F3.10** Añadir el paquete **Cinemachine** y sustituir `ThirdPersonCamera.cs` por una cámara *FreeLook* de Cinemachine.
- [x] **F3.11** La cámara no atraviesa paredes ni el suelo (*Collider* de Cinemachine).
- [x] **F3.12** Cámara de **fijado de objetivo**: con `LockOn` activo, la cámara encuadra al jugador y al enemigo.
- [x] **F3.13** Cámara para vehículos (más lejos y más alta).
- [x] **F3.14** Sacudida de cámara (*Impulse*) al recibir y dar golpes fuertes.
- [x] **F3.15** Opciones: sensibilidad y eje Y invertido.
  - Hecho con Cinemachine 2.9.7: `CameraRig` maneja una FreeLook a pie, otra más lejana en vehículo, una cámara de fijado que encuadra a guerrero y enemigo, el FOV al correr (+8) y la sacudida (al recibir 15 o más de daño y al acertar un especial cuerpo a cuerpo). Las opciones se guardan en `GameSettings` (PlayerPrefs) y de momento se cambian en el menú `F1`; el menú de opciones de verdad llega en F10.

**Tareas — escenario de pruebas**

- [x] **F3.16** Crear la escena `Assets/Scenes/Test/MovementGym.unity` con rampas, escaleras, huecos de distintos tamaños, paredes para trepar y plataformas móviles. Aquí se prueba cualquier cambio de movimiento.
  - Hecho (`MovementGymBuilder`, incluido en *Rebuild Everything*). Contiene rampas de 15°, 30° y 50°, escaleras de 0,2 m y 0,4 m, huecos de 2, 4 y 6 m sobre el Mar Digital, paredes de 3, 6 y 10 m, una plataforma lateral, un ascensor con repisa alta y dos dianas (Bloks que no disparan) para probar el fijado. No va en la build.

**Terminado cuando:** recorrer el `MovementGym` con los 3 personajes resulta cómodo con teclado y mando, sin que la cámara se atasque.

- [x] **F3.17** (Lo tiene que hacer una persona) Recorrer el `MovementGym` con Odd, Ulrich y Yumi, con teclado y con mando, y anotar qué se siente mal. Los valores se ajustan en `MovementProfile.asset` y en los componentes de `CameraRig` sin tocar código. — Probado por el usuario en Unity (octubre de 2026).

**Riesgos:** el `CharacterController` de Unity tiene límites (pendientes, escalones). Si da problemas, valorar un controlador cinemático propio, pero solo si es necesario.

---

[← F02](F02_arquitectura.md) · [Índice](00_INDICE.md) · [F04 →](F04_combate_base.md)
