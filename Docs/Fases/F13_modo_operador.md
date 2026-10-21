# F13 — Modo Operador (Jeremie)

> **Bloque D** — Sistemas de la serie · **Depende de:** [F12](F12_vertical_slice_terminada.md) · **Estimación:** 3 semanas
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [← F12](F12_vertical_slice_terminada.md) · [F14 →](F14_mundo_real_fabrica.md)

**Objetivo:** la interfaz del superordenador y su influencia en Lyoko (Biblia 5.6).

**Requisitos:** Fase 12.

**Tareas**

- [ ] **F13.1** Diseño de la interfaz del superordenador (ventanas con estilo de la serie: mapa del sector, fichas de los guerreros, consola de comandos).
- [ ] **F13.2** **Abrir/cerrar** el Operador durante una misión: el juego se ralentiza (no se para del todo) para que haya tensión.
- [ ] **F13.3** **Vista de PV** de todos y alertas.
- [ ] **F13.4** **Escanear** el sector → marca la torre activada y los grupos de monstruos.
- [ ] **F13.5** **Materializar vehículo** junto a un guerrero (con tiempo de espera y coste de energía del superordenador).
- [ ] **F13.6** **Virtualización**: elegir qué personaje entra y en qué punto del sector aparece.
- [ ] **F13.7** **Materialización de emergencia**: sacar a un guerrero de Lyoko (para mandarlo a la Tierra en las misiones divididas).
- [ ] **F13.8** Recurso **energía del superordenador** que limita cuántas acciones se pueden hacer.
- [ ] **F13.9** Minijuegos: teclear código contra reloj, encontrar frecuencia (alinear ondas), redirigir energía (conectar nodos). Cada uno como prefab reutilizable.
- [ ] **F13.10** Modo **automático**: si el jugador no quiere usar el Operador, la IA de Jeremie hace lo básico (con peores tiempos).
- [ ] **F13.11** Comandos del Operador como `OperatorCommandDefinition` (asset) para añadir programas nuevos sin tocar el código (contraantivirus, Skidbladnir, multiagente…).

**Terminado cuando:** en la misión del Bosque se puede escanear, materializar vehículos y resolver un minijuego que abre un camino.

**Scripts nuevos de esta fase**

| Script | Para qué |
| --- | --- |
| `OperatorConsole`, `OperatorCommandDefinition`, minijuegos | Modo Operador |

---

[← F12](F12_vertical_slice_terminada.md) · [Índice](00_INDICE.md) · [F14 →](F14_mundo_real_fabrica.md)
