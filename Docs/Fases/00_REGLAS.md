# Reglas comunes de todas las fases

> [Índice](00_INDICE.md)

## Cómo usar las fases

1. Las fases van **en orden**. Una fase no empieza hasta que las que necesita (columna *Depende de* del índice) cumplen su **criterio de terminado**. Algunas se pueden hacer en paralelo y el índice lo indica.
2. Cada tarea tiene un código (`F4.3` = Fase 4, tarea 3) para poder decir “vamos con la F4.3”.
3. Al terminar una tarea, se marca `[x]` en el archivo de su fase y se hace commit.
4. Al empezar o terminar una fase, se actualiza su **Estado** y la **Fase activa** en [00_INDICE.md](00_INDICE.md).
5. Las estimaciones suponen **una persona trabajando ~10 horas a la semana**.
6. Si una tarea resulta más grande de lo previsto, se divide en subtareas (`F4.3a`, `F4.3b`…) en su archivo antes de empezarla.

## Git

- `main` siempre debe abrir y compilar en Unity sin errores rojos.
- Una rama por fase o tarea grande: `fase-04-combate`, `fase-09-modelo-odd`…
- Commits pequeños con el código de la tarea: `F4.2: añadir esquiva con invulnerabilidad`.
- Antes de hacer merge a `main`: abrir Unity, pulsar Play y comprobar la misión de prueba.
- Archivos grandes (`.fbx`, `.blend`, texturas, audio) → **Git LFS** (se configura en F00).

## Unity

- Versión fija: **Unity 2022.3.56f1** (`ProjectSettings/ProjectVersion.txt`). No actualizarla a mitad de proyecto.
- Todo el código va en el espacio de nombres `CodeLyokoFanGame` (y subespacios: `.Data`, `.UI`, `.Operator`…).
- Los valores ajustables (vida, daño, velocidades) van en **ScriptableObjects** (`CharacterDefinition`, `EnemyDefinition`…), nunca escritos a fuego en el código.
- Los prefabs finales van en `Assets/Prefabs/`. Lo de `Assets/Game/Generated/` es provisional y se puede regenerar.

## Definición de “terminado” (para cualquier tarea)

- Funciona en Play sin errores en la consola.
- No rompe la misión de prueba del Bosque.
- Sus valores se pueden ajustar desde el Inspector o desde un ScriptableObject.
- Está en git.

## Receta de capítulo

Las fases F19–F23 son sobre todo **contenido** (niveles, misiones, monstruos nuevos) hecho con los sistemas que ya existen. Cada capítulo sigue estos pasos:

1. Escribir el guion de las misiones en `Docs/Guiones/CapituloX.md`.
2. Montar los niveles con bloques grises y probarlos.
3. Crear los monstruos y jefes nuevos.
4. Montar las misiones como `MissionDefinition`.
5. Hacer el arte de los niveles.
6. Añadir cinemáticas y diálogos.
7. Hacer pruebas con otras personas y equilibrar.
