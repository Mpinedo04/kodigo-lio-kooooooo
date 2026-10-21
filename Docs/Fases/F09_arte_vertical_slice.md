# F09 — Arte de la vertical slice

> **Bloque C** — Presentación · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 6–8 semanas (paralela a 4–8)
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F08](F08_vehiculos.md) · [F10 →](F10_hud_menus_ui.md)

**Objetivo:** cambiar las cápsulas y cubos por modelos, animaciones y efectos con el estilo de Lyoko. Sigue `Docs/MODELING_PIPELINE.md`.

**Requisitos:** Fase 3. **Se trabaja en paralelo** con las Fases 4–8 (el arte no bloquea la jugabilidad).

### 9A — Estilo visual

- [ ] **F9.1** **Shader toon** (cel-shading) en URP: 2–3 tonos de luz, borde, contorno opcional. Probar en un personaje y un monstruo.
- [ ] **F9.2** Shader del **Mar Digital** (animado, con brillo de datos).
- [ ] **F9.3** Cielo blanco/brumoso de Lyoko y niebla por sector.
- [ ] **F9.4** Postprocesado (URP Volume): bloom suave, corrección de color por sector.
- [ ] **F9.5** Guía de color por sector (Bosque verde, Desierto naranja, Hielo azul claro, Montaña marrón/morado, Sector 5 azul/blanco).

### 9B — Personajes (por orden)

Para cada uno: **boceto → malla de bloqueo → malla final → UV → texturas/colores → rig → animaciones → prefab**.

- [ ] **F9.6** **Odd** (primero, porque arranca como personaje activo).
- [ ] **F9.7** **Ulrich**.
- [ ] **F9.8** **Yumi**.
- [ ] **F9.9** **Aelita**.
- [ ] **F9.10** Rig *Humanoid* común para los 4 → las animaciones se pueden compartir.
- [ ] **F9.11** **Animaciones mínimas** de cada uno: reposo, andar, correr, saltar (subida/caída/aterrizaje), esquivar, ataque básico (combo), especial, cada habilidad, recibir golpe, devirtualizarse, montar/pilotar.
  - Opción rápida: animaciones de **Mixamo** (gratis) adaptadas y luego retocadas.
- [ ] **F9.12** `Animator Controller` por personaje con *Blend Tree* de movimiento y capas para ataques.
- [ ] **F9.13** Registrar el origen y licencia de todo modelo o animación externos en `Assets/ArtSources/ASSET_SOURCES.md`.

### 9C — Monstruos

- [ ] **F9.14** Kankrelat → Hornet → Blok → Krabe → Megatank (de más simple a más complejo).
- [ ] **F9.15** El **ojo de X.A.N.A.** como material emisivo aparte en cada monstruo (para que brille al cargar ataque).
- [ ] **F9.16** Animaciones: reposo, mover, atacar, recibir golpe, morir. El Megatank necesita abrir/cerrar.

### 9D — Vehículos y escenario

- [ ] **F9.17** Modelos de Overboard, Overbike y Overwing.
- [ ] **F9.18** **Sector Bosque**: árboles gigantes (troncos/copas modulares), plataformas, caminos, bordes con el Mar Digital, fragmentos de datos flotantes.
- [ ] **F9.19** **Torre**: exterior, halo animado e interior.
- [ ] **F9.20** Kit modular (piezas que encajan) para montar sectores rápido más adelante.

### 9E — Efectos (VFX Graph o Particle System)

- [ ] **F9.21** Virtualización (aparición del personaje).
- [ ] **F9.22** **Devirtualización** (el avatar se deshace en polígonos).
- [ ] **F9.23** Láser de monstruo, flecha de Odd, estela de la katana, abanico, campo de energía, escudo, telequinesis.
- [ ] **F9.24** Explosión/devirtualización de monstruos.
- [ ] **F9.25** Halo de la torre y efecto de desactivación.

**Terminado cuando:** la misión del Bosque no tiene ni una cápsula ni un cubo provisional, y una captura de pantalla “parece” Code Lyoko.

**Riesgos:** es la fase que más tiempo come. Si se atasca, priorizar **siluetas reconocibles con colores planos** antes que detalle. Un buen shader toon tapa mucho.

**Assets de esta fase**

| Modelos | Animaciones | Efectos | Audio |
| --- | --- | --- | --- |
| Odd, Ulrich, Yumi, Aelita (Lyoko); Kankrelat, Hornet, Blok, Krabe, Megatank; 3 vehículos; kit del Bosque; torre | Set básico de los 4 + monstruos | Virtualización, devirtualización, ataques, halo | — |

---

[← F08](F08_vehiculos.md) · [Índice](00_INDICE.md) · [F10 →](F10_hud_menus_ui.md)
