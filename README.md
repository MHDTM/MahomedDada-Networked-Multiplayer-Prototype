# MahomedDada-Networked-Multiplayer-Prototype
Local Network multiplier 2D prototype game 

# How to Run
- Open in **Unity 2022.3 LTS** or later.  
- Load the scene: `Scenes/Main Menu`.  
- Alternatively, run the **build** included in the submission zip.  
- Use two instances on the same network (one as Host, one as Client).

## Controls
- **A / D** – Move Left / Right  
- **Spacebar** – Jump  
- **Left Shift** – Throw Shuriken  
- **Escape** – Pause Menu (Return to Menu / Quit options)  

## Features
- Peer-to-peer networking using **Unity Netcode for GameObjects**.  
- Host and Client setup for local multiplayer.  
- Network-synced player movement, jumping, and attacking.  
- Independent player camera for each instance.  
- Working respawn system (5-second delay).  
- Functional pause menu with UI access to main menu and quit.  
- Animated character movement and attack synced over the network.  
- Background music and jump sound effects.  
- Tutorial-style level layout showcasing core mechanics.

## Notes
- The pause menu currently allows movement but provides functional menu access (to be refined in future updates).  
- Networking works best when all players are on the same LAN/Wi-Fi.  
- The Unity project includes a `.gitignore` to reduce repository size.

## Instructor
- Monique Butler