# LinguaQuest: Project README

## Project Overview

**LinguaQuest** is a 2D educational RPG that aims to gamify cultural and language education through **Situated Learning**—integrating linguistic concepts directly into cultural contexts.

- **Engine:** Unity 6
- **Target Platform:** WebGL (itch.io)
- **Art Style:** Pixel Art
- **Key Research Metric:** PXI (Player Experience Inventory)

---

## File Structure & Workflow

The project follows a standard Unity modular structure. All development-related modifications occur within the **`Assets`** folder.

### 1. The "Inspector-First" Workflow

> [!IMPORTANT]
> **Technical Lead Note:** While the file structure is organized logically, the most efficient way to navigate the project is to **select a GameObject in the Hierarchy**. The Inspector will provide direct links to the associated Prefabs, Scripts, and ScriptableObjects, auto-navigating you to the correct folder.

### 2. Assets Directory Breakdown

- **`Animations/`**: Contains Animation Controllers and `.anim` clips for the player, NPCs, and environmental hazards (e.g., the "slipping" state logic).
- **`Art/`**: 16-bit pixel art assets, including tilemaps (Grass, Stone, Wood) and sprite sheets for character states.
- **`Music/` & `SFX/**`: Audio assets managed by the `StepSoundManager` and global audio controllers.
- **`Prefabs/`**: Reusable GameObjects including magic effects (Fireball, Lightning), UI panels, and the Player.
- **`Scripts/`**: The core logic of the game, organized by functional domain:
- `Player/`: Movement (`PlayerExploring.cs`), State Machines, and Combat.
- `Managers/`: Persistence and Global Logic (`SceneTracker.cs`, `BagManager.cs`).
- `ScriptableObjects/`: Data containers for decoupling logic (`Signal.cs`, `FloatValue.cs`, `VectorValue.cs`).

---

## Key Technical Systems

### ScriptableObject Architecture

To ensure clean data management and memory efficiency for WebGL:

- **Values**: `BoolValue`, `FloatValue`, and `VectorValue` allow for data persistence across scenes without heavy `DontDestroyOnLoad` reliance.
- **Signals**: The `Signal` and `SignalListener` system creates a decoupled observation pattern, allowing the Player to trigger UI updates (like health or coin counts) without direct references.

### Situated Learning Logic

The game utilizes a **JSON-based Content Loader**. This allows for the dynamic swapping of linguistic datasets and cultural dialogues, ensuring the game can be adapted for different languages without recompiling the codebase.

### Player State Machine

The player operates on a `PlayerState` enum (`walk`, `attack`, `interact`, `stagger`, `slip`). The **Slip Mechanic** is a specialized physics-based state that handles momentum and collision reflection, used to gamify environmental navigation.

---

## Technical Constraints (Unity 6 / WebGL)

1. **Memory Management**: Since the target is itch.io, textures must be compressed and audio should be set to "Compressed In Memory" or "Streaming" to avoid browser crashes.
2. **UI Scaling**: The Canvas Scaler is set to **0.5 Match (Width/Height)** to maintain 16-bit visual integrity across various browser window sizes.

---

## Academic Context

This project serves as a research tool to measure the efficacy of situated learning in a digital environment.

- **Primary Metric**: Player Experience Inventory (PXI).
- **Validation**: User study conducted with participants.
