# F08 — Vehículos

> **Bloque B** — Jugabilidad de Lyoko · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 2 semanas (paralela a 5–7)
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F07](F07_aelita_torres_mision.md) · [F09 →](F09_arte_vertical_slice.md)

**Objetivo:** Overboard, Overbike y Overwing divertidos de pilotar.

**Requisitos:** Fase 3. Se puede hacer en paralelo con las Fases 5–7.

**Tareas**

- [x] **F8.1** Separar `VehicleRig.cs` en `VehicleController` (física común) + `VehicleDefinition` (valores por vehículo).
- [x] **F8.2** **Overboard (Odd):** vuelo libre en 3D, giro cerrado, piruetas (botón de truco que llena energía), Odd puede disparar montado.
- [x] **F8.3** **Overbike (Ulrich):** pegada al suelo, muy rápida, salto con el turbo, embestida que hace daño, derrape.
- [x] **F8.4** **Overwing (Yumi):** vuelo estable, admite **pasajero** (Aelita u otro), el pasajero puede disparar.
- [x] **F8.5** Montar y desmontar con animación corta; aparece al lado del personaje (luego lo pedirá el Operador).
- [x] **F8.6** Vida del vehículo: al llegar a 0 explota y el piloto cae.
- [x] **F8.7** Cámara de vehículo (F3.13) y efecto de velocidad (líneas, FOV).
- [x] **F8.8** Pista de pruebas `Assets/Scenes/Test/VehicleTrack.unity` (servirá para el modo Carreras).
- [x] **F8.9** Probar los tres vehículos en `VehicleTrack` y en el Bosque, con Aelita de pasajera. — Probado por el usuario en Unity (octubre de 2026).

**Terminado cuando:** se puede cruzar el Bosque en los tres vehículos, con Aelita de pasajera en el Overwing, y luchar montado.

**Cómo funciona (prototipo)**

- El cuerpo del vehículo es el `CharacterController` del guerrero: al montar, la cápsula se alarga hasta el casco y `PlayerController` solo llama a `VehicleController.Drive`.
- Dirección relativa a la cámara, como a pie; la cámara de vehículo se recoloca detrás sola si no mueves el ratón.
- Los golpes al piloto van a la vida del vehículo (un `Hurtbox` en el guerrero apunta al `Health` del vehículo). Al llegar a 0 explota, el piloto sale despedido y el vehículo tarda 6 s en volver.
- Cambiar de guerrero montado: el siguiente aparece en su propio vehículo (si está disponible) y Aelita cambia de asiento o baja al suelo.
- Overboard: subir/bajar también mirando arriba/abajo con la cámara. Trucos: tonel (con izquierda/derecha), mortal, giro 360; en cadena dan más energía.
- Overbike: no vuela; `Espacio` salta (más con turbo), `Ctrl` + girar = derrape con mini turbo al soltar; a más de 12 m/s embiste (daño por `HitResolver`).
- Overwing: `E` sube o baja a Aelita (a menos de 7 m); ella dispara sola al enemigo más cercano.
- Todos los valores están en `Assets/Game/Data/Vehicles/*.asset`, pero los escribe `VehicleDataBuilder`: ajústalos allí y ejecuta *Rebuild Everything*.

**Controles montado (mapa `Vehicle`)**

| Acción | Teclado/ratón | Mando |
| --- | --- | --- |
| Dirigir | `WASD` | Stick izq. |
| Subir / bajar (Overbike: saltar / derrapar) | `Space` / `Ctrl` | A / B |
| Turbo | `Shift` | RT |
| Disparar / especial | Clic izq. / clic der. | X / Y |
| Truco (Overboard; Overbike en el aire) | `C` | LT |
| Subir/bajar pasajera (Overwing) | `E` | Cruceta abajo |
| Bajarse | `V` | Cruceta arriba |

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `Vehicles/VehicleController` | Física común: montar, conducir, volar, trucos, embestida, explosión |
| `Vehicles/VehicleMath`, `Vehicles/TrickTracker`, `Vehicles/RiderHitRedirect` | Lógica pura (con tests en `VehicleTests`) |
| `Vehicles/PassengerSeat` | Asiento de Aelita y su disparo automático |
| `Vehicles/SpeedLines`, `Vehicles/VehicleExplosion` | Efectos provisionales |
| `Vehicles/TargetDummy`, `Vehicles/TrackGate` | Dianas y puertas de la pista (evento `TrackGate.Passed` para el modo Carreras) |
| `Editor/VehicleDataBuilder`, `Editor/VehiclePrefabBuilder`, `Editor/VehicleTrackBuilder` | Datos, prefabs y escena de la pista |

---

[← F07](F07_aelita_torres_mision.md) · [Índice](00_INDICE.md) · [F09 →](F09_arte_vertical_slice.md)
