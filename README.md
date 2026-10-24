# CMP6206: Artificial Intelligence for Games
## Coursework Project: 3D Tactical Infiltration & Multi-Agent Stealth Simulation

**Student Name:** Pablo Martín Navarro  
**Institution:** Birmingham City University (BCU)  
**Module:** CMP6206 - Artificial Intelligence for Games (Semester 1, 2026/2027)  
**Module Leader:** Jan Krasniewicz  

---

### 1. Game Concept & Core Reference: "3D Pac-Man Infiltration"
This project is an adversarial 3D tactical infiltration simulation developed in Unity. The player navigates a warehouse environment, utilizing cover and obstacles to avoid detection by autonomous security agents.

**Core Academic Reference (Pac-Man 3D Mechanics):**  
Taking inspiration from the foundational multi-agent AI design of *Pac-Man* (Toru Iwatani, 1980), the simulation does not rely on identical enemy units. Instead, it features two distinct ground-based enemy archetypes with asymmetric decision-making, sensing profiles, and target-selection mathematics:
* **The Patrol Guard (Blinky archetype):** Direct pursuit agent. Tracks the player's exact current grid location using shortest-path traversal.
* **The Interceptor Guard (Pinky archetype):** Predictive ambush agent. Calculates the player's forward trajectory vector and routes towards future junction nodes to cut off escape routes.

---

### 2. Formal PEAS Specification (Russell & Norvig Framework)
* **Performance Measure (P):**
  * *Detection Latency:* Time taken from line-of-sight acquisition to alert state.
  * *Search Efficiency:* Speed of clearing last known player coordinates ($LKP$).
  * *Interception Rate:* Successful ambushes executed by coordinated flanking.
* **Environment (E):**
  * 3D warehouse floor with static and dynamic physical barriers (crates, walls, corridors).
  * Characteristics: *Partially Observable* (occlusion by geometry), *Dynamic* (player moves in real-time), *Continuous* (physics-based vectors), *Multi-Agent* (adversarial dynamic between player and guards).
* **Actuators (A):**
  * Vector-based translation and rotation (`Transform`, `Time.deltaTime`).
  * Field-of-View sensory feedback (Visual cone debug lines: Green = Clear, Yellow = Occluded, Red = Alert).
  * State visualizers and radio broadcast alerts between ground units.
* **Sensors (S):**
  * *Conical Ray Bundle:* 11-ray horizontal fan spanning 90° FOV using `Physics.Raycast` for line-of-sight and physical occlusion.
  * *Acoustic Proximity:* Spherical trigger check (`Physics.OverlapSphere`) detecting rapid player footstep noise.

---

### 3. Dual-NPC Ground AI Specification

| Feature | Enemy Archetype 1: Patrol Guard (*Blinky*) | Enemy Archetype 2: Interceptor Guard (*Pinky*) |
| :--- | :--- | :--- |
| **Primary Role** | Direct patrol & chasing | Flanking & ambush prediction |
| **Target Computation** | Exact current player node ($\vec{P}_{player}$) | Predicted forward node ($\vec{P}_{future} = \vec{P} + \vec{v} \cdot t$) |
| **Pathfinding Engine** | Uniform-Cost Search (UCS / Dijkstra) | Uniform-Cost Search (UCS) biased towards intersections |
| **Decision Architecture** | Classical State Machine (Patrol, Investigate, Chase) | Stack-Based FSM with return memory (Ambush, Search) |
| **Primary Sensor** | 90° Conical Raycast Bundle (High range) | Narrow FOV + Acoustic proximity radius |

---

### 4. Implementation Roadmap & Milestones

* **Phase 1 (Weeks 4–5) — Spatial Discretization & Uninformed Pathfinding:**
  * [x] *Milestone 1:* Project foundation, formal PEAS documentation, greybox prototype, vector movement, and raycast sensor.
  * [ ] Discretize warehouse ground into a 2D grid graph (`Node.cs`, `Grid.cs`) with physical unwalkable masks.
  * [ ] Implement custom **Uniform-Cost Search (UCS / Dijkstra)** to navigate weighted terrain without Unity's NavMesh.
* **Phase 2 (Weeks 6–7) — Finite State Machine (FSM) Decision Engine:**
  * [ ] Implement State Pattern for Patrol Guard: `PatrolState`, `InvestigateState`, `ChaseState`.
  * [ ] Integrate hearing triggers and last-known-position memory.
* **Phase 3 (Weeks 8–9) — Predictive Interceptor AI:**
  * [ ] Build vector prediction logic calculating the player's future trajectory.
  * [ ] Implement Stack-Based FSM for ambush behaviors and tactical search routines.
* **Phase 4 (Weeks 10–11) — Multi-Agent Coordination & Alert Broadcasts:**
  * [ ] Inter-agent communication system: Guards relay player positions and coordinate flanking routes.
* **Phase 5 (December / January) — Evaluation, Report & Final Submission:**
  * [ ] Performance benchmarking and algorithmic complexity evaluation.
  * [ ] Technical Report (Word/PDF) and 10-minute video walkthrough defense.
