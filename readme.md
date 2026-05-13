# The Escape Room: An Interactive VR Game

## Project Information
*   **Team Name:** The Escape Room
*   **Team Members (Names and UNIs):** Erica Liu (kl3734), Kusuma Jaipiam (kj2634), Shanshan Wu (sw3833), Zian Zhang (zz3402)
*   **Date of Submission:** May 12, 2026
*   **Development Platform(s):** Unity 6.3 LTS
*   **Mobile Platforms/Devices:** Macbook Air M2 (Mac OS 15.7.3), Macbook Pro M5 (Tahoe 26.3.1), Windows 11
*   **Video URL:** [Insert Video Link Here]

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
The mini-map marker detection collider sometimes interefere with each other across clues/puzzles. If the player stands in the overlapping area of two different clue detection colliders, inspecting one might trigger markers for both clues to show up on minimap at the same time.
For the Buzz-the-wire game, the player could technically cheat by sinking the key under the table.

### Asset Sources

*   **Old Room (table, chair, portrait):** https://sketchfab.com/3d-models/old-room-6173a3c88c384f768dfc80967b6527b4
*   **Vase with flowers:** https://sketchfab.com/3d-models/flowers-in-vase-b1047276fc7f4421b5f695ad9ff59e72#download
*   **Key:** https://sketchfab.com/3d-models/key-ad78fc71092849ca9cd2f264e14a8167
*   **Wire:** https://sketchfab.com/3d-models/barbed-wire-v2-c225c5e9ace0499690809858330ffbd0
*   **Door:** https://sketchfab.com/3d-models/wood-door-old-3d-scan-f66689cc6b884820beae3eb37b2b016c
*   **Future table:** https://sketchfab.com/3d-models/future-table-939df479c3f948dba03f49f45b9af6cd
*   **Future vase:** https://sketchfab.com/3d-models/cheap-gold-metallic-vase-fd4693c234b74eca87b978bb330e1963
*   **Bookshelf:** https://sketchfab.com/3d-models/bookshelf-6cdeb78cd91d49ed87434239f95b5544#download
*   **Player Icon:** https://assetstore.unity.com/packages/3d/props/low-poly-3d-icons-pack-lite-295587
*   **Chest:** https://assetstore.unity.com/packages/3d/props/leathertrunk-295428
*   **Treasure:** https://assetstore.unity.com/packages/3d/props/medieval-gold-14162
*   **Past Key:** 
https://sketchfab.com/3d-models/key-a4aca11a2259462f8735a60eead33962
