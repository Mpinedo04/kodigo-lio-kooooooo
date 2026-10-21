# F08 — Vehículos

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 2 semanas (paralela a 5–7)
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F07](F07_aelita_torres_mision.md) · [F09 →](F09_arte_vertical_slice.md)

**Objetivo:** Overboard, Overbike y Overwing divertidos de pilotar.

**Requisitos:** Fase 3. Se puede hacer en paralelo con las Fases 5–7.

**Tareas**

- [ ] **F8.1** Separar `VehicleRig.cs` en `VehicleController` (física común) + `VehicleDefinition` (valores por vehículo).
- [ ] **F8.2** **Overboard (Odd):** vuelo libre en 3D, giro cerrado, piruetas (botón de truco que llena energía), Odd puede disparar montado.
- [ ] **F8.3** **Overbike (Ulrich):** pegada al suelo, muy rápida, salto con el turbo, embestida que hace daño, derrape.
- [ ] **F8.4** **Overwing (Yumi):** vuelo estable, admite **pasajero** (Aelita u otro), el pasajero puede disparar.
- [ ] **F8.5** Montar y desmontar con animación corta; aparece al lado del personaje (luego lo pedirá el Operador).
- [ ] **F8.6** Vida del vehículo: al llegar a 0 explota y el piloto cae.
- [ ] **F8.7** Cámara de vehículo (F3.13) y efecto de velocidad (líneas, FOV).
- [ ] **F8.8** Pista de pruebas `Assets/Scenes/Test/VehicleTrack.unity` (servirá para el modo Carreras).

**Terminado cuando:** se puede cruzar el Bosque en los tres vehículos, con Aelita de pasajera en el Overwing, y luchar montado.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `VehicleController` | Física común de vehículos |

---

[← F07](F07_aelita_torres_mision.md) · [Índice](00_INDICE.md) · [F09 →](F09_arte_vertical_slice.md)
