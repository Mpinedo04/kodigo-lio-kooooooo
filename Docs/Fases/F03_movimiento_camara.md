# F03 — Movimiento y cámara

> **Bloque A** — Base técnica · **Depende de:** [F02](F02_arquitectura.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F02](F02_arquitectura.md) · [F04 →](F04_combate_base.md)

**Objetivo:** que moverse por Lyoko **se sienta bien** (es lo que más se nota al jugar).

**Requisitos:** Fase 2.

**Tareas — movimiento**

- [ ] **F3.1** Aceleración y frenado suaves (no ir de 0 a máxima velocidad de golpe).
- [ ] **F3.2** Giro del personaje suave hacia la dirección de movimiento.
- [ ] **F3.3** Salto con **altura variable** (soltar el botón antes = salto más bajo), **coyote time** (0,1 s para saltar justo después de salir de un borde) y **buffer de salto** (pulsar un poco antes de tocar el suelo también cuenta).
- [ ] **F3.4** Gravedad más fuerte al caer que al subir (salto menos “flotante”).
- [ ] **F3.5** **Esquiva** con 0,2 s de invulnerabilidad y 0,5 s de espera.
- [ ] **F3.6** Correr (`Sprint`) con aumento de campo de visión de la cámara.
- [ ] **F3.7** Detección de borde para no caer sin querer al Mar Digital caminando (opcional: el personaje se para al borde si no se está corriendo).
- [ ] **F3.8** Movimiento específico por personaje: doble salto y trepar paredes (Odd), salto largo (Yumi), planear (Aelita, más adelante).
- [ ] **F3.9** Plataformas móviles: el personaje se mueve con ellas.

**Tareas — cámara**

- [ ] **F3.10** Añadir el paquete **Cinemachine** y sustituir `ThirdPersonCamera.cs` por una cámara *FreeLook* de Cinemachine.
- [ ] **F3.11** La cámara no atraviesa paredes ni el suelo (*Collider* de Cinemachine).
- [ ] **F3.12** Cámara de **fijado de objetivo**: con `LockOn` activo, la cámara encuadra al jugador y al enemigo.
- [ ] **F3.13** Cámara para vehículos (más lejos y más alta).
- [ ] **F3.14** Sacudida de cámara (*Impulse*) al recibir y dar golpes fuertes.
- [ ] **F3.15** Opciones: sensibilidad y eje Y invertido.

**Tareas — escenario de pruebas**

- [ ] **F3.16** Crear la escena `Assets/Scenes/Test/MovementGym.unity` con rampas, escaleras, huecos de distintos tamaños, paredes para trepar y plataformas móviles. Aquí se prueba cualquier cambio de movimiento.

**Terminado cuando:** recorrer el `MovementGym` con los 3 personajes resulta cómodo con teclado y mando, sin que la cámara se atasque.

**Riesgos:** el `CharacterController` de Unity tiene límites (pendientes, escalones). Si da problemas, valorar un controlador cinemático propio, pero solo si es necesario.

---

[← F02](F02_arquitectura.md) · [Índice](00_INDICE.md) · [F04 →](F04_combate_base.md)
