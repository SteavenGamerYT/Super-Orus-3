# Super Orus 3

**Super Orus 3** is a fast-paced 3D first-person shooter built in Unity (Unity 6 / 6000.5.x).

---

## 🎮 Features

- **Dynamic FPS Gameplay:** Fast movement, jetpack mechanics, aiming, weapon switching, and responsive recoil.
- **Arsenal of Weapons:**
  - **Blaster:** Rapid-fire laser pistol with heat management.
  - **Shotgun:** Close-quarters heavy scatter weapon.
  - **Launcher / Disc Launcher:** Area-of-effect projectile launcher.
- **Smart AI & Enemies:**
  - Hover bots, turrets, and mobile ground units powered by Unity NavMesh navigation and state machines.
- **Custom HUD & UI:** TextMeshPro-powered health bars, crosshairs, ammo counters, compass radar, and notification toasts.
- **Multiple Scenes:**
  - `IntroMenu`: Start menu and game entry.
  - `MainScene`: Primary campaign/arena level.
  - `DemoScene`: Sandbox test arena with terrain, Redwood environments, and weapon pickups.
  - `WinScene` & `LoseScene`: Objective completion and game-over flows.

---

## 🛠️ Requirements & Setup

- **Unity Version:** Unity 6 (`6000.5.7f1` or later recommended)
- **Render Pipeline:** Built-in Render Pipeline (Linear Color Space)
- **Dependencies:** TextMesh Pro, Unity UI (`com.unity.ugui`), AI Navigation

### Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/SteavenGamerYT/Super-Orus-3.git
   ```
2. Open **Unity Hub**.
3. Click **Add** -> **Add project from disk** and select the repository root folder.
4. Launch the project using Unity 6 (6000.5.x).
5. Load `Assets/FPS/Scenes/IntroMenu.unity` or `Assets/FPS/Scenes/MainScene.unity` and press **Play**!

---

## 🕹️ Controls

| Action | Key / Input |
|---|---|
| **Move** | `W`, `A`, `S`, `D` |
| **Look** | `Mouse` |
| **Shoot** | `Left Mouse Button` |
| **Aim / Secondary** | `Right Mouse Button` |
| **Jump** | `Space` |
| **Jetpack** | `Space` (Hold while in air) |
| **Crouch** | `C` or `Left Ctrl` |
| **Sprint** | `Left Shift` |
| **Switch Weapons** | `1`, `2`, `3` or `Mouse ScrollWheel` |
| **Pause / Menu** | `Escape` |

---

## 📁 Project Structure

```
├── Assets/
│   ├── FPS/              # Core FPS game scenes, prefabs, and configuration
│   ├── GameObject/       # Prefabs for player, weapons, pickups, and VFX
│   ├── Material/         # Shaders and materials (PBR Standard)
│   ├── Mesh/             # 3D models and meshes
│   ├── Scripts/          # C# gameplay, AI, UI, and audio controllers
│   ├── TextMesh Pro/     # UI fonts, resources, and shaders
│   └── Texture2D/        # Textures and normal maps
├── Packages/             # Unity Package Manager manifest
└── ProjectSettings/      # Unity project and tag/layer settings
```

---

## 📄 License

This project is licensed under standard open-source terms. See project files for third-party asset licenses and attribution.
