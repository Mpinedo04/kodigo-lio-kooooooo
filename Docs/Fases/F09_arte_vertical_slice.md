# F09 — Arte de la vertical slice

> **Bloque C** — Presentación · **Depende de:** [F03](F03_movimiento_camara.md) · **Estimación:** 6–8 semanas (paralela a 4–8)
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F08](F08_vehiculos.md) · [F10 →](F10_hud_menus_ui.md)

**Objetivo:** cambiar las cápsulas y cubos por modelos, animaciones y efectos con el estilo de Lyoko. Sigue `Docs/MODELING_PIPELINE.md`.

**Requisitos:** Fase 3. **Se trabaja en paralelo** con las Fases 4–8 (el arte no bloquea la jugabilidad).

**Cómo se ha hecho:** en este PC no hay Blender ni modelos descargados, así que **todo el arte lo genera el código** (mallas low-poly facetadas, shader toon y colores planos), siguiendo el consejo de *Riesgos*: siluetas reconocibles con colores planos antes que detalle. Los generadores guardan mallas y materiales en `Assets/Game/Generated/Art/` (`ArtAssetPersistence`). El arte hecho a mano (malla final, UV, texturas, rig *Humanoid*, clips) queda como tarea opcional **F25.11**.

### 9A — Estilo visual

- [x] **F9.1** **Shader toon** (cel-shading) en URP: 2–3 tonos de luz, borde, contorno opcional. Probar en un personaje y un monstruo. — `Art/Shaders/LyokoToon.shader`: 2 o 3 tonos, luz de borde, emisión, sombras y contorno (casco invertido con normales suavizadas en UV3). Lo usan guerreros, monstruos, vehículos y escenario (`Visual/LyokoMaterials.cs`).
- [x] **F9.2** Shader del **Mar Digital** (animado, con brillo de datos). — `DigitalSea.shader`: rejilla azul animada con olas y pulsos de datos.
- [x] **F9.3** Cielo blanco/brumoso de Lyoko y niebla por sector. — `LyokoSky.shader` + niebla y luz ambiental por sector.
- [x] **F9.4** Postprocesado (URP Volume): bloom suave, corrección de color por sector. — Volumen global `LyokoPostProcess`: bloom, tonemapping, color y viñeta.
- [x] **F9.5** Guía de color por sector (Bosque verde, Desierto naranja, Hielo azul claro, Montaña marrón/morado, Sector 5 azul/blanco). — Colores de cielo, niebla, suelo, terreno, acento y bloom en cada `SectorDefinition`; los aplica `Editor/LookBuilder.cs`.

### 9B — Personajes (por orden)

Para cada uno: **boceto → malla de bloqueo → malla final → UV → texturas/colores → rig → animaciones → prefab**.

- [x] **F9.6** **Odd** (primero, porque arranca como personaje activo).
- [x] **F9.7** **Ulrich**.
- [x] **F9.8** **Yumi**.
- [x] **F9.9** **Aelita**.
  - F9.6–F9.9: modelo low-poly generado por código (`Visual/WarriorModel.cs`) con la silueta, los colores y el arma de cada uno (orejas de gato y cola de Odd, katana de Ulrich, abanicos de Yumi, orejas de elfo de Aelita). Sin UV ni texturas: el shader toon usa colores planos.
- [x] **F9.10** Rig *Humanoid* común para los 4 → las animaciones se pueden compartir. — Esqueleto común `Visual/CharacterRig.cs` (cadera, columna, pecho, cabeza, brazos y piernas) para los 4. No es un rig *Humanoid* de Unity porque no hay modelos importados (F25.11).
- [x] **F9.11** **Animaciones mínimas** de cada uno: reposo, andar, correr, saltar (subida/caída/aterrizaje), esquivar, ataque básico (combo), especial, cada habilidad, recibir golpe, devirtualizarse, montar/pilotar. — Animación procedural (`Visual/ProceduralAnimator.cs`): todas las de la lista, más el planeo de Aelita. No hay clips.
- [x] **F9.12** `Animator Controller` por personaje con *Blend Tree* de movimiento y capas para ataques. — Sustituido por `ProceduralAnimator`, que mezcla las poses por código. El `Animator Controller` llegará con los clips reales (F25.11).
- [x] **F9.13** Registrar el origen y licencia de todo modelo o animación externos en `Assets/ArtSources/ASSET_SOURCES.md`. — No se ha usado nada externo; anotado en `ASSET_SOURCES.md`.

### 9C — Monstruos

- [x] **F9.14** Kankrelat → Hornet → Blok → Krabe → Megatank (de más simple a más complejo). — Cuerpos facetados con contorno (`Enemies/EnemyRigBuilder.cs`); conservan sus colisionadores.
- [x] **F9.15** El **ojo de X.A.N.A.** como material emisivo aparte en cada monstruo (para que brille al cargar ataque). — `LowPolyMesh.XanaEye()` con su propio material emisivo; brilla al cargar (`EnemyTelegraph`).
- [x] **F9.16** Animaciones: reposo, mover, atacar, recibir golpe, morir. El Megatank necesita abrir/cerrar. — `Visual/MonsterAnimator.cs` (patas, alas, balanceo) más el comportamiento de cada monstruo; el Megatank abre y cierra.

### 9D — Vehículos y escenario

- [x] **F9.17** Modelos de Overboard, Overbike y Overwing. — Piezas facetadas con shader toon y contorno (`Editor/VehiclePrefabBuilder.cs`).
- [x] **F9.18** **Sector Bosque**: árboles gigantes (troncos/copas modulares), plataformas, caminos, bordes con el Mar Digital, fragmentos de datos flotantes. — Árboles que salen del Mar Digital, acantilados bajo cada plataforma, plataformas con el borde biselado, columnas de datos y grupos de datos flotantes (`Editor/SectorArtBuilder.cs`).
- [x] **F9.19** **Torre**: exterior, halo animado e interior. — Columna facetada, halo que gira y late, franjas de luz del color del estado; interior con aros de datos, círculos de luz en el suelo y Aelita con su modelo (`Editor/TowerBuilder.cs`, `Level/TowerVisual.cs`).
- [x] **F9.20** Kit modular (piezas que encajan) para montar sectores rápido más adelante. — `SectorArtBuilder` (`CreateTree`, `AddCliff`, `CreateDataCluster`, `CreateDigitalSea`) y `LowPolyMesh` (`Slab` para plataformas, `Prism`, `Gem`, `Ring`…). Por ahora con piezas del Bosque; cada sector nuevo añadirá las suyas.

### 9E — Efectos (VFX Graph o Particle System)

- [x] **F9.21** Virtualización (aparición del personaje). — `Visual/VirtualizationEffect.cs`: el guerrero se dibuja de los pies a la cabeza mientras bajan aros de luz. Al entrar en Lyoko y tras el Retorno de emergencia.
- [x] **F9.22** **Devirtualización** (el avatar se deshace en polígonos). — `Combat/DevirtualizeEffect.cs`: fragmentos y destello.
- [x] **F9.23** Láser de monstruo, flecha de Odd, estela de la katana, abanico, campo de energía, escudo, telequinesis. — Disparos facetados que brillan con estela de su color, estela de la katana, abanico con estela, campo de energía, disco de luz del escudo de Odd y aura de telequinesis.
- [x] **F9.24** Explosión/devirtualización de monstruos. — `Enemies/EnemyDevirtualize.cs`: fragmentos y destello.
- [x] **F9.25** Halo de la torre y efecto de desactivación. — `TowerVisual`: halo por estado y destello al desactivarse.

**Terminado cuando:** la misión del Bosque no tiene ni una cápsula ni un cubo provisional, y una captura de pantalla “parece” Code Lyoko.

- Comprobado por el test `ArtAuditTests` (ninguna primitiva de Unity en la escena del Bosque ni en los prefabs generados) y con las capturas de **Tools > Code Lyoko Fan Game > Capture Art Preview** (`Editor/ArtPreview.cs`).

**Riesgos:** es la fase que más tiempo come. Si se atasca, priorizar **siluetas reconocibles con colores planos** antes que detalle. Un buen shader toon tapa mucho.

**Assets de esta fase**

| Modelos | Animaciones | Efectos | Audio |
| --- | --- | --- | --- |
| Odd, Ulrich, Yumi, Aelita (Lyoko); Kankrelat, Hornet, Blok, Krabe, Megatank; 3 vehículos; kit del Bosque; torre | Set básico de los 4 + monstruos | Virtualización, devirtualización, ataques, halo | — |

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `Art/Shaders/LyokoToon.shader`, `LyokoGlow.shader`, `DigitalSea.shader`, `LyokoSky.shader` | Cel-shading con contorno, brillo aditivo, Mar Digital y cielo |
| `Visual/LyokoMaterials.cs` | Materiales toon y de brillo compartidos (uno por color) |
| `Visual/MeshBuilder.cs`, `Visual/LowPolyMesh.cs` | Mallas facetadas por código (prismas, gemas, cajas, aros, losas) con caché |
| `Visual/WarriorModel.cs`, `Visual/CharacterRig.cs`, `Visual/ProceduralAnimator.cs` | Modelo, esqueleto y animación de los guerreros |
| `Visual/MonsterAnimator.cs`, `Visual/Spin.cs` | Animación de monstruos y piezas que giran o laten |
| `Visual/VirtualizationEffect.cs` | Efecto de virtualización |
| `Editor/LookBuilder.cs`, `Editor/SectorArtBuilder.cs` | Aspecto de cada sector (cielo, niebla, postprocesado) y piezas del Bosque |
| `Editor/ArtAssetPersistence.cs`, `Editor/ArtPreview.cs` | Guardar mallas y materiales generados; capturas de prueba |

---

[← F08](F08_vehiculos.md) · [Índice](00_INDICE.md) · [F10 →](F10_hud_menus_ui.md)
