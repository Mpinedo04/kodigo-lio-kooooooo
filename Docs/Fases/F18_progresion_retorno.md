# F18 — Progresión y Retorno al pasado

> **Bloque D** — Sistemas de la serie · **Depende de:** [F13](F13_modo_operador.md), [F17](F17_historia_dialogos_guardado.md) · **Estimación:** 2 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F17](F17_historia_dialogos_guardado.md) · [F19 →](F19_prologo_capitulo1.md)

**Objetivo:** las recompensas y el sistema de “X.A.N.A. cada vez más fuerte” (Biblia 13.4 y 14).

**Requisitos:** Fases 13 y 17.

**Tareas**

- [ ] **F18.1** Moneda **Datos**: se ganan al devirtualizar monstruos y completar misiones.
- [ ] **F18.2** **Mejoras** en la fábrica: árbol pequeño por personaje (daño, vida del escudo, duración del Supersprint, coste de la telequinesis…), definido en assets.
- [ ] **F18.3** **Nivel de X.A.N.A.**: sube con cada Retorno al pasado; los monstruos ganan vida y aparecen más (multiplicadores en `DifficultyProfile`).
- [ ] **F18.4** Al terminar una misión: si el ataque causó daños en la Tierra → **Retorno al pasado obligatorio** (+1 Nivel de X.A.N.A.); si no → bonificación de Datos.
- [ ] **F18.5** **Retorno de emergencia** tras fallar: repetir la misión con +2 Nivel de X.A.N.A.
- [ ] **F18.6** Desbloqueo de habilidades por historia (el Escudo de Odd en el Cap. 2, etc.).
- [ ] **F18.7** **Enciclopedia** (archivos de X.A.N.A., diario de Franz Hopper, recuerdos de Aelita) que se rellena con los coleccionables.
- [ ] **F18.8** Trajes desbloqueables (solo visuales).

**Terminado cuando:** completar misiones da Datos, las mejoras se notan en el juego y el Nivel de X.A.N.A. cambia la dificultad.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `DataCurrency`, `UpgradeTree`, `XanaLevel`, `DifficultyProfile`, `Encyclopedia` | Progresión |

---

[← F17](F17_historia_dialogos_guardado.md) · [Índice](00_INDICE.md) · [F19 →](F19_prologo_capitulo1.md)
