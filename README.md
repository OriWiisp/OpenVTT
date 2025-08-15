# OpenVTT — Virtual Tabletop & Dungeon Master Script Tool

## Overview
OpenVTT is a powerful Windows application designed for Dungeons & Dragons Dungeon Masters. It combines a D&D-themed user interface with tools for managing scripts, running a virtual tabletop (2D & 3D), and editing maps.

## Features
- **Script Reader & Teleprompter**: Write or import your DM scripts in Markdown format with scene headings. Includes a teleprompter mode with adjustable speed, font size, mirror mode, fullscreen, and blackout.
- **2D Virtual Tabletop**: Run your game with a grid-based tabletop, draggable tokens, zoom, and map loading.
- **Map Editor**: Create custom maps with a tile painter, save them as `.ovttmap`, and load them directly into the 2D tabletop.
- **3D Virtual Tabletop**: A 3D grid plane with orbit and zoom controls, plus the ability to add simple 3D tokens.
- **D&D-Themed UI**: Parchment-style backgrounds and deep red accents for immersive gameplay.

## Installation
1. Install **.NET 8 SDK** and **Visual Studio 2022** or use the `dotnet` CLI.
2. Clone or download the repository.
3. Open the project in Visual Studio or run:
   
   ```bash
   dotnet build
   dotnet run
   ```

## Usage
1. **Scripts**: Start lines with `#` or `##` to define scenes. Select a scene from the sidebar to jump to it.
2. **Teleprompter**: Press **F5** or click *Start Teleprompter*. Use space to play/pause, arrow keys to adjust speed, `B` to blackout, and `F11` for fullscreen.
3. **2D Tabletop**: Add tokens, move them around, and snap to the grid. Open maps saved from the Map Editor.
4. **Map Editor**: Select tile types and click to paint. Save maps for use in the tabletop.
5. **3D Tabletop**: Orbit with left-drag, zoom with the mouse wheel, and reset the camera anytime.

## Hotkeys
- **F5**: Start Teleprompter
- **Space**: Play/Pause teleprompter
- **↑ / ↓**: Increase/Decrease teleprompter speed
- **B**: Blackout teleprompter
- **F11**: Fullscreen teleprompter
- **Ctrl+S**: Save script

## File Formats
- `.txt` / `.md`: Scripts
- `.ovttmap`: Map files

## License
MIT License — see `LICENSE` file for details.

---
For future updates, planned features include fog of war, line-of-sight, multiplayer sync, and an integrated dice roller.
