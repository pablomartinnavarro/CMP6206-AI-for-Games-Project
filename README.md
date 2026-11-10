# CMP6206 - Artificial Intelligence for Games

## 3D Warehouse Stealth AI System

**Institution:** Birmingham City University (BCU) / Universidad de Alcalá (UAH)  
**Module:** CMP6206 - Artificial Intelligence for Games  
**Engine:** Unity 6 (Built-in 3D Render Pipeline)  
**Language:** C#  
**Navigation Paradigm:** Custom Grid-Based Spatial Graph + Octile A* Pathfinding (Zero Unity NavMesh black-box dependency)

---

## 1. Project Overview & Gameplay Loop

This project implements a 3D tactical stealth and evasion prototype developed in Unity 6 within a closed warehouse environment ($30 \times 30\text{ m}$). The player controls an infiltrator from a first-person perspective, requiring situational awareness, posture management, and path prediction to complete mission objectives while evading autonomous security entities.

### Core Objectives
* **Objective / Win Condition:** Locate and retrieve **4 Security Keycards** scattered throughout the warehouse layout behind physical barriers and reach the designated **Extraction Zone** to escape.
* **Loss Condition (Game Over):** Entering the lethal capture radius ($d < 1.2\text{ m}$) of any patrolling guard or tactical hunter triggers an immediate detection sequence, freezing player controls and prompting a scenario restart.

---

## 2. AI Architecture Comparison

To evaluate divergent decision-making paradigms in academic game AI, the warehouse is secured by two distinct architectural models sharing the same mathematical navigation core:

| Feature | Enemy Type A: Finite State Machine (FSM) | Enemy Type B: Behaviour Tree (BT) |
| :--- | :--- | :--- |
| **Paradigm** | Deterministic Finite State Machine (`switch-case`) | Hierarchical Composite Tree (`Tick()` / Task return states) |
| **Behavioral Role** | Routine Perimeter Patrol Guard | Tactical Central Hunter / Stalker |
| **State Flexibility** | Rigid single-state execution (`Patrol`, `Investigate`, `Chase`) | Priority-driven fallback (`Selector`, `Sequence`) |
| **Memory / Data** | Internal class variables (`lastKnownPosition`) | Shared blackboard dictionary (`SetData` / `GetData`) |
| **Sensory System** | Angular cone ($90^\circ$, $12\text{ m}$) with real-time Raycast FOV fan | Real-time multi-ray slicing + occlusion masking |
| **Pathfinding** | Real-time waypoint requests to custom $A^*$ engine | Reactive re-pathing and waypoint clearing |

---

## 3. Spatial Discretization & Custom Navigation Engine

In compliance with academic guidelines prohibiting Unity's built-in `NavMesh` / `NavMeshAgent` black-boxes, navigation is executed through an autonomous grid and pathfinding architecture:

* **Grid Discretization (`GridSystem.cs`):** Maps the $30 \times 30\text{ m}$ environment into an array of discrete nodes ($900$ cells, node diameter $= 1.0\text{ m}$). Cell passability is evaluated at startup using `Physics.CheckSphere` against the `Obstacle` layer. Continuous world coordinates map to matrix coordinates in $O(1)$ time complexity.
* **Node Data Structure (`PathNode.cs`):** Lightweight data container tracking matrix coordinates, world coordinates, walkability flag, parent pointers for path reconstruction, and evaluation costs:
  $$f(n) = g(n) + h(n)$$
* **$A^*$ Pathfinding Solver (`Pathfinding.cs`):** Computes optimal trajectories using Open/Closed sets and an Octile distance heuristic for 8-directional movement:
  $$h(n) = 14 \cdot \min(\Delta x, \Delta y) + 10 \cdot |\Delta x - \Delta y|$$

---

## 4. Behaviour Tree Architecture (Enemy Type B)

The Tactical Hunter (`EnemyBT.cs`) evaluates a modular, tick-based composite tree structure in C# without third-party frameworks. Every node returns one of three discrete states: `SUCCESS`, `FAILURE`, or `RUNNING`.

                   [ ROOT: Selector ]
                     /            \
                    /              \
       [ Sequence: Chase ]        [ TaskPatrol ]
         /                \
[ CheckPlayerInVision ]  [ TaskChase ]

* **`Selector` (Root / Priority OR):** Ticks the aggressive chase sequence first. If target acquisition fails, falls back immediately to the patrol task.
* **`Sequence` (Target Pursuit / AND):** Requires active line-of-sight confirmation before executing path generation.
* **`CheckPlayerInVision` (Condition Leaf):** Performs mathematical angle and distance checks, followed by physical occlusion raycasts against the `Obstacle` layer. Upon target acquisition, stores the player's reference into the shared tree blackboard (`parent.SetData("target", playerTransform)`).
* **`TaskChase` (Action Leaf):** Queries the custom $A^*$ solver (`Pathfinding.cs`) at controlled intervals ($0.3\text{ s}$) to pursue moving targets dynamically.
* **`TaskPatrol` (Action Leaf):** Traverses the assigned waypoint route (`WP_BT_1` through `WP_BT_3`) and automatically increments targets upon waypoint boundary entry.

---

## 5. Sensory Perception & Stealth Mechanics

* **Visual Cone & Occlusion:** Vision is evaluated mathematically using vector angles ($\theta \le 45^\circ$) and distance thresholds ($r \le 12\text{ m}$), verified against an occlusion layer mask (`Obstacle`). Multi-ray Gizmo fans display real-time geometry collision cuts in green/yellow/cyan, turning red/magenta upon target acquisition.
* **Acoustic Signature (Noise System):** The player's footsteps emit a dynamic noise radius evaluated against the AI's hearing threshold:
  * Stationary: $0.0\text{ m}$
  * Crouching: $1.5\text{ m}$
  * Sprinting/Walking: $8.0\text{ m}$

---

## 6. Implementation Roadmap & Session Breakdown

- [x] **Milestone 1: Core Greybox & Spatial Discretization**
  - [x] $30 \times 30\text{ m}$ warehouse layout with perimeter walls and occlusion crates (`Obstacle` layer).
  - [x] First-person stealth controller with calibrated crouch collider offsets and dynamic noise emission.
  - [x] Custom grid matrix generation (`GridSystem.cs`, $900$ discrete nodes, $O(1)$ coordinate mapping).
  - [x] Custom $A^*$ algorithm (`Pathfinding.cs`) using Octile heuristic ($10/14$) and backtracking reconstruction.
- [x] **Milestone 2: Enemy Type A - Finite State Machine (FSM)**
  - [x] Cyclic waypoint patrol circuit (`WP_1`, `WP_2`, `WP_3`) with arrival tolerances.
  - [x] Dynamic vision cone with visual raycast fan cutting against geometry.
  - [x] Acoustic detection triggering investigation state with local scanning rotation.
  - [x] Continuous path pursuit through custom $A^*$ traversal.
- [x] **Milestone 3: Enemy Type B - Modular Behaviour Tree (BT)**
  - [x] Abstract node hierarchy (`BTNode`, `Selector`, `Sequence`).
  - [x] Blackboard data context for decoupled inter-node communication.
  - [x] Condition leaf (`CheckPlayerInVision`) with geometric/physical line-of-sight checks.
  - [x] Action leaves (`TaskPatrol`, `TaskChase`) consuming custom $A^*$ waypoints.
  - [x] Integrated `EnemyBT` execution script and independent patrol route setup.
- [ ] **Milestone 4: Gameplay Loop, Keycards & UI**
  - [ ] Keycard pickup prefabs with rotation and trigger detection.
  - [ ] Lethal capture collider check with game-over reset sequence.
  - [ ] HUD Canvas (Keycard counter, objective tracker, detection status).
- [ ] **Milestone 5: Level Population, Optimization & Final Video Demonstration**
  - [ ] Multi-agent layout (2 FSM perimeter guards + 1 BT central hunter).
  - [ ] Material passes and aesthetic pass.
  - [ ] Dual-viewport recording (*Game* + *Scene* with Gizmos active) for academic viva/submission.