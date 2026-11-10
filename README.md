# CMP6206 - Artificial Intelligence for Games

## 3D Warehouse Stealth AI System

**Institution:** Birmingham City University (BCU) / Universidad de Alcalá (UAH)  
**Module:** CMP6206 - Artificial Intelligence for Games  
**Engine:** Unity 6 (Built-in 3D Render Pipeline)  
**Language:** C#  
**Navigation Paradigm:** Custom Grid-Based Spatial Graph + Octile A* Pathfinding (Zero Unity NavMesh black-box dependency)

---

## 1. Project Overview & Core Mechanics

This project is a 3D tactical stealth and evasion prototype set in an industrial warehouse environment ($30 \times 30\text{ m}$). The player controls an infiltrator in first-person perspective, requiring situational awareness, posture management, and path prediction to complete mission objectives while evading autonomous security entities.

### Game Loop & Objectives
* **Objective / Win Condition:** Locate and retrieve **4 Security Keycards** scattered throughout the warehouse layout behind physical barriers and reach the designated **Extraction Zone** to escape.
* **Loss Condition (Game Over):** Getting within lethal interception range ($d < 1.2\text{ m}$) of any patrolling guard or tactical hunter triggers an immediate detection sequence, freezing player controls and prompting a scenario restart.

---

## 2. AI Architecture Comparison

To evaluate divergent decision-making paradigms in academic game AI, the warehouse is secured by two distinct architectural models sharing the same mathematical navigation core:

| Feature | Enemy Type A: Finite State Machine (FSM) | Enemy Type B: Behaviour Tree (BT) |
| :--- | :--- | :--- |
| **Paradigm** | Deterministic Finite State Machine (`switch-case`) | Hierarchical Composite Tree (`Tick()` / Task return states) |
| **Behavioral Role** | Routine Perimeter Patrol Guard | Tactical Central Hunter / Stalker |
| **State Flexibility** | Rigid single-state execution (`Patrol`, `Investigate`, `Chase`) | Priority-driven fallback (`Selector`, `Sequence`, `Decorator`) |
| **Sensory System** | Angular cone ($90^\circ$, $12\text{ m}$) with real-time Raycast FOV fan | Predictive ray-slicing + persistent search memory |
| **Pathfinding** | Real-time waypoint requests to custom $A^*$ engine | Reactive re-pathing and waypoint clearing |

---

## 3. Sensory Perception & Stealth Mechanics

* **Visual Cone & Occlusion:** Vision is evaluated mathematically using vector angles ($\theta \le 45^\circ$) and distance thresholds ($r \le 12\text{ m}$), verified against an occlusion layer mask (`Obstacle`). A multi-ray Gizmo fan displays real-time collision cuts against geometry in green/yellow, turning red upon target acquisition.
* **Acoustic Signature (Noise System):** The player's footsteps emit a dynamic noise radius evaluated against the AI's hearing threshold:
  * Stationary: $0.0\text{ m}$
  * Crouching: $1.5\text{ m}$
  * Sprinting/Walking: $8.0\text{ m}$

---

## 4. Implementation Roadmap & Session Breakdown

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
- [ ] **Milestone 3: Enemy Type B - Modular Behaviour Tree (BT)**
  - [ ] Abstract node hierarchy (`Node`, `Selector`, `Sequence`, `ActionNode`).
  - [ ] Hunter tree implementation (`CheckPlayerInFOV`, `TaskChase`, `TaskSearchLastKnown`, `TaskPatrol`).
  - [ ] Stalker archetype with search memory before returning to base route.
- [ ] **Milestone 4: Gameplay Loop, Keycards & UI**
  - [ ] Keycard pickup prefabs with rotation and trigger detection.
  - [ ] Lethal capture collider check with game-over reset sequence.
  - [ ] HUD Canvas (Keycard counter, objective tracker, detection status).
- [ ] **Milestone 5: Level Population, Optimization & Final Video Demonstration**
  - [ ] Multi-agent layout (2 FSM perimeter guards + 1 BT central hunter).
  - [ ] Material passes and aesthetic pass.
  - [ ] Dual-viewport recording (*Game* + *Scene* with Gizmos active) for academic viva/submission.