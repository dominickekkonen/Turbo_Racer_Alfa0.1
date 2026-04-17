
# 🏎️ Turbo Racer XL

A high-speed arcade drifter built entirely within the C# console. No external graphics libraries—just pure ANSI escape sequences and procedural rendering.

## 🕹️ Game Features
* **Synthwave Rendering:** High-detail login background using a 5-stage color gradient (Purple, Red, Orange, Yellow).
* **Procedural Palm Trees:** Sine-wave calculated trunks with ASCII-textured fronds.
* **Horizon Traffic:** Layered background car silhouettes integrated into the horizon line.
* **Real-time Dashboard:** Flicker-free UI sidecar tracking MPH, Engine Temp, and RPM.
* **Progressive Difficulty:** Engine-driven speed scaling as your score increases.

---

## 🛠️ Technical Implementation
* **Graphics Engine:** Uses a layered `StringBuilder` approach to render the sky, sun, city, and palms before writing to the console buffer.
* **Color System:** Custom ANSI 256-color constants for the neon/synthwave palette.
* **Collision Logic:** Coordinate-based AABB detection between the player and road obstacles.
* **Input Handling:** Non-blocking `Console.KeyAvailable` loop to ensure smooth 60fps-style movement.

---

## 🚀 Setup & Requirements
* **Runtime:** .NET 8.0 SDK.
* **IDE:** Visual Studio 2022.
* **Terminal:** Best viewed in **Windows Terminal**. 
* **Font:** Set your console font to `Cascadia Code` or `Consolas` to ensure the ASCII shapes align correctly.

---

## ⌨️ Controls
* **Move:** WASD or Arrow Keys
* **Turbo:** Spacebar (Increases speed and engine heat)
* **Pause:** P
* **Quit:** ESC

## 📜 License
MIT
