# Project Folder Structure

## Overview

This project uses **feature-based organization** built around **Unity Atoms** as the reactive data layer. Every gameplay system owns its assets in a self-contained folder. Shared/global assets live in `_Game/`. The structure is designed for a 4-person remote team to minimize merge conflicts, maximize searchability, and prevent asset clutter.

### Key Architectural Decision: Unity Atoms

Unity Atoms replaces traditional C# event buses, manager singletons, and direct references with ScriptableObject-based reactive data. This means:

- **Variables** (AtomVariables) are the single source of truth for all shared state
- **Events** (AtomEvents) are the only mechanism for cross-feature communication
- **Actions** (AtomActions) encapsulate reusable response logic as swappable SO assets
- **Conditions** (AtomConditions) encapsulate boolean logic as swappable SO assets
- **Lists** (AtomLists) manage runtime collections as observable SOs
- **FSM** (State Machines) define feature state via SO-based state/transition assets

All of these are ScriptableObject instances on disk. They live in `Atoms/` folders, never in `Scripts/`.

---

## Top-Level Layout

```
Assets/
├── _Game/          Shared project-wide systems, reactive data, and assets
├── _Features/      All gameplay features (one folder per feature)
├── _Debug/         Dev-only tools, test scenes, cheat systems
├── Art/            Raw/source art assets before feature integration
├── ThirdParty/     Store assets, plugins (read-only)
├── Settings/       HDRP, render pipeline, quality settings (Unity-managed)
└── HDRPDefaultResources/   (Unity-generated, do not modify)
```

The `_` prefix forces `_Game`, `_Features`, and `_Debug` to the top of Unity's Project window.

---

## _Game/ — Shared Project Foundation

Everything that multiple features depend on. This is the shared contract. Changing anything here affects the whole project.

```
_Game/
├── Atoms/                          THE reactive data layer for cross-feature communication
│   ├── Variables/                  Shared state read by multiple features
│   │                               Examples: GameState, IsPaused, MasterVolume, TimeScale
│   ├── Events/                     Cross-feature signals
│   │                               Examples: OnGamePaused, OnSceneTransition, OnPlayerDied
│   ├── Actions/                    Global response logic (swappable via Inspector)
│   │                               Examples: LogAction, SaveGameAction, AnalyticsAction
│   ├── Conditions/                 Global boolean checks (swappable via Inspector)
│   │                               Examples: IsGamePaused, IsPlayerAlive
│   ├── Lists/                      Shared runtime collections
│   │                               Examples: ActiveEnemiesList, ActiveProjectilesList
│   └── FSM/                        Global state machines
│                                   Examples: GameStateMachine (Menu→Loading→Playing→Paused)
│
├── Config/                         Non-reactive ScriptableObjects (static configuration)
│                                   Examples: BalanceConfig, DifficultySettings, BuildSettings
│                                   These do NOT change at runtime. Pure tuning data.
│
├── Audio/
│   ├── Music/                      Background tracks, stingers
│   ├── SFX/                        Shared sound effects (UI clicks, generic impacts)
│   └── Mixers/                     AudioMixer assets and snapshots
│
├── Input/                          Input Action assets, input action maps
│
├── Materials/                      Shared/global materials (skybox, post-processing)
│
├── Prefabs/                        Shared prefabs (damage numbers, pooled FX, loading screen)
│
├── Scenes/
│   ├── _Boot/                      Bootstrap/initialization scene (always loads first)
│   ├── MainMenu/                   Main menu scene and scene-specific assets
│   ├── Gameplay/                   Primary gameplay scene(s)
│   └── Test/                       Throwaway test scenes (per-dev scratchpads)
│
├── Scripts/
│   ├── Atoms/                      Custom Atom type DEFINITIONS (C# classes only)
│   │                               Generated types from Atom Generator go here.
│   │                               Example: HealthDataVariable.cs, HealthDataEvent.cs
│   │                               The SO instances created from these go in Atoms/ folders.
│   ├── Core/                       Bootstrap, GameManager, app lifecycle
│   ├── Extensions/                 C# extension methods (TransformExtensions, VectorExtensions)
│   ├── Interfaces/                 Shared interfaces (IDamageable, IInteractable, ISaveable)
│   └── Utilities/                  Helpers, math utils, object pooling, coroutine runners
│
├── Shaders/                        Global shaders, shader graphs, shader includes
│
├── Textures/                       Shared textures (noise maps, LUTs, gradient ramps)
│
├── UI/
│   ├── Fonts/                      Font assets, TMP font assets, font materials
│   ├── Icons/                      Shared icon sprites
│   ├── Sprites/                    Shared UI sprites, sprite atlases
│   └── Themes/                     UI style sheets, color palette ScriptableObjects
│
└── VFX/                            Shared particle systems, VFX Graph assets
```

### The Atoms vs Config Distinction

This is the most important organizational rule in the project:

| Folder | Contents | Mutates at Runtime | Observed/Reactive |
|--------|----------|-------------------|-------------------|
| `Atoms/` | AtomVariables, AtomEvents, AtomActions, AtomConditions, AtomLists, FSMs | Yes | Yes |
| `Config/` | Plain ScriptableObjects for tuning/configuration | No | No |

If a designer needs to tweak a value and see it change live in play mode via Atom listeners — it's an **Atom Variable**, goes in `Atoms/Variables/`.

If it's a static number baked into a system at startup (spawn rate, XP curve, item database) — it's a **Config SO**, goes in `Config/`.

---

## _Features/ — Gameplay Features

Every distinct gameplay system gets its own folder. Features are self-contained: scripts, atoms, config, prefabs, and art all live together.

```
_Features/
├── Player/
│   ├── Atoms/
│   │   ├── Variables/              PlayerHealth, PlayerPosition, PlayerMoveSpeed, IsGrounded
│   │   ├── Events/                 OnPlayerJump, OnPlayerDamaged, OnPlayerInteract
│   │   ├── Actions/                HealPlayerAction, KnockbackAction
│   │   ├── Conditions/             IsPlayerGrounded, HasEnoughStamina
│   │   ├── Lists/                  PlayerBuffsList, PlayerInventoryList
│   │   └── FSM/                    PlayerStateMachine (Idle→Run→Jump→Fall→Climb)
│   ├── Config/                     PlayerBaseStats (starting HP, base speed — static tuning)
│   ├── Scripts/                    PlayerController, PlayerMotor, PlayerAnimator
│   ├── Animations/
│   │   ├── Controllers/            Animator controllers
│   │   └── Clips/                  Animation clips
│   ├── Prefabs/                    Player prefab and variants
│   ├── Materials/                  Player-specific materials
│   ├── Models/                     Player model, rig
│   └── VFX/                        Player-specific effects (footsteps, trails)
│
├── Tablet/
│   ├── Atoms/
│   │   ├── Variables/              CurrentTabletText, IsTabletActive
│   │   ├── Events/                 OnTabletOpened, OnTabletClosed, OnCommandSubmitted
│   │   ├── Actions/                ProcessTabletCommandAction
│   │   ├── Conditions/             IsTabletUnlocked
│   │   └── FSM/                    TabletStateMachine (Off→Boot→Idle→Processing)
│   ├── Config/                     TabletDisplayConfig (font size, scroll speed)
│   ├── Materials/
│   ├── Textures/
│   └── _Features/                  Sub-features (complex feature decomposition)
│       ├── TabletCommandInput/
│       │   ├── Atoms/
│       │   │   ├── Variables/      CurrentInputBuffer, CursorPosition
│       │   │   └── Events/         OnInputChar, OnInputSubmit, OnInputClear
│       │   └── Scripts/
│       ├── TabletCommands/
│       │   ├── Atoms/
│       │   │   └── Actions/        MoveCommandAction, LookCommandAction, UseCommandAction
│       │   └── Scripts/
│       └── TabletDisplay/
│           ├── Atoms/
│           │   ├── Variables/      DisplayLines, ScrollOffset
│           │   └── Events/         OnDisplayRefresh
│           ├── Config/             DisplayConfig (max lines, colors)
│           ├── Prefabs/
│           └── Scripts/
│
├── Camera/
│   ├── Atoms/
│   │   ├── Variables/              CurrentCameraTarget, CameraZoom, CameraShakeIntensity
│   │   ├── Events/                 OnCameraShake, OnCameraTransition
│   │   ├── Actions/                ShakeCameraAction, TransitionCameraAction
│   │   └── Conditions/             IsCameraLocked
│   ├── Config/                     CameraConfig (follow speed, dead zone, look ahead)
│   ├── Profiles/                   Cinemachine profiles, volume overrides
│   └── Scripts/                    CameraController, CameraStates
│
├── AI/
│   ├── Atoms/
│   │   ├── Variables/              ActiveEnemyCount, AlertLevel
│   │   ├── Events/                 OnEnemySpawned, OnEnemyKilled, OnAlertTriggered
│   │   ├── Actions/                SpawnEnemyAction, DespawnEnemyAction
│   │   ├── Conditions/             IsPlayerDetected, IsInPatrolRange
│   │   ├── Lists/                  ActiveEnemiesList, PatrolPointsList
│   │   └── FSM/                    EnemyBrainFSM (Patrol→Alert→Chase→Attack→Flee)
│   ├── Config/                     EnemySpawnTable, PatrolRouteData, DifficultyScaling
│   └── Scripts/                    AIController, BehaviorTree nodes, Sensors
│
├── Environment/
│   ├── Atoms/
│   │   ├── Variables/              CurrentRoomId, AmbientLightLevel
│   │   ├── Events/                 OnDoorOpened, OnSwitchActivated, OnTrapTriggered
│   │   └── Actions/                ToggleDoorAction, ActivateTrapAction
│   ├── Config/                     RoomDatabase, InteractableConfig
│   ├── Scripts/                    Interactive objects, hazards, triggers
│   ├── Prefabs/                    Doors, switches, destructibles
│   ├── Materials/
│   ├── Models/
│   └── Textures/
│
└── UI/
    ├── HUD/
    │   ├── Atoms/
    │   │   ├── Variables/          DisplayedHealth, DisplayedAmmo, CompassBearing
    │   │   └── Events/             OnHUDShow, OnHUDHide
    │   ├── Config/                 HUDLayout (positions, sizes, visibility defaults)
    │   ├── Prefabs/
    │   └── Scripts/
    ├── PauseMenu/
    │   ├── Atoms/
    │   │   ├── Variables/          SelectedMenuIndex
    │   │   └── Events/             OnPauseToggled, OnSettingsChanged
    │   ├── Prefabs/
    │   └── Scripts/
    └── Inventory/
        ├── Atoms/
        │   ├── Variables/          SelectedSlotIndex, IsInventoryOpen
        │   ├── Events/             OnItemSelected, OnItemUsed, OnItemDropped
        │   └── Actions/            UseItemAction, DropItemAction, SwapSlotsAction
        ├── Config/                 InventoryConfig (max slots, stack sizes)
        ├── Prefabs/
        └── Scripts/
```

### Adding a New Feature

Minimum template:

```
_Features/[FeatureName]/
├── Atoms/
│   ├── Variables/
│   └── Events/
├── Scripts/
└── Prefabs/
```

Add more Atom subfolders (Actions/, Conditions/, Lists/, FSM/) and asset folders (Config/, Materials/, Models/, etc.) only when the feature needs them. Do not pre-create empty folders.

### When a Feature Needs Sub-Features

Use `_Features/` nesting only when a feature contains genuinely independent subsystems that have their own reactive data. The Tablet is a good example: CommandInput, Commands, and Display each have distinct state and events.

If sub-systems share all the same Atoms and only differ in scripts, they are not separate sub-features. Just use separate script files in the parent `Scripts/` folder.

---

## _Debug/ — Development Tools

```
_Debug/
├── Scripts/        Cheat console, debug overlays, gizmo drawers, Atom inspectors
├── Scenes/         Test harness scenes for isolated feature testing
└── Prefabs/        Debug-only prefabs (FPS counter, collision visualizers)
```

Must be excluded from production builds via assembly definitions with Editor-only platform constraints or `#if UNITY_EDITOR` / `#if DEVELOPMENT_BUILD` guards.

---

## Art/ — Asset Staging Area

Raw art assets land here first. When a developer integrates an asset into a feature, they move it into the appropriate `_Features/[Feature]/` folder.

```
Art/
├── Models/
│   ├── Characters/
│   ├── Props/
│   └── Environment/
├── Textures/
│   ├── Characters/
│   ├── Props/
│   └── Environment/
├── Animations/
│   ├── Characters/
│   └── Props/
└── Concepts/       Reference images, mood boards (add to .gitignore)
```

---

## ThirdParty/ — External Assets

```
ThirdParty/
├── Custom Inspector/
└── [OtherAsset]/
```

---

## Rules

### 1. Atoms Are the Data Layer — No Exceptions

All runtime state lives in AtomVariables. All cross-feature signals go through AtomEvents. No C# static events, no singleton getters, no direct component references between features.

The reactive data flow is:

```
Feature A writes to AtomVariable → AtomEvent fires → Feature B responds via AtomAction/Listener
```

If you find yourself writing `FindObjectOfType`, `GetComponent` across features, or a static event bus — stop. Create an Atom.

### 2. Atoms/ vs Config/ Is a Hard Boundary

- `Atoms/` = reactive, runtime-mutable, observable ScriptableObjects (Variables, Events, Actions, Conditions, Lists, FSM)
- `Config/` = static, design-time-only ScriptableObjects (tuning data, databases, curves, tables)

A Config SO is read once at initialization. An Atom Variable is read and written continuously and triggers listeners when changed. Never mix them.

### 3. Scripts/ vs Atoms/ — Code vs Instances

- `Scripts/` contains C# source files: MonoBehaviours, custom AtomAction class definitions, custom AtomCondition class definitions, custom Atom type definitions, utilities
- `Atoms/` contains ScriptableObject asset instances created from those classes via `Create Asset` menu

The class `HealPlayerAction : AtomAction<int>` lives in `_Features/Player/Scripts/`.
The SO instance `HealPlayer_10HP.asset` created from that class lives in `_Features/Player/Atoms/Actions/`.

### 4. Global vs Feature Atoms — Ownership Rules

| Location | Who creates | Who reads | Who writes |
|----------|------------|-----------|------------|
| `_Game/Atoms/` | Team consensus | Any feature | Designated owner only |
| `_Features/[X]/Atoms/` | Feature owner | That feature (primarily) | That feature only |

A feature may **read** another feature's Atom Variables or **listen** to another feature's Atom Events — but it must never **write** to them. If Feature B needs to cause a state change in Feature A, Feature B raises a global event in `_Game/Atoms/Events/` that Feature A listens to.

Exception: `_Game/Atoms/Variables/` for truly shared state (GameState, IsPaused) — ownership is documented per-variable.

### 5. Feature Folders Are Self-Contained

A feature folder carries its own scripts, atoms, config, and prefabs. The deletion test: removing a feature folder should only break references to that feature's Atoms from other features (which are loose SO references easily spotted in the Inspector as missing). It must never cause compile errors in other features.

### 6. No Loose Files in Assets Root

Every file must live inside an appropriate folder. Nothing sits directly in `Assets/` except this document and Unity-generated files that cannot be moved.

### 7. ThirdParty Is Read-Only

Never modify files inside `ThirdParty/`. Write wrapper scripts in `_Game/Scripts/` or the relevant feature folder.

### 8. One Scene Per Folder

Each scene gets its own subfolder containing the `.unity` file and scene-specific baked data (lighting, navmesh, occlusion).

### 9. _Debug Gets Stripped from Builds

All scripts in `_Debug/` must use assembly definitions with Editor-only platform constraints or `#if UNITY_EDITOR` / `#if DEVELOPMENT_BUILD` guards.

### 10. _Game Changes Require Team Communication

`_Game/` is the shared contract. Before modifying `_Game/Atoms/`, `_Game/Scripts/Interfaces/`, or `_Game/Config/`, communicate the change to the team. Breaking changes here break everyone.

### 11. Art/ Is a Staging Area, Not Permanent Storage

Assets in `Art/` are awaiting integration. Once placed into a feature folder, the asset is removed from `Art/`.

### 12. Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Folders | PascalCase | `TabletDisplay` |
| Scripts | PascalCase matching class name | `PlayerController.cs` |
| Atom Variables | PascalCase descriptive name | `PlayerHealth.asset` |
| Atom Events | PascalCase with On prefix | `OnPlayerDamaged.asset` |
| Atom Actions | PascalCase with Action suffix | `HealPlayerAction.asset` |
| Atom Conditions | PascalCase descriptive | `IsPlayerGrounded.asset` |
| Atom Lists | PascalCase with List suffix | `ActiveEnemiesList.asset` |
| Atom FSM | PascalCase with FSM/State suffix | `PlayerStateMachine.asset` |
| Config SOs | PascalCase with Config suffix | `PlayerBaseConfig.asset` |
| Prefabs | PascalCase | `PlayerCharacter.prefab` |
| Materials | PascalCase | `TabletScreen.mat` |
| Textures | PascalCase with channel suffix | `TabletScreen_Albedo.png` |
| Animations | PascalCase with subject prefix | `Player_Idle.anim` |
| Scenes | PascalCase | `MainMenu.unity` |

### 13. Assembly Definitions

Each major folder should have an assembly definition (`.asmdef`) to enforce dependency direction and speed up compile times:

- `_Game/Scripts/` — `Game.Core.asmdef` (depends on Unity Atoms assemblies, nothing else)
- `_Game/Scripts/Atoms/` — `Game.Atoms.asmdef` (depends on Game.Core + Unity Atoms)
- `_Features/[Feature]/Scripts/` — `Game.[Feature].asmdef` (depends on Game.Core + Game.Atoms)
- `_Debug/Scripts/` — `Game.Debug.asmdef` (Editor-only, can depend on anything)

Features must **never** reference other feature assemblies directly. Cross-feature communication goes exclusively through Atom SO references wired in the Inspector.

### 14. Sub-Features Are Optional Depth

Only nest `_Features/` inside a feature when it has genuinely independent subsystems with their own reactive data (like Tablet). One level of nesting is the maximum. Simple features do not need sub-features.
