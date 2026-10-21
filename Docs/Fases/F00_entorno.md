# F00 — Preparar el entorno

> **Bloque A** — Base técnica · **Depende de:** — · **Estimación:** 1 semana
> [Índice](00_INDICE.md) · [Reglas](00_REGLAS.md) · [F01 →](F01_recuperar_prototipo.md)

**Objetivo:** tener todas las herramientas instaladas y el repo listo para trabajar con archivos grandes.

**Requisitos:** nada.

**Tareas**

- [ ] **F0.1** Instalar **Unity Hub** y **Unity 2022.3.56f1** con los módulos *Windows Build Support (IL2CPP)* y *Visual Studio Community*.
- [ ] **F0.2** Instalar **Visual Studio 2022** (o VS Code + extensión de C# y Unity) y comprobar que el autocompletado funciona con el proyecto abierto.
- [ ] **F0.3** Instalar **Blender** (versión LTS actual) para modelos.
- [ ] **F0.4** Instalar **Git LFS** (`git lfs install`) y crear `.gitattributes` con:
  - `*.fbx`, `*.blend`, `*.png`, `*.psd`, `*.tga`, `*.wav`, `*.mp3`, `*.ogg`, `*.exr`, `*.anim`(si son grandes) → LFS.
- [ ] **F0.5** Revisar `.gitignore`: que ignore `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `Build/`, `*.csproj`, `*.sln`.
- [ ] **F0.6** En Unity: `Edit > Project Settings > Editor` → *Asset Serialization: Force Text* y *Version Control: Visible Meta Files*.
- [ ] **F0.7** Crear un tablero de tareas (GitHub Projects o issues) usando las plantillas que ya hay en `.github/ISSUE_TEMPLATE/`.
- [ ] **F0.8** (Opcional) Instalar un editor de audio (**Audacity**) y de imagen (**Krita** o **GIMP**).

**Archivos:** `.gitattributes` (nuevo), `.gitignore` (revisar).

**Terminado cuando:** Unity abre el proyecto, Visual Studio reconoce los scripts, y un archivo `.png` de prueba subido a git aparece como LFS (`git lfs ls-files`).

**Riesgos:** si se abre el proyecto con otra versión de Unity, hay que reimportar todo y puede romper cosas → usar siempre la 2022.3.56f1.

---

[Índice](00_INDICE.md) · [F01 →](F01_recuperar_prototipo.md)
