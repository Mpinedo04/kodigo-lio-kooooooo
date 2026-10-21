# Estructura de carpetas objetivo

> [Índice](../00_INDICE.md)


```
Assets/
├── ArtSources/            # Notas, licencias, fuentes .blend (ya existe)
├── Audio/
│   ├── Music/
│   ├── SFX/
│   └── Mixers/
├── Characters/            # Odd, Ulrich, Yumi, Aelita, William, versiones mundo real
│   └── Odd/ (Models, Materials, Textures, Animations, Odd.prefab)
├── Enemies/               # Un subcarpeta por monstruo
├── Vehicles/
├── Environments/
│   ├── Lyoko/ (Forest, Desert, Ice, Mountain, Sector5, DigitalSea, Replikas, Cortex)
│   └── RealWorld/ (Factory, Kadic, Hermitage, City)
├── Game/
│   ├── Data/              # ScriptableObjects (ya existe)
│   ├── Editor/            # Herramientas de editor (ya existe)
│   ├── Input/
│   └── Scripts/
│       ├── Core/          # GameManager, SceneLoader, SaveSystem, ObjectPool
│       ├── Events/
│       ├── Player/        # Controladores, cámara
│       ├── Combat/
│       ├── Abilities/ (Odd, Ulrich, Yumi, Aelita, William)
│       ├── Enemies/ (Brain, States, por monstruo)
│       ├── Vehicles/
│       ├── Mission/
│       ├── Operator/
│       ├── RealWorld/
│       ├── Story/         # Diálogos, flags, cinemáticas
│       ├── Progression/
│       ├── UI/
│       ├── Audio/
│       └── Debug/
├── Prefabs/
├── Scenes/
│   ├── Boot.unity
│   ├── MainMenu.unity
│   ├── Lyoko/
│   ├── RealWorld/
│   └── Test/ (MovementGym, CombatArena, EnemyGym, VehicleTrack)
├── Shaders/
├── VFX/
└── Tests/
Docs/
├── BIBLIA_DEL_JUEGO.md
├── Fases/ (00_INDICE.md, 00_REGLAS.md, F00…F26, anexos/)
└── Guiones/ (Prologo.md, Capitulo1.md…)
```
