# The Escape Room: An Interactive VR Game

## Project Information
*   **Team Name:** The Escape Room
*   **Team Members:** Erica Liu (kl3734), Kusuma Jaipiam (kj2634), Shanshan Wu (sw3833), Zian Zhang (zz3402)
*   **Date of Submission:** May 12, 2026
*   **Development Platform(s):** Unity 6.3 LTS
*   **Mobile Platforms/Devices:** Meta Quest 2, Meta Quest 3, Macbook Air M2 (Mac OS 15.7.3), **[TO BE ADDED MORE]**
*   **Video URL:** **[TO BE INSERTED]**

## Project Directory Overview

### Assets/Scenes/
Contains the main scene `Escape Room.unity` where the full experience is built, alongside `BasicScene` used for isolated feature testing and prototyping during development.

### Assets/Scripts/
Contains all custom C# scripts organized into subdirectories by function.
*   **Puzzle/:** Logic for individual puzzle mechanics including the padlock, buzz wire, bookshelf, and simultaneous object placement.
*   **Interaction/:** Scripts handling player input, grabbing, and object interactables.
*   **UI/:** Scripts for the minimap, timer, pop-up panels, and HUD elements.
*   **Room/:** Scripts managing room state, gating, and transitions between the two rooms.
*   **Progress/:** Scripts tracking overall game state and win condition.
*   **Transport/:** Scripts for player teleportation and travel.
*   **Experiment/ and Debug/:** Scripts used during development and testing.
*   **Editor/:** Any custom Unity Editor scripts used to assist development.

### Assets/Prefabs/
Contains all prefabricated GameObjects organized into subdirectories.
*   **Puzzles/:** Prefabs for the padlock, buzz wire, chest, bookshelf, and placement targets.
*   **Primitives/:** Basic shape prefabs used across puzzles.
*   **Object Placeholders/:** Objects retrieved from the chest used in the final puzzle.
*   **Mini Map/:** Minimap camera and display prefabs.
*   **UI/:** All World Space canvas prefabs for pop-up panels and HUD elements.

## Deployment and Targets
*   **Special Instructions for Deploying:** N/A
*   **Special Instructions for Preparing Targets:** N/A

### Missing Features
*   Size change portal
*   Removed last planned puzzle of holding 2 objects

### Bugs
**[TO BE ADDED MORE]**

### Asset Sources

*   **Past Room (table, chair, portrait):** https://sketchfab.com/3d-models/old-room-6173a3c88c384f768dfc80967b6527b4
*   **Past Room's Vase with flowers:** https://sketchfab.com/3d-models/flowers-in-vase-b1047276fc7f4421b5f695ad9ff59e72#download
*   **Key:** https://sketchfab.com/3d-models/key-ad78fc71092849ca9cd2f264e14a8167
*   **Wire:** https://sketchfab.com/3d-models/barbed-wire-v2-c225c5e9ace0499690809858330ffbd0
*   **Door:** https://sketchfab.com/3d-models/wood-door-old-3d-scan-f66689cc6b884820beae3eb37b2b016c
*   **Future Room's Table:** https://sketchfab.com/3d-models/future-table-939df479c3f948dba03f49f45b9af6cd
*   **Future Room's Vase:** https://sketchfab.com/3d-models/cheap-gold-metallic-vase-fd4693c234b74eca87b978bb330e1963
*   **Future Room's Bookshelf:** https://sketchfab.com/3d-models/bookshelf-6cdeb78cd91d49ed87434239f95b5544#download

