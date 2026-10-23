# Code Lyoko: Fan Game — Biblia del Juego

> Documento maestro del juego. Recoge **todo** lo que habrá: historia, personajes, habilidades, enemigos, jefes, mapas, vehículos, sistemas y plan de producción.
> Proyecto fan **no comercial**. Code Lyoko pertenece a sus titulares (MoonScoop / Mediatoon). No publicar ni monetizar.

Leyenda usada en todo el documento:

- **[Canon]**: está en la serie tal cual.
- **[Juego]**: adaptación o decisión de diseño nuestra para que funcione como videojuego.
- **[Hecho]**: ya existe un prototipo en el repo. **[Pendiente]**: aún no existe.

---

## Índice

1. [Qué hay ya en el repo](#1-qué-hay-ya-en-el-repo)
2. [Concepto del juego](#2-concepto-del-juego)
3. [Estructura de juego (los 4 modos de juego)](#3-estructura-de-juego)
4. [Historia y campaña completa](#4-historia-y-campaña-completa)
5. [Personajes jugables y sus habilidades](#5-personajes-jugables-y-sus-habilidades)
6. [Personajes secundarios (no combatientes)](#6-personajes-secundarios)
7. [X.A.N.A. y antagonistas](#7-xana-y-antagonistas)
8. [Bestiario: todos los monstruos](#8-bestiario-todos-los-monstruos)
9. [Jefes](#9-jefes)
10. [Ataques de X.A.N.A. en el mundo real](#10-ataques-de-xana-en-el-mundo-real)
11. [Mundos y mapas](#11-mundos-y-mapas)
12. [Vehículos](#12-vehículos)
13. [Sistemas de juego](#13-sistemas-de-juego)
14. [Progresión, coleccionables y desbloqueables](#14-progresión-coleccionables-y-desbloqueables)
15. [Modos extra](#15-modos-extra)
16. [Controles](#16-controles)
17. [Estilo visual y sonido](#17-estilo-visual-y-sonido)
18. [Plan de producción (hitos)](#18-plan-de-producción)

---

## 1. Qué hay ya en el repo

Lo que ya se hizo en su momento (Unity 2022.3 LTS + URP):

| Parte | Estado | Dónde |
| --- | --- | --- |
| Escena del Sector Bosque con plataformas, Mar Digital, puente y torre activada | [Hecho] (se genera desde menú) | `Tools > Code Lyoko Fan Game > Build Vertical Slice Scene` |
| Odd, Ulrich y Yumi jugables, cambio con `1`/`2`/`3` | [Hecho] prototipo | `CharacterSwitcher.cs`, `PlayerController.cs`, `PlayerCombat.cs` |
| Aelita como acompañante con IA que te sigue y desactiva la torre | [Hecho] prototipo | `AelitaCompanion.cs`, `TowerObjective.cs` |
| Kankrelat, Blok, Krabe, Hornet y Megatank con IA, láser, vida y punto débil | [Hecho] prototipo | `EnemyAI.cs`, `WeakPoint.cs`, `WaveSpawner.cs` |
| Combate en tercera persona, fijado de objetivo (`Tab`), devirtualización | [Hecho] prototipo | `LockOnSystem.cs`, `Health.cs`, `Projectile.cs` |
| Vehículos personales (Overboard, Overbike, Overwing) con `V` | [Hecho] prototipo | `VehicleRig.cs` |
| Misión con fases (intro → puente → combate → escolta → jefe → torre) | [Hecho] prototipo | `MissionDirector.cs`, `MissionManager.cs` |
| Fichas de datos editables (personajes, enemigos, vehículos) | [Hecho] | `Assets/Game/Scripts/Data/` |
| Prefabs provisionales (cápsulas/cubos) de todo el reparto | [Hecho] | `FullRosterPlaceholderBuilder.cs` |
| Modelos 3D reales, animaciones, shader estilo Lyoko | [Pendiente] | `Docs/MODELING_PIPELINE.md` |
| Mundo real (Kadic, fábrica), historia, resto de sectores | [Pendiente] | Este documento |

En resumen: hay una **“vertical slice”**, una sola misión jugable con formas provisionales que demuestra el bucle básico. Todo lo demás de este documento es lo que falta construir.

---

## 2. Concepto del juego

**Género:** acción-aventura 3D en tercera persona con partes de exploración y de gestión.

**Fantasía del jugador:** ser la Banda de Lyoko. De día eres estudiante en la Academia Kadic; cuando X.A.N.A. ataca, corres a la fábrica, te virtualizas y luchas en Lyoko mientras Jeremie te guía desde el superordenador.

**Pilares de diseño:**

1. **Dos mundos a la vez.** Lo que pasa en el mundo real afecta a Lyoko y al revés. Mientras unos luchan en Lyoko, otros sobreviven al ataque en la Tierra.
2. **Un equipo, no un héroe.** Cada personaje tiene un rol claro y la clave es cambiar entre ellos en el momento justo.
3. **Proteger a Aelita.** Ella es la única que puede desactivar torres. Si cae al Mar Digital, se pierde la partida.
4. **Fiel a la serie.** Los nombres, los sonidos y la forma de luchar deben recordar a la serie (100 puntos de vida, devirtualización, “Retorno al pasado”).
5. **Campaña por temporadas.** Se juega la historia completa de la precuela a *Evolution*.

**Bucle de una misión típica:**

```
Kadic (vida diaria) → aparece una señal de X.A.N.A. → ir a la fábrica
→ escáneres → Lyoko: escoltar a Aelita hasta la torre
   (a la vez en la Tierra: sobrevivir al ataque de X.A.N.A.)
→ Aelita introduce CODE: LYOKO → "¡Retorno al pasado, ya!" → resultados
```

---

## 3. Estructura de juego

El juego tiene cuatro capas jugables que se alternan:

| Capa | Qué se hace | Personaje | Estilo |
| --- | --- | --- | --- |
| **Lyoko** | Combate, plataformas, escolta, puzles con torres | Odd, Ulrich, Yumi, Aelita, William | Acción 3D en tercera persona (lo que ya existe) |
| **Mundo real** | Explorar Kadic, hablar, esquivar a Jim, llegar a la fábrica, sobrevivir ataques | Toda la banda | Aventura/sigilo/escapes en tercera persona |
| **Operador** | Vigilar puntos de vida, materializar vehículos, ejecutar programas, abrir rutas | Jeremie (luego Aelita o Laura) | Interfaz del superordenador, minijuegos de tiempo |
| **Mar Digital** | Navegar con el Skidbladnir, combates submarinos, entrar en réplicas | Toda la banda | Pilotaje 3D (temporada 4) |

**Misiones divididas [Juego]:** en las misiones importantes el jugador controla a la vez a quien está en Lyoko y a quien se quedó en la Tierra (por ejemplo, Yumi y Odd en Lyoko mientras Ulrich frena a un espectro en Kadic). Se cambia entre ambos con un botón y un temporizador obliga a no descuidar ninguno de los dos frentes.

---

## 4. Historia y campaña completa

La campaña se divide en **capítulos = temporadas**. Cada capítulo tiene episodios-misión, un jefe y un final.

### Prólogo — *El despertar de X.A.N.A.* (precuela) [Canon]

- **Trama:** Jeremie Belpois encuentra una fábrica abandonada junto a Kadic y, dentro, un superordenador apagado. Lo enciende y descubre Lyoko y a una chica digital sin memoria: Aelita. Al encenderlo también despierta a **X.A.N.A.**, una inteligencia artificial hostil. Ulrich, Odd y Yumi se unen al descubrir el secreto.
- **Misiones:**
  1. *La fábrica* — explorar la fábrica (mundo real, tutorial de movimiento).
  2. *El superordenador* — tutorial de Operador con Jeremie: encender el sistema y hablar con Aelita.
  3. *Primera virtualización* — tutorial de combate con Ulrich en el Sector Bosque.
  4. *Formación de la banda* — tutorial de cambio de personaje y de vehículos.
- **Desbloquea:** Odd, Ulrich, Yumi, Sector Bosque, el programa “Retorno al pasado”.

### Capítulo 1 — Temporada 1: *Materializar a Aelita* [Canon]

- **Trama:** X.A.N.A. ataca la Tierra activando torres. La banda la protege mientras Jeremie trabaja en un programa para traer a Aelita al mundo real. Al conseguirlo descubren que X.A.N.A. le ha implantado un **virus** que la ata al superordenador: si lo apagan, Aelita muere.
- **Puntos importantes:**
  - Se presentan los cuatro sectores y los monstruos básicos.
  - El **Retorno al pasado** se convierte en la herramienta para borrar los daños, pero X.A.N.A. se hace más fuerte cada vez que se usa.
  - Odd tiene la habilidad *Visión del futuro* (precognición).
  - Final: Aelita se materializa, pero queda unida a X.A.N.A. por el virus.
- **Misiones ejemplo:** torre en el Desierto, ataque eléctrico en Kadic, Megatank en el Sector Hielo, misión dividida con Jim de por medio.
- **Jefe final:** asalto a la torre del Sector Montaña con hordas mixtas y dos Megatanks.
- **Desbloquea:** Desierto, Hielo, Montaña, Aelita en el mundo real.

### Capítulo 2 — Temporada 2: *El Sector 5 y Franz Hopper* [Canon]

- **Trama:** Aelita vive en Kadic como estudiante. La banda descubre el **Sector 5 (Cartago)**, el corazón de Lyoko, y pistas sobre su creador, **Franz Hopper**, que resulta ser el padre de Aelita. X.A.N.A. intenta robar la memoria de Aelita con la **Scyphozoa**.
- **Puntos importantes:**
  - Nuevos monstruos: **Tarántula, Creeper, Manta, Guardián, Scyphozoa**.
  - Aelita aprende los **Campos de energía**.
  - Odd pierde la Visión del futuro y gana el **Escudo**.
  - La Ermita (la casa de Franz) y sus secretos.
  - Final: X.A.N.A. roba la memoria de Aelita y escapa al exterior; Franz Hopper le devuelve la vida. Aelita ya no depende del superordenador.
- **Mecánica nueva:** el temporizador del Sector 5 (la cuenta atrás de la llave de acceso) y la Cúpula Celeste.
- **Jefe final:** persecución de la Scyphozoa en el Núcleo de Lyoko.
- **Desbloquea:** Sector 5, Ermita, Campos de energía de Aelita.

### Capítulo 3 — Temporada 3: *La destrucción de Lyoko* [Canon]

- **Trama:** X.A.N.A., libre en la red, tiene las **llaves de Lyoko** (robadas de la memoria de Aelita) y empieza a **destruir los sectores** uno a uno para aislar a la banda. William Dunbar se une al grupo, pero en su primera misión la Scyphozoa lo captura y X.A.N.A. lo posee.
- **Puntos importantes:**
  - Aelita consigue sus **alas** (vuelo).
  - Nuevo monstruo: la **Marabunta** (enjambre que devora el terreno).
  - Los cuatro sectores van cayendo uno a uno (el primero es el Hielo) y el último en desaparecer es el Sector 5.
  - Final: Lyoko destruido; el grupo cree que todo está perdido.
- **Mecánica nueva [Juego]:** los sectores se “derrumban” durante las misiones: suelo que se borra, rutas que desaparecen. Al terminar cada misión, el sector queda bloqueado en el mapa.
- **Jefe final:** **William poseído por X.A.N.A.** en el Sector 5 mientras el núcleo colapsa.
- **Desbloquea:** vuelo de Aelita, William (como enemigo).

### Capítulo 4 — Temporada 4: *El Mar Digital* [Canon]

- **Trama:** Jeremie reconstruye Lyoko con los datos de Franz Hopper. X.A.N.A. ha creado **réplicas** de los sectores en otros superordenadores del mundo para conquistarlo. La banda construye el **Skidbladnir** para viajar por el **Mar Digital** y destruir las réplicas. Aparece el **Kolossus**. Al final rescatan a William y Jeremie y Aelita terminan el **programa multiagente** para destruir a X.A.N.A.
- **Puntos importantes:**
  - Nuevos trajes de la temporada 4 para todos.
  - Ulrich gana su **segunda katana** e *Impacto*. Yumi ataca con dos abanicos sin perder la telequinesis.
  - Monstruos del Mar Digital: **Kongre, Tiburón, Kalamar**.
  - Réplicas: volcán, bosque, hielo, desierto, y la de los laboratorios militares.
  - Se rescata a William. Durante un tiempo, un **clon polimórfico** ocupa su lugar en Kadic.
  - Final: **Franz Hopper se sacrifica** para dar energía al programa multiagente. X.A.N.A. queda destruido y Aelita apaga el superordenador.
- **Jefe final [Juego]:** asalto final en cadena: Kolossus → William → sobrevivir mientras se ejecuta el programa multiagente → secuencia final de despedida de Franz Hopper.
- **Desbloquea:** Skidbladnir, Mar Digital, William jugable, trajes T4.

### Capítulo 5 (Epílogo) — *Code Lyoko Evolution* [Canon]

- **Trama:** el superordenador se vuelve a encender porque X.A.N.A. sobrevivió. Aparecen **Laura Gauthier**, una alumna muy inteligente que quiere entrar en el grupo, y el **Profesor Tyron**, un científico que usa su propio superordenador. Se descubre **Cortex**, un mundo virtual nuevo creado por Tyron.
- **Puntos importantes:**
  - Nuevo enemigo: los **Ninjas** (avatares controlados por X.A.N.A.).
  - Lyoko renovado y Cortex.
  - Final abierto (la serie termina sin cerrar la historia) → **[Juego]** escribimos un final propio y lo marcamos como no canon.
- **Desbloquea:** Cortex, trajes Evolution, modo Ninja en la Arena.

### Episodios libres / misiones secundarias [Juego]

Misiones cortas inspiradas en episodios sueltos que no son de la trama principal: tormentas, días de examen, citas fallidas, Sissi espiando. Se juegan desde el mapa de Kadic cuando “X.A.N.A. activa una torre” al azar.

---

## 5. Personajes jugables y sus habilidades

**Reglas comunes [Canon]:** todos los guerreros tienen **100 puntos de vida (PV)** en Lyoko. Al llegar a 0 se **devirtualizan**: salen del escáner en la fábrica y ya no pueden volver a esa misión. Si caen al **Mar Digital** el resultado es el mismo, excepto para Aelita (ver 13.3).

**Formato de cada ficha:** rol, arma, habilidades por temporada, vehículo, ataques del juego y estadísticas base sobre 10 **[Juego]**.

---

### 5.1 Odd Della Robbia — El gato

- **Rol:** daño a distancia, movilidad, molestar a los enemigos.
- **Avatar:** traje morado de gato con cola y orejas (T1–T3), traje T4 morado y negro.
- **Arma [Canon]:** **Flechas láser** disparadas desde los guanteletes de los brazos.
- **Habilidades [Canon]:**
  - **Visión del futuro** (T1): ve unos segundos del futuro.
  - **Escudo** (T2+): escudo de energía violeta en el brazo.
  - **Garras**: escala paredes, araña.
  - Agilidad felina: saltos altos, caer de pie.
- **Vehículo:** **Overboard**.
- **Personalidad:** gracioso, glotón, mujeriego. Tiene un perro, Kiwi, escondido en su cuarto.

| Botón | Ataque [Juego] |
| --- | --- |
| Ataque básico | Ráfaga de flechas láser (rápida, poco daño) |
| Ataque especial | Flecha cargada (perfora y aturde) |
| Habilidad 1 | **Escudo** (bloquea láseres frontales; se agota con los golpes) |
| Habilidad 2 | **Visión del futuro**: cámara lenta de 3 s con la trayectoria de los disparos enemigos marcada (solo en el Capítulo 1 y en modo libre) |
| Pasiva | Trepar paredes con garras, doble salto |
| Definitiva | **Lluvia felina**: salto alto y lluvia de flechas en área |

| Vida | Velocidad | Daño | Alcance | Defensa | Control |
| --- | --- | --- | --- | --- | --- |
| 100 | 9 | 5 | 9 | 4 | 4 |

---

### 5.2 Ulrich Stern — El samurái

- **Rol:** cuerpo a cuerpo, mucho daño en ráfaga, rematar enemigos grandes.
- **Avatar:** samurái amarillo (T1–T3), traje T4 marrón con dos katanas.
- **Arma [Canon]:** **Katana** (dos katanas en T4).
- **Habilidades [Canon]:**
  - **Supersprint**: carrera ultrarrápida.
  - **Triplicado**: crea dos clones de sí mismo.
  - **Triangular**: los tres Ulrich rodean al enemigo y solo el real ataca.
  - **Impacto** (T4): golpe potente.
- **Vehículo:** **Overbike**.
- **Personalidad:** serio, solitario, enamorado de Yumi pero no se atreve a decírselo. Es el mejor luchador del grupo.

| Botón | Ataque [Juego] |
| --- | --- |
| Ataque básico | Combo de katana de 3 golpes |
| Ataque especial | Tajo cargado; rompe guardias y escudos |
| Habilidad 1 | **Supersprint** (esquiva + carga, atraviesa disparos) |
| Habilidad 2 | **Triplicado** (2 clones que atraen el fuego enemigo y hacen poco daño) |
| Combo | Triplicado + Supersprint = **Triangular** (los clones rodean al objetivo y el golpe del real es crítico) |
| Definitiva | **Impacto** (T4): golpe en área que derriba a todos |
| Bloqueo | Desviar láseres con la katana si se pulsa en el momento justo |

| Vida | Velocidad | Daño | Alcance | Defensa | Control |
| --- | --- | --- | --- | --- | --- |
| 100 | 8 | 9 | 2 | 6 | 3 |

---

### 5.3 Yumi Ishiyama — La geisha

- **Rol:** control de masas, apoyo, mover cosas por el escenario.
- **Avatar:** geisha japonesa (T1–T3), traje T4 negro y morado.
- **Arma [Canon]:** **Abanicos** (tessen) que lanza como bumeranes.
- **Habilidades [Canon]:**
  - **Telequinesis**: mueve objetos, rocas y enemigos con la mente; se cansa al usarla.
  - Acrobacias y artes marciales.
- **Vehículo:** **Overwing**.
- **Personalidad:** madura, tranquila, un curso mayor que los demás. Ulrich le gusta, pero también William la persigue.

| Botón | Ataque [Juego] |
| --- | --- |
| Ataque básico | Lanzar abanico (vuelve como bumerán; golpea a la ida y a la vuelta) |
| Ataque especial | Lanzar los dos abanicos en abanico ancho |
| Habilidad 1 | **Telequinesis: agarrar**: levanta a un enemigo pequeño o una roca y la lanza |
| Habilidad 2 | **Telequinesis: muro**: levanta una barrera de terreno que tapa los láseres |
| Puzle | Mover bloques, puentes y plataformas con telequinesis |
| Definitiva | **Danza de abanicos**: torbellino que absorbe a los enemigos cercanos |
| Coste | La telequinesis gasta **Concentración**; si se agota, Yumi queda aturdida 2 s |

| Vida | Velocidad | Daño | Alcance | Defensa | Control |
| --- | --- | --- | --- | --- | --- |
| 100 | 7 | 6 | 6 | 5 | 10 |

---

### 5.4 Aelita Schaeffer (Aelita Stones) — La princesa de Lyoko

- **Rol:** objetivo de la misión, apoyo, la llave de todo. **Es la única que puede desactivar torres.**
- **Avatar:** elfa rosa (T1–T3), traje T4 rosa y blanco.
- **Arma [Canon]:** **Campos de energía**: esferas rosas que lanza con las manos (T2+).
- **Habilidades [Canon]:**
  - **Creatividad**: crea o borra partes del terreno (puentes, muros) cantando/concentrándose.
  - **Alas** (T3+): vuela.
  - **Llaves de Lyoko**: entra en las torres e introduce *CODE: LYOKO*.
  - Siente cuándo hay una torre activada cerca.
- **Vehículo:** pasajera del Overwing o de otros vehículos; piloto de una Nav Skid en T4.
- **Personalidad:** curiosa, valiente, al principio no sabe nada del mundo real. Hija de Franz Hopper y Anthea.

**Cómo se usa en el juego [Juego]:**

- **Capítulos 1–2:** Aelita va **controlada por la IA** y hay que escoltarla (ya hecho en el prototipo). Se le pueden dar órdenes sencillas: *seguirme / esperar / esconderse*.
- **Capítulo 3+:** pasa a ser **jugable** cuando consigue las alas.

| Botón | Ataque [Juego] |
| --- | --- |
| Ataque básico | Campo de energía (proyectil teledirigido lento) |
| Ataque especial | Campo de energía cargado (explota en área) |
| Habilidad 1 | **Creatividad: puente**: crea una plataforma temporal |
| Habilidad 2 | **Creatividad: muro**: crea un muro que bloquea el paso de los monstruos |
| Pasiva (T3+) | **Alas**: planear y volar a ratos |
| Torre | Mantener `E` dentro de la torre para introducir el código |

| Vida | Velocidad | Daño | Alcance | Defensa | Control |
| --- | --- | --- | --- | --- | --- |
| 100 | 6 | 5 | 7 | 3 | 7 |

---

### 5.5 William Dunbar — El guerrero rebelde

- **Rol:** tanque/pesado. Jugable al final del Capítulo 4 y en Evolution. Enemigo y jefe en el Capítulo 3 y parte del 4.
- **Avatar:** traje blanco y negro (normal) / traje negro con el ojo de X.A.N.A. (poseído).
- **Arma [Canon]:** **Espadón** (zweihänder) enorme.
- **Habilidades [Canon]:**
  - **Supernube**: se convierte en humo negro para moverse muy rápido y atravesar enemigos.
  - **Levitación** y **superfuerza** (poseído por X.A.N.A.).
  - Cuando está poseído pilota una **Manta negra**.
- **Personalidad:** chulo, valiente, compite con Ulrich por Yumi.

| Botón | Ataque [Juego] |
| --- | --- |
| Ataque básico | Mandoble lento y muy fuerte |
| Ataque especial | Onda de choque con el espadón contra el suelo |
| Habilidad 1 | **Supernube** (desplazamiento invulnerable) |
| Habilidad 2 | Bloqueo pesado (aguanta casi todo de frente) |
| Definitiva | **Golpe del Titán**: carga y tajo que parte enemigos con armadura |

| Vida | Velocidad | Daño | Alcance | Defensa | Control |
| --- | --- | --- | --- | --- | --- |
| 100 | 5 | 10 | 3 | 9 | 4 |

---

### 5.6 Jeremie Belpois — El operador

- **Rol:** **no lucha en Lyoko**. Es el cerebro: controla el superordenador.
- **Personalidad:** genio, tímido, enamorado de Aelita. Siempre con su portátil.
- **En el juego [Juego]: Modo Operador.** Durante las misiones en Lyoko hay una interfaz (vista del superordenador) donde el jugador (o la IA, si se prefiere) puede:

| Programa del superordenador | Efecto en el juego |
| --- | --- |
| **Virtualización / Transferencia / Escaneo** | Enviar a un personaje a Lyoko (y elegir dónde aparece) |
| **Materializar vehículo** | Hacer aparecer un vehículo junto al personaje |
| **Ver PV** | Ver la vida de todos y avisar del peligro |
| **Escanear torre activada** | Marca la torre activada en el mapa |
| **Retorno al pasado** | Al terminar la misión, deshace los daños del mundo real (ver 13.4) |
| **Materialización** | Devolver a alguien a la Tierra en medio de una misión |
| **Programas especiales** | Contraantivirus, cerrar mareas de datos, sacar la clave del Sector 5, lanzar el Skidbladnir, programa multiagente |

Minijuegos del operador **[Juego]**: teclear códigos contra reloj, encontrar la frecuencia del ataque, redirigir energía, abrir rutas en el Mar Digital.

---

## 6. Personajes secundarios

Personajes del mundo real. No luchan en Lyoko, pero aparecen en la historia, en Kadic y en algunas misiones.

| Personaje | Quién es | Papel en el juego |
| --- | --- | --- |
| **Elisabeth “Sissi” Delmas** | Hija del director, persigue a Ulrich | Molesta, espía, a veces víctima de X.A.N.A. Misiones de sigilo para evitarla |
| **Herb Pichon y Nicolas Poliakoff** | Los secuaces de Sissi | Patrullan Kadic, momentos cómicos |
| **Jim Morales** | Profesor de educación física | “Guardia” de Kadic: si te pilla fuera de clase, te castiga. Una vez sí supo de Lyoko, pero le borraron la memoria |
| **Sr. Delmas** | Director de Kadic | Castigos, reglas, escenas de historia |
| **Suzanne Hertz** | Profesora de ciencias | Escenas en clase, misiones de examen |
| **Rosa** | Cocinera del comedor | Odd y la comida (coleccionables) |
| **Milly y Tamiya** | Periodistas del periódico de Kadic | Pistas y rumores (misiones secundarias) |
| **Kiwi** | El perro de Odd | Coleccionables y gags |
| **Franz Hopper (Waldo Schaeffer)** | Creador de Lyoko y X.A.N.A., padre de Aelita | Pista central de la historia. Guía misteriosa en T2–T4. Se sacrifica al final |
| **Anthea Schaeffer (Hopper)** | Madre de Aelita | Recuerdos y flashbacks; trama de memoria de Aelita |
| **Laura Gauthier** | Alumna genio (Evolution) | Operadora rival y aliada temporal |
| **Padres de los personajes** | — | Llamadas y escenas puntuales |

---

## 7. X.A.N.A. y antagonistas

### 7.1 X.A.N.A. [Canon]

- **Qué es:** un programa multiagente (inteligencia artificial) creado por Franz Hopper que se volvió malvado y quiere dominar el mundo real.
- **Símbolo:** el **ojo de X.A.N.A.**: aparece en los monstruos, en las pupilas de los poseídos y en todo lo que controla.
- **Cómo ataca:** **activa torres** en Lyoko. Cada torre activada le da poder para atacar la Tierra. Para frenar el ataque, Aelita debe desactivarla.
- **Torres [Canon]:** halo **azul** = neutral, halo **rojo** = activada por X.A.N.A., halo **verde** = activada por Jeremie/Aelita, halo **blanco** = activada por Franz Hopper.
- **Evolución en el juego [Juego]:** cada “Retorno al pasado” sube el **Nivel de X.A.N.A.** (ver 13.4), que hace más difíciles los siguientes ataques.

### 7.2 William poseído (X.A.N.A. William)

Ver 5.5 y 9. Jefe recurrente en los Capítulos 3 y 4. Duelo uno contra uno con Ulrich o Yumi.

### 7.3 Profesor Tyron (Evolution)

Científico que estudia el superordenador por su cuenta. Antagonista humano. Los **Ninjas** y la tecnología de Cortex están a su alcance.

### 7.4 Espectros y clones

X.A.N.A. puede crear **espectros** (fantasmas de energía) que poseen personas o adoptan una forma física, y **clones polimórficos** que se hacen pasar por alguien. Ver sección 10.

---

## 8. Bestiario: todos los monstruos

Todos los monstruos llevan el **ojo de X.A.N.A.**, que es su **punto débil**: si se le acierta, cae de un golpe o recibe daño crítico.

Valores **[Juego]**: PV del monstruo, daño de su ataque contra los 100 PV del jugador y amenaza (1–5).

### 8.1 Monstruos de Lyoko (Temporada 1)

| Monstruo | Aspecto | Ataque | Punto débil y cómo vencerlo | PV | Daño | Amenaza | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Kankrelat** | Cucaracha pequeña | Láser desde el ojo | Ojo en la parte de arriba; va en grupos | 20 | 10 | 1 | [Hecho] |
| **Blok** | Cubo con ojos en las caras | Láser, láser congelante, anillos de fuego, rayo de “prisión” | Ojo de cualquier cara; hay que flanquear | 50 | 10–20 | 2 | [Hecho] |
| **Krabe** | Cangrejo alto de cuatro patas | Láser y patadas con las patas | Ojo en la parte de arriba del cuerpo; derribarlo atacando las patas | 80 | 20 | 3 | [Hecho] |
| **Hornet (Frelion)** | Avispa voladora | Láser rápido y ácido | Ojo en la frente; mejor con ataques a distancia | 30 | 10 | 2 | [Hecho] |
| **Megatank** | Bola rodante blindada | Se abre y dispara un rayo enorme | Solo es vulnerable con la armadura abierta: esquivar el rayo y atacar el ojo | 150 | 40–60 | 4 | [Hecho] |

### 8.2 Monstruos del Sector 5 (Temporada 2+)

| Monstruo | Aspecto | Ataque | Punto débil y cómo vencerlo | PV | Daño | Amenaza | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Tarántula** | Bípedo resistente que se apoya en cuatro patas | Ráfaga de láser muy precisa con los dos brazos | Muy resistente; hay que rodearla | 120 | 15 por ráfaga | 4 | [Pendiente] |
| **Creeper (Rampant)** | Serpiente grande | Rayo desde la boca; sube por paredes y techos | Ojo en la cabeza; emboscadas en el Sector 5 | 60 | 20 | 3 | [Pendiente] |
| **Manta** | Raya voladora grande | Láser y **minas** que suelta en vuelo | Ojo en el lomo; derribarla en vuelo | 90 | 20 | 3 | [Pendiente] |
| **Guardián** | Esfera de energía | Atrapa a un guerrero dentro de una burbuja | No se puede romper desde dentro: los compañeros o Jeremie deben liberarlo | — | Atrapa | 3 | [Pendiente] |
| **Scyphozoa** | Medusa gigante con tentáculos | Agarra a Aelita y **le roba la memoria**; también posee guerreros | Atacar los tentáculos para soltar a Aelita | 200 | Robo de memoria | 5 | [Pendiente] |

### 8.3 Monstruos de la Temporada 3

| Monstruo | Aspecto | Ataque | Punto débil y cómo vencerlo | PV | Daño | Amenaza | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Marabunta** | Enjambre que se extiende como una mancha y devora el terreno | Absorbe a los guerreros y el suelo | No tiene un solo ojo: hay que destruir el núcleo o escapar | Área | 10/s | 5 | [Pendiente] |
| **Manta negra** | Manta oscura | Montura de William poseído | Ver jefe de William | 120 | 20 | 4 | [Pendiente] |

### 8.4 Monstruos del Mar Digital (Temporada 4)

| Monstruo | Aspecto | Ataque | Punto débil y cómo vencerlo | PV | Daño | Amenaza | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Kongre** | Anguila/piraña del Mar Digital | Muerde y se engancha a las Nav Skids | Torpedos de las Nav Skids | 40 | 10 al casco | 2 | [Pendiente] |
| **Tiburón (Shark)** | Tiburón robótico | Torpedos | Torpedos y maniobras | 70 | 20 al casco | 3 | [Pendiente] |
| **Kalamar** | Calamar gigante | Se agarra al Skidbladnir y lo perfora | Desenganchar las Nav Skids y atacar desde fuera | 250 | 30 al casco | 4 | [Pendiente] |
| **Kolossus** | Gigante de lava y roca | Puñetazos, lava, destrozar el Skidbladnir | Jefe (ver 9) | 1000 | 50+ | 5 | [Pendiente] |

### 8.5 Enemigos de Evolution

| Enemigo | Aspecto | Ataque | Cómo vencerlo | PV | Daño | Amenaza | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Ninjas** | Avatares humanoides negros | Espadas, golpes y movilidad, como un guerrero | Duelos; esquivar y contraatacar | 100 | 10–20 | 3 | [Pendiente] |

### 8.6 Variantes de los videojuegos oficiales (opcionales) [Juego]

Inspiradas en los juegos oficiales (*Code Lyoko* de DS, *Quest for Infinity*, *Fall of X.A.N.A.*). Sirven para dar variedad sin salirse de la estética.

| Variante | Idea | Amenaza |
| --- | --- | --- |
| Monstruos oscuros | Versiones “corrompidas” más fuertes de los normales | +1 |
| Escorpión | Enemigo de cola con disparo en arco | 3 |
| Cucarachas explosivas | Kankrelats kamikazes | 2 |
| Ácaros | Enjambres diminutos | 1 |
| Volcanoides | Monstruos de lava de las réplicas volcánicas | 3 |
| Insektos | Insectos voladores en enjambre | 2 |
| Omegatank / Ubertank | Megatanks mejorados (más rayos, más armadura) | 5 |

---

## 9. Jefes

| Jefe | Capítulo | Dónde | Mecánica [Juego] |
| --- | --- | --- | --- |
| **Megatank alfa** | Prólogo / Cap. 1 | Bosque | Esquivar el rayo, golpear el ojo con la armadura abierta. Tutorial de jefes ([Hecho] versión básica) |
| **Asedio a la torre** | Cap. 1 | Montaña | Oleadas mixtas con dos Megatanks. Proteger a Aelita mientras crea un puente |
| **Scyphozoa** | Cap. 2 | Núcleo / Sector 5 | Atrapa a Aelita: tienes 30 s para cortar los tentáculos antes de que le robe la memoria |
| **Guardián + Tarántulas** | Cap. 2 | Arena del Sector 5 | Un compañero queda atrapado; el otro aguanta mientras Jeremie descifra la burbuja |
| **Marabunta** | Cap. 3 | Montaña | Huida: el suelo se come detrás de ti; al final, destruir el núcleo |
| **William poseído** | Cap. 3 y 4 | Sector 5 / réplicas | Duelo 1 contra 1. Fases: espadón → Supernube → montado en la Manta negra |
| **Kalamar** | Cap. 4 | Mar Digital | Combate de vehículos: soltar las Nav Skids y quitarle los tentáculos del casco |
| **Kolossus** | Cap. 4 | Réplica del volcán / Sector 5 | Jefe gigante: subir por su cuerpo, golpear las placas de su espalda. Destroza el Skidbladnir si no se le para |
| **Final de X.A.N.A.** | Cap. 4 | Red / Sector 5 | Resistir oleadas de todos los monstruos mientras el programa multiagente carga; escena final de Franz Hopper |
| **Tyron / Ninjas** | Cap. 5 | Cortex | Combate contra un equipo de Ninjas que imitan las habilidades de la banda |

---

## 10. Ataques de X.A.N.A. en el mundo real

Cuando X.A.N.A. activa una torre, ataca la Tierra. En el juego, estas son las misiones de **mundo real**, que ocurren a la vez que la misión en Lyoko.

| Tipo de ataque [Canon] | Qué pasa | Cómo se juega [Juego] |
| --- | --- | --- |
| **Espectros** | Fantasmas de electricidad que persiguen o poseen a gente | Huida/sigilo; aturdirlos con extintores o electricidad |
| **Poseídos (zombis)** | Personas con el ojo de X.A.N.A. en los ojos, fuerza sobrehumana | Esquivar, no se les puede herir (son inocentes). Llegar a la fábrica |
| **Clones polimórficos** | Copias falsas de alguien conocido | Detectar al impostor (detalles raros, diálogos) |
| **Electricidad y máquinas** | Fallos eléctricos, coches o máquinas que cobran vida | Escapes y puzles de entorno |
| **Clima y naturaleza** | Tormentas, niebla, enjambres de insectos, animales | Supervivencia contra el tiempo |
| **Satélites, redes, ejército** | Ataques a gran escala (T4) | Misiones de infiltración y escape |
| **Sueños e ilusiones** | X.A.N.A. engaña con alucinaciones | Niveles “trampa” donde nada es lo que parece |

**Regla [Juego]:** el ataque del mundo real tiene una **barra de amenaza**. Si llega al máximo antes de desactivar la torre, la misión falla (a menos que se haga Retorno al pasado de emergencia, con penalización).

---

## 11. Mundos y mapas

### 11.1 Lyoko [Canon]

| Zona | Aspecto | Característica de juego [Juego] | Estado |
| --- | --- | --- | --- |
| **Sector Bosque** | Árboles gigantes, caminos estrechos | Rutas lineales, emboscadas entre troncos | [Hecho] básico |
| **Sector Desierto** | Mesetas y cañones naranjas | Espacios abiertos, Megatanks y Hornets, vehículos | [Pendiente] |
| **Sector Hielo** | Glaciares y témpanos | Suelo resbaladizo, puentes de hielo que se rompen | [Pendiente] |
| **Sector Montaña** | Montañas flotantes, caminos al borde del vacío | Plataformas verticales, riesgo de caer al Mar Digital | [Pendiente] |
| **Sector 5 (Cartago)** | Esfera interior azul, laberinto que cambia | Paredes que se mueven, cuenta atrás, Cúpula Celeste, **Núcleo de Lyoko**, **Arena** | [Pendiente] |
| **Mar Digital** | Océano de datos que rodea Lyoko y conecta la red | Caerse = devirtualización. En T4 se navega con el Skidbladnir | [Hecho] como peligro |
| **Torres** | Torres blancas con interior de plataformas | Desactivación, viajar entre sectores (torre de paso) | [Hecho] básico |
| **Réplicas** (T4) | Copias de sectores (volcán, bosque, hielo, desierto) en otros superordenadores | Variaciones “corruptas” de los sectores, con laboratorios de X.A.N.A. | [Pendiente] |
| **Cortex** (Evolution) | Mundo virtual nuevo, más tecnológico | Enemigos Ninja, estructuras que cambian | [Pendiente] |

### 11.2 Mundo real [Canon]

| Lugar | Qué hay | Uso en el juego [Juego] |
| --- | --- | --- |
| **Academia Kadic** | Aulas, residencias, comedor, patio, gimnasio, despacho del director | Centro del juego: misiones secundarias, conversaciones, coleccionables, esquivar a Jim |
| **Habitación de Jeremie** | Ordenador conectado al superordenador | Guardar partida, ver la historia, pantalla de sistemas |
| **Parque y alcantarillas** | Entrada secreta a las alcantarillas que llevan a la fábrica (en monopatín/patinete) | Trayecto rápido a la fábrica; persecuciones |
| **La fábrica** | Ascensor, **sala del superordenador** (con la silla de Jeremie), **sala de escáneres**, **el superordenador** abajo | Base de operaciones. Mejoras y equipamiento |
| **La Ermita** | Casa abandonada de Franz Hopper en el bosque | Exploración, secretos, flashbacks de Aelita |
| **Ciudad** | Calles cercanas, tiendas | Ataques de X.A.N.A. a mayor escala |

---

## 12. Vehículos

| Vehículo | Piloto | Cómo funciona [Canon] | En el juego [Juego] | Estado |
| --- | --- | --- | --- | --- |
| **Overboard** | Odd | Tabla voladora | Rápido y ágil, piruetas, dispara a la vez | [Hecho] prototipo |
| **Overbike** | Ulrich | Moto flotante de una rueda | Muy rápida, embiste a los enemigos, puede atravesar paredes del Sector 5 | [Hecho] prototipo |
| **Overwing** | Yumi (Aelita pasajera) | Moto voladora | Estable, lleva dos personas | [Hecho] prototipo |
| **Skidbladnir** | Todo el equipo (lo pilota Aelita/Jeremie) | Submarino para el Mar Digital | Pilotaje, sonar, escudo, torpedos; las Nav Skids se separan | [Pendiente] |
| **Nav Skids** | Cada guerrero | Navecillas que forman el Skidbladnir | Combate submarino individual, torpedos | [Pendiente] |
| **Orbe de transporte** | — | Esfera que lleva del sector a Cartago | Transición entre sectores y el Sector 5 | [Pendiente] |
| **Manta negra** | William poseído | Manta controlada por X.A.N.A. | Montura del jefe; quizá jugable en modo libre | [Pendiente] |

Los vehículos los materializa el Operador (ver 5.6). Si el vehículo pierde toda su vida, desaparece y el pasajero cae.

---

## 13. Sistemas de juego

### 13.1 Puntos de vida y devirtualización [Canon]

- Cada guerrero tiene **100 PV**. El HUD los muestra como en la serie (la pantalla de Jeremie).
- Con **0 PV** te devirtualizas: desaparece el avatar en píxeles y sales del escáner en la fábrica.
- Un guerrero devirtualizado **no puede volver** a Lyoko en esa misión **[Juego]**: pasa al **mundo real**, donde puede ayudar en el frente de la Tierra. Así, perder a alguien en Lyoko no es solo un castigo.

### 13.2 Cambio de personaje

- En Lyoko se controla a un personaje y los demás van con IA. Se cambia con `1`–`4` ([Hecho] para 3).
- **Sinergias [Juego]:** algunos combos solo existen si dos personajes están juntos:
  - **Yumi + Ulrich:** Yumi levanta a un enemigo y Ulrich lo remata en el aire.
  - **Odd + Aelita:** el escudo de Odd protege a Aelita mientras carga un campo de energía doble.
  - **Yumi + Aelita:** Creatividad + Telequinesis = puentes enormes.
  - **Ulrich + William:** doble golpe pesado que rompe la armadura de los Megatanks.

### 13.3 Proteger a Aelita

- Si Aelita se devirtualiza en Lyoko o cae al Mar Digital, **la misión falla** **[Juego]**. En los capítulos 1–2 en el canon, que cayera al Mar Digital habría significado perderla para siempre.
- La Scyphozoa intenta atraparla: si nadie la defiende, le roba la memoria (fallo de misión en el Cap. 2).

### 13.4 Retorno al pasado y Nivel de X.A.N.A.

- **[Canon]** Al final de una misión Jeremie lanza el **Retorno al pasado**: el tiempo retrocede y se borran los daños del ataque. Solo los que han estado en Lyoko recuerdan lo que pasó.
- **[Canon]** Cada Retorno al pasado hace a X.A.N.A. más fuerte.
- **[Juego]** Por eso existe el **Nivel de X.A.N.A.**: cada Retorno al pasado lo sube un poco y los monstruos ganan algo de vida o aparecen más. Las misiones que se terminan **sin necesitar el Retorno al pasado** (cuando el ataque del mundo real no causó daños graves) no lo suben y dan más recompensa.
- **[Juego]** También se puede usar un **Retorno de emergencia** para repetir una misión fallida, pero sube mucho el Nivel de X.A.N.A.

### 13.5 Combate

- Tercera persona con fijado de objetivo (`Tab`) ([Hecho]).
- **Punto débil:** el ojo de X.A.N.A. ([Hecho] `WeakPoint.cs`).
- **Esquivar y desviar:** esquivar en el último momento ralentiza el tiempo; Ulrich puede devolver láseres con la katana.
- **Daño de caída:** caer al Mar Digital = 0 PV.
- **Energía especial:** se llena al golpear y permite usar la habilidad definitiva.

### 13.6 Torres

- Llegar con Aelita a la torre activada → entrar → subir a la plataforma superior → *CODE: LYOKO*.
- **Torre de paso [Canon]:** se puede usar para viajar de un sector a otro.
- Puzles **[Juego]:** algunas torres están cerradas por un Guardián, rodeadas por la Marabunta o en un sector que se derrumba.

### 13.7 Mundo real

- Movimiento libre por Kadic con horarios (clase, comida, noche) **[Juego]**.
- **Sigilo:** evitar a Jim por la noche, a Sissi y al director.
- **Coartadas:** si faltas a clase para ir a la fábrica, tienes que buscar una excusa (minijuego de diálogo). Demasiadas faltas = castigo (menos tiempo libre).

### 13.8 Guardado

- Se guarda en el ordenador de Jeremie y al terminar cada misión ([Hecho] `Checkpoint.cs` para los puntos de control dentro de la misión).

---

## 14. Progresión, coleccionables y desbloqueables

**Mejoras de personaje [Juego]:**

- Cada misión da **Datos** (moneda). Jeremie los usa en la fábrica para mejorar los “programas” de cada guerrero: más daño, más duración de las habilidades, menos tiempo de espera.
- Las habilidades se desbloquean siguiendo la historia (Escudo de Odd en el Cap. 2, alas de Aelita en el Cap. 3, segunda katana de Ulrich en el Cap. 4, etc.).

**Coleccionables:**

| Coleccionable | Dónde | Recompensa |
| --- | --- | --- |
| **Diario de Franz Hopper** | Ermita, Sector 5, réplicas | Historia del pasado de Lyoko |
| **Fragmentos de memoria de Aelita** | Lyoko | Recuerdos de su infancia y de Anthea |
| **Archivos de X.A.N.A.** | Torres y réplicas | Información de cada monstruo (enciclopedia) |
| **Comida de Rosa / Kiwi** | Kadic | Gags y trajes cómicos para Odd |
| **Canciones del grupo** | Kadic | Música para el menú (Yumi y los demás en el baile de Kadic) |

**Desbloqueables:**

- Trajes T1–T3, T4, Evolution y *Garage Kids* (el piloto original) para cada personaje.
- Modelos de enemigos en la enciclopedia.
- Modos extra (ver 15).

---

## 15. Modos extra

| Modo | Descripción [Juego] |
| --- | --- |
| **Misión libre** | Rejugar cualquier misión con cualquier personaje |
| **Arena del Sector 5** | Oleadas infinitas; ranking por oleadas |
| **Desafíos del Operador** | Solo Jeremie: minijuegos del superordenador contra reloj |
| **Jefes seguidos** | Todos los jefes uno detrás de otro |
| **Carreras en Lyoko** | Carreras con Overboard/Overbike/Overwing por los sectores |
| **Modo “¿Y si…?”** | Misiones no canon: Jeremie virtualizado, X.A.N.A. gana, William nunca poseído |

---

## 16. Controles

Configurados en `Assets/Game/Input/LyokoControls.inputactions` (F02). **[Hecho]** = ya funciona al jugar; **[Tecla lista]** = la tecla existe, pero la mecánica llega en la fase indicada.

**A pie (mapa `Lyoko`)**

| Acción | Teclado/ratón | Mando (Xbox / PlayStation) | Estado |
| --- | --- | --- | --- |
| Moverse | `WASD` / flechas | Stick izq. | [Hecho] |
| Cámara | Ratón | Stick der. | [Hecho] |
| Correr | `Shift` | L3 | [Hecho] |
| Saltar | `Space` | A / ✕ | [Hecho] |
| Esquivar | `Ctrl` | B / ○ | [Hecho] |
| Ataque básico (combos; mantener = cargado) | Clic izq. | X / □ | [Hecho] |
| Ataque especial | Clic der. | Y / △ | [Hecho] |
| Habilidad 1 | `Q` | LB / L1 | [Hecho] |
| Habilidad 2 | `F` | RB / R1 | [Hecho] |
| Definitiva | `X` | LB + RB / L1 + R1 | [Hecho] |
| Guardia / desvío | `C` | LT / L2 | [Hecho] |
| Fijar objetivo | `Tab` | R3 | [Hecho] |
| Vehículo | `V` | Cruceta arriba | [Hecho] |
| Interactuar / torre | `E` (mantener) | RT / R2 (mantener) | [Hecho] |
| Cambiar personaje | `1` `2` `3` `4` | Cruceta izq. / der. | [Hecho] (los demás siguen con IA) |
| Órdenes a Aelita (pulsar = siguiente; mantener = rueda) | `Z` | Cruceta abajo | [Hecho] |
| Abrir Operador | `O` | Select / Share | [Tecla lista] F13 |
| Cambiar Lyoko ↔ Tierra | `G` | — | [Tecla lista] F16 |

**Montado (mapa `Vehicle`)**

| Acción | Teclado/ratón | Mando | Estado |
| --- | --- | --- | --- |
| Dirigir | `WASD` | Stick izq. | [Hecho] |
| Subir / bajar | `Space` / `Ctrl` | A / B | [Hecho] |
| Turbo | `Shift` | RT / R2 | [Hecho] |
| Disparar | Clic izq. | X / □ | [Hecho] |
| Bajarse | `V` | Cruceta arriba | [Hecho] |
| Truco (llena energía especial) | `C` | LT / L2 | [Hecho] |
| Subir o bajar a Aelita (Overwing) | `E` | Cruceta abajo | [Hecho] |
| Overbike: salto / derrape | `Space` / `Ctrl` | A / B | [Hecho] |

**Interfaz (mapa `UI`)**

| Acción | Teclado | Mando | Estado |
| --- | --- | --- | --- |
| Reintentar (misión fallida) | `R` | Y / △ | [Hecho] |
| Pausa | `P` | Start / Options | [Tecla lista] F10 |
| Menú de depuración | `F1` | — | [Hecho] (solo editor y builds de desarrollo) |

También existen los mapas `RealWorld` (F14–F15) y `Operator` (F13), ya configurados.

---

## 17. Estilo visual y sonido

- **Lyoko:** 3D estilo cel-shading/toon como la serie, colores saturados por sector, cielo blanco. Personajes con contorno suave.
- **Mundo real:** en la serie es 2D. **[Juego]** En 3D con shader de cómic plano para que se distinga de Lyoko.
- **Devirtualización:** el avatar se rompe en polígonos/píxeles y desaparece.
- **HUD:** inspirado en la pantalla de Jeremie (barras de PV, mapa de sector, alertas de “¡torre activada!”). Sin flechas modernas: rutas marcadas con elementos del propio mundo (ver `Docs/BACKLOG.md`).
- **Sonido:** sonidos tipo “bip” del superordenador, voz de Jeremie como narrador de misión, música electrónica para Lyoko y música más tranquila para Kadic. No usar la música ni las voces oficiales: crear música propia inspirada en ese estilo.
- **Modelos:** seguir el flujo de `Docs/MODELING_PIPELINE.md` y anotar la licencia de cada modelo fan en `Assets/ArtSources/ASSET_SOURCES.md`.

---

## 18. Plan de producción

Orden recomendado para no perderse. Cada hito debe funcionar y quedar guardado en git antes de pasar al siguiente.

> El desglose completo, tarea por tarea, está en [`Fases/00_INDICE.md`](Fases/00_INDICE.md) (27 fases, de la F00 a la F26, un archivo por fase).

| Hito | Contenido | Depende de |
| --- | --- | --- |
| **H0 – Recuperar el proyecto** | Abrir en Unity, compilar, generar escena y prefabs. Seguir `Docs/UNITY_FIRST_OPEN_CHECKLIST.md` | — |
| **H1 – Vertical slice pulida** | La misión del Bosque bonita: modelos reales de Odd/Ulrich/Yumi/Aelita, animaciones, efectos de devirtualización | H0 |
| **H2 – Habilidades completas** | Habilidades 1/2/definitiva de los 4 guerreros (sección 5) | H1 |
| **H3 – Operador** | Interfaz de Jeremie, vehículos materializables, Retorno al pasado | H1 |
| **H4 – Mundo real básico** | Fábrica (ascensor, escáneres) y una zona pequeña de Kadic | H1 |
| **H5 – Capítulo 1 completo** | 4 sectores, monstruos de la T1, misiones divididas, jefe | H2, H3, H4 |
| **H6 – Capítulo 2** | Sector 5, Scyphozoa, Tarántula, Creeper, Manta, Guardián, Ermita | H5 |
| **H7 – Capítulo 3** | Destrucción de sectores, Marabunta, William poseído | H6 |
| **H8 – Capítulo 4** | Skidbladnir, Mar Digital, réplicas, Kalamar, Kolossus, final | H7 |
| **H9 – Epílogo Evolution** | Cortex, Ninjas, Laura, Tyron | H8 |
| **H10 – Extras** | Arena, carreras, jefes seguidos, trajes | H5+ |

**Consejo:** no intentar hacerlo todo a la vez. Con H0–H5 ya hay un juego completo y jugable de una temporada. El resto se añade capítulo a capítulo.
