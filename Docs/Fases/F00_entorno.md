# F00 — Preparar el entorno

> **Bloque A** — Base técnica · **Depende de:** — · **Estimación:** 1 semana
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [F01 →](F01_recuperar_prototipo.md)

**Objetivo:** tener todas las herramientas instaladas y el repo listo para trabajar con archivos grandes.

**Requisitos:** nada.

**Tareas**

- [x] **F0.1** Instalar **Unity Hub** y **Unity 2022.3.56f1** con el módulo *Windows Build Support (IL2CPP)*. Desinstalado Unity 6000.6.7f1 para no abrir el proyecto con otra versión.
- [ ] **F0.2** (Pendiente, no bloquea: de momento se puede usar Cursor) Instalar **Visual Studio 2022 Community** (lo más sencillo: marcarlo como módulo al instalar Unity en F0.1; Cursor/VS Code también sirve, pero con peor soporte de Unity) y comprobar que el autocompletado funciona con el proyecto abierto.
- [x] **F0.3** Instalar **Blender** (versión LTS actual) para modelos.
- [x] **F0.4** Instalar **Git LFS** (`git lfs install`) y crear `.gitattributes`: modelos, texturas, audio, vídeo y fuentes van por LFS; los YAML de Unity (`.unity`, `.prefab`, `.asset`, `.meta`…) como texto.
  - Hecho en este PC. En cada PC nuevo hay que ejecutar `git lfs install` una vez.
- [x] **F0.5** Revisar `.gitignore`: que ignore `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `Build/`, `*.csproj`, `*.sln`. Añadidos también IDEs (`.idea/`), `BuildLogs/` y archivos del sistema.
- [x] **F0.6** En Unity: `Edit > Project Settings > Editor` → comprobar *Asset Serialization: Force Text* y *Version Control: Visible Meta Files* (en 2022.3 ya vienen así por defecto; solo hay que mirarlo).
- [x] **F0.7** Tablero de tareas: se usan las casillas de `Docs/Fases/` y el estado de `00_INDICE.md`. Las plantillas de `.github/ISSUE_TEMPLATE/` quedan para reportar errores de Unity y tareas de assets.
- [ ] **F0.8** (Opcional) Instalar un editor de audio (**Audacity**) y de imagen (**Krita** o **GIMP**).

**Archivos:** `.gitattributes` (nuevo), `.gitignore` (revisar).

**Terminado cuando:** Unity abre el proyecto, Visual Studio reconoce los scripts, y un archivo `.png` de prueba subido a git aparece como LFS (`git lfs ls-files`).

**Riesgos:** si se abre el proyecto con otra versión de Unity, hay que reimportar todo y puede romper cosas → usar siempre la 2022.3.56f1.

---

[Índice](00_INDICE.md) · [F01 →](F01_recuperar_prototipo.md)
