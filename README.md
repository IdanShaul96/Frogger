# Frogger

A remake of the 1981 arcade classic *Frogger*, built in Unity 6 (URP 2D).

Guide your frog from the bottom of the screen to the five homes at the top. First cross a road full of traffic, then a river where the only safe footing is drifting logs and turtles. Fill all five homes to clear the round. Each new round is faster than the last.

> Full design details are in the [Game Design Document](Docs/GDD.md).

---

## How to play

### Goal

Get a frog into each of the **five home slots** in the hedge at the top of the board. When all five are filled, the round is cleared, the homes empty, and the next round starts with faster lanes. Your score carries over between rounds. Keep going until you run out of lives, and try to beat the high score.

### Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Hop up / down / left / right | **W A S D** or **Arrow keys** | D-pad, or flick the left stick |
| Start game / play again | **Enter** | South button (A / ✕) |
| Quit | **Esc** (title and game-over screens only) | — |

- One key press is **exactly one hop** of one grid cell. Holding a key does not keep hopping: release it and press again.
- There are no diagonal hops.
- A press made during a hop is remembered for one more hop, so quick presses are never lost.

### The board

From bottom to top:

1. **Start strip.** The frog appears here.
2. **Road.** Five lanes of cars, trucks, tractors and race cars. Touching any vehicle kills the frog.
3. **Median.** A safe strip between the road and the river.
4. **River.** Five lanes of logs and turtles. The water itself is deadly. While standing on a log or turtle, the frog rides along with it.
5. **Home row.** A hedge with five open slots.

Every lane moves at a constant speed with constant spacing, and neighbouring lanes move in opposite directions. Nothing is random: if you die, you misjudged a gap.

### Ways to lose a life

- Getting hit by a vehicle.
- Landing in the water, or standing on turtles when they dive.
- Being carried off the edge of the screen by a log or turtle.
- Hopping into the hedge, or into a home that is already filled.
- The **30-second timer** running out. The timer resets for every life. At 5 seconds left, the numbers turn red and an alarm sounds.

### Diving turtles

Turtles are platforms, not enemies. Every turtle group repeats the same cycle:

1. **Surfaced** (5 s): safe to stand on.
2. **Half-submerged** (1.5 s): still safe. This is your warning to get off.
3. **Fully submerged** (2 s): the group counts as open water, and a frog standing on it drowns.

Groups in the same lane are out of step with each other, so the whole lane is never underwater at once.

### Lives

You start with **3 lives**, shown as frog icons in the top-right corner. After a death there is a short pause, then the frog reappears at the start with a fresh 30-second timer. When the last life is lost, the board freezes and the game-over screen appears.

### Scoring

| Event | Points |
|---|---|
| Reaching a row you have not reached before in this life | +10 |
| Filling a home slot | +50 |
| Time bonus when filling a home | +10 per second left on the timer |
| Clearing the round (all five homes filled) | +1000 |

Hopping backwards and then forward again over the same rows scores nothing. The **high score** is saved between sessions and shown on the title and game-over screens.

---

## Running the project

### Requirements

- **Unity 6000.3.20f1** (Unity 6). Other Unity 6 versions will probably work but have not been tested.
- The project uses the Universal Render Pipeline (2D Renderer), the Input System, and TextMeshPro. All three come in through the package manifest.

### Opening and playing

1. Clone the repository:
   ```bash
   git clone https://github.com/IdanShaul96/Frogger.git
   ```
2. Open the folder in Unity Hub with Unity 6000.3.20f1.
3. Open the scene `Assets/Scenes/Frogger.unity`.
4. Press **Play**. For the most accurate picture, set the Game view to **Full HD (1920×1080)**.

### Building

Open **File → Build Profiles**, choose **Windows**, and build. `Frogger.unity` is already in the build list. The game runs fullscreen at the monitor's resolution.

---

## Display and resolution

The board is a **14 × 16 grid** of 16-pixel cells, which is 224 × 256 pixels of art. A Pixel Perfect Camera scales the board up to fill the height of the screen (×4.2 at 1080p) and centres it, with black bars on the sides. A wider monitor never shows more of the board, so the game plays the same on every screen. The HUD is part of the board, so it always lines up with it.

---

## Technical overview

| System | Where | What it does |
|---|---|---|
| State machine | `GameManager` | Explicit states (Title, Playing, LifeLost, RoundClear, GameOver). Owns score, lives and the per-life timer. |
| Object pooling | `LaneController`, `LanePool` | Each lane creates all of its vehicles or platforms when the game starts and recycles them from one edge to the other. Nothing is instantiated during play. |
| ScriptableObject config | `GameConfig` (`Assets/Config/GameConfig.asset`) | All global tuning values (timer, lives, hop speed, scoring, turtle timings) in one asset, editable while the game runs. |
| Trigger-based collision | `PlayerController`, `Rideable`, `Home` | The frog moves by exact grid hops, not physics forces. Triggers only answer "what did I land on / what hit me". |
| Diving turtles | `TurtleDiver` | Runs the surfaced → warning → submerged cycle and marks the group unsafe while it is under water. |
| Persistence | `GameManager` | The high score is stored in `PlayerPrefs`. |
| UI | `UIManager` | HUD, title screen, game-over screen, blinking prompts. |
| Audio | `AudioManager` | Sound effects, the looping background music, and the low-time alarm. |

Per-lane settings (prefab, direction, speed, spacing, start offset, turtle dive delays) live on each `LaneController` under the `Lanes` object in the scene.

### Project layout

```
Assets/
  Audio/      Sound effects and music
  Config/     GameConfig asset
  Fonts/      bit5x3 pixel font
  Prefabs/    Vehicles, logs, turtles, home slot
  Scenes/     Frogger.unity
  Scripts/    All game code
  Sprites/    Pixel art
Docs/
  GDD.md      Game Design Document
```

---

## Credits

- **Design and programming:** Idan Shaul
- **Sprites and font (bit5x3):** by Zigurous, from their Frogger tutorial. The tutorial does not state a licence, so these assets are used here for learning purposes only and are not covered by any licence of this project.
- **Sound effects and music:** [Pixabay](https://pixabay.com/sound-effects/), used under the Pixabay Content License

*Frogger* is a trademark of Konami. This is a non-commercial portfolio project, and "Frogger" is used only as a working title. No original arcade assets ship with the game. The arcade flyer in `Docs/images` is there for reference only and is not part of the build.
