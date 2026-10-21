# Fases de desarrollo — Índice

Plan para llevar el prototipo al juego completo de [`BIBLIA_DEL_JUEGO.md`](../BIBLIA_DEL_JUEGO.md). Hay **un archivo por fase**: lee solo la fase en la que estés trabajando. Las reglas comunes están en [`00_REGLAS.md`](00_REGLAS.md).

**Fase activa:** F04 — solo falta probar el combate en la `CombatArena` (F4.17). También queda la prueba de la F03 en el `MovementGym` (F3.17). Después, F05

**Estados:** `Pendiente` · `En curso` · `Hecha`. Al cambiar el estado de una fase, actualiza esta tabla y la línea **Fase activa**.

| Fase | Nombre | Bloque | Depende de | Estimación | Estado |
| --- | --- | --- | --- | --- | --- |
| [F00](F00_entorno.md) | Preparar el entorno | A | — | 1 sem | Hecha |
| [F01](F01_recuperar_prototipo.md) | Recuperar y estabilizar el prototipo | A | F00 | 1–2 sem | Hecha |
| [F02](F02_arquitectura.md) | Arquitectura base | A | F01 | 3 sem | Hecha |
| [F03](F03_movimiento_camara.md) | Movimiento y cámara | A | F02 | 2 sem | En curso |
| [F04](F04_combate_base.md) | Combate base | B | F03 | 3 sem | En curso |
| [F05](F05_habilidades.md) | Habilidades de los personajes | B | F04 | 4 sem | Pendiente |
| [F06](F06_enemigos_t1.md) | Enemigos de la Temporada 1 | B | F04 | 4 sem | Pendiente |
| [F07](F07_aelita_torres_mision.md) | Aelita, torres y misión | B | F05, F06 | 2 sem | Pendiente |
| [F08](F08_vehiculos.md) | Vehículos | B | F03 | 2 sem (paralela a F05–F07) | Pendiente |
| [F09](F09_arte_vertical_slice.md) | Arte de la vertical slice | C | F03 | 6–8 sem (paralela a F04–F08) | Pendiente |
| [F10](F10_hud_menus_ui.md) | HUD, menús y UI | C | F04 | 2 sem | Pendiente |
| [F11](F11_audio.md) | Audio | C | F04 | 1–2 sem | Pendiente |
| [F12](F12_vertical_slice_terminada.md) | **Vertical slice terminada** (demo) | C | F05–F11 | 1 sem | Pendiente |
| [F13](F13_modo_operador.md) | Modo Operador (Jeremie) | D | F12 | 3 sem | Pendiente |
| [F14](F14_mundo_real_fabrica.md) | Mundo real: la fábrica | D | F12 | 3 sem | Pendiente |
| [F15](F15_mundo_real_kadic.md) | Mundo real: Kadic | D | F14 | 5 sem | Pendiente |
| [F16](F16_misiones_divididas.md) | Misiones divididas y ataques de X.A.N.A. | D | F13, F15 | 3 sem | Pendiente |
| [F17](F17_historia_dialogos_guardado.md) | Historia: diálogos, cinemáticas y guardado | D | F12 | 3 sem | Pendiente |
| [F18](F18_progresion_retorno.md) | Progresión y Retorno al pasado | D | F13, F17 | 2 sem | Pendiente |
| [F19](F19_prologo_capitulo1.md) | **Prólogo y Capítulo 1** (versión 1.0) | E | F13–F18 | 10 sem | Pendiente |
| [F20](F20_capitulo2_sector5.md) | Capítulo 2: Sector 5 | E | F19 | 10 sem | Pendiente |
| [F21](F21_capitulo3_destruccion.md) | Capítulo 3: Destrucción de Lyoko | E | F20 | 8 sem | Pendiente |
| [F22](F22_capitulo4_mar_digital.md) | Capítulo 4: Mar Digital | E | F21 | 12 sem | Pendiente |
| [F23](F23_capitulo5_evolution.md) | Capítulo 5: Evolution | E | F22 | 6 sem | Pendiente |
| [F24](F24_modos_extra.md) | Modos extra | F | F19 | 4 sem | Pendiente |
| [F25](F25_pulido_optimizacion.md) | Pulido, optimización y pruebas | F | F23 | 6 sem | Pendiente |
| [F26](F26_build_final.md) | Build final | F | F25 | 1 sem | Pendiente |

**Bloques:** A Base técnica · B Jugabilidad de Lyoko · C Presentación · D Sistemas de la serie · E Campaña · F Cierre.

**Total aproximado:** 2,5–3 años a ~10 h/semana. **Puntos de parada con algo jugable:**

- Tras **F12**: una misión pulida (demo).
- Tras **F19**: la Temporada 1 completa (versión 1.0).
- Cada capítulo siguiente es una ampliación.

**Anexos:** [estructura de carpetas](anexos/carpetas.md) · [assets por fase](anexos/assets.md). Los scripts nuevos de cada fase están al final de su propio archivo.

## Primer paso

Empezar por **F00** y **F01**: instalar Unity 2022.3.56f1, abrir el proyecto y seguir [`UNITY_FIRST_OPEN_CHECKLIST.md`](../UNITY_FIRST_OPEN_CHECKLIST.md). Si sale un error rojo en la consola, copiarlo entero y arreglarlo antes de seguir.
