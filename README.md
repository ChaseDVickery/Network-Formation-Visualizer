# Network-Formation-Visualizer
By: Chase Vickery

### Table of Contents  
1. [Description](#description)
2. [Running](#running)
3. [Editing](#editing)
4. [Controls](#controls)
5. [Examples](#examples)

<a name="description"/></a>
## Description
A framework for basic network visualization and network formation game visualization.
Includes 2 models for networks: a connection model, and an arbitrary example model.
Includes 4 kinds of network formation games: random game, simultaneous game, dynamic game, and stochastic dynamic game.
Editable networks help users set initial conditions for network formation games.
Certain network and formation game parameters can be edited through their corresponding interfaces.

<a name="running"/></a>
## Running
Note: Executable testing was performed only on a Windows 10 machine. Source testing was performed in Unity Editor v2022.2.1f1.
### Windows
1. Download or clone this git repository and extract the zip file.
2. Navigate to the "Build > Windows > latest" subfolder.
3. Run the "Network Formation Visualizer.exe" file.
### Linux
1. Download or clone this git repository and extract the zip file.
2. Navigate to the "Build > Linux > latest" subfolder.
3. Run the 86_64 file (May need to allow executable permissions).
### Mac
1. Download or clone this git repository and extract the zip file.
2. Navigate to the "Build > macOS > latest" subfolder.
3. Run the program (May need to allow executable permissions).

<a name="editing"/></a>
## Editing (requires Unity)
1. Download or clone this git repository and extract the zip file.
2. Download and install Unity Hub
3. Open a project from disk and navigate to the folder, opening with Editor 2022.2.1f1.
4. Add the TextMeshPro package from the package manager.

<a name="controls"/></a>
## Controls
### Keyboard
- Space: Connects players (all or bipartite) depending on primary/secondary selection. Connects all selected agents if only primary selection is being used. Connects all agents between both selections if some are in the primary selection and some are in secondary selection. Adds a new agent if there are not agents selected.
- a: Connect all agents in primary selection.
- s: Connect agents between primary and secondary selections.
- r: Remove selected components (edges and agents).
- 1: Select all agents.
- 2: Select all edges.
- Ctrl (Hold): Keep current selection. New selections add or remove from selection.
### Mouse
- Left Click/Drag: Primary selection/area selection.
- Right Click/Drag: Secondary selection/area selection.
- Middle Click/Drag: Move camera.
- Middle Scroll: Zoom in/out.

<a name="examples"/></a>
### Examples
![](https://github.com/ChaseDVickery/Network-Formation-Visualizer/blob/main/videos/dynamic_move_agent.gif)
![](https://github.com/ChaseDVickery/Network-Formation-Visualizer/blob/main/videos/dynamic_weight_change.gif)
![](https://github.com/ChaseDVickery/Network-Formation-Visualizer/blob/main/videos/dynamic_delta_change.gif)
