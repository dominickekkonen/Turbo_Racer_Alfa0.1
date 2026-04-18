using System.Text;
using System.Text.RegularExpressions;
using Turbo_Racer_Alfa1.Addons;
using Turbo_Racer_Alfa1.Entities;
using Turbo_Racer_Alfa1.Models;

namespace Turbo_Racer_Alfa1.Main
{
    class TurboRacerGame
    {
        struct Pixel
        {
            public char Char;
            public string Color;
            public string BgColor;
        }

        private Pixel[,] _screenBuffer;
        private int _bufferWidth;
        private int _bufferHeight;

        const string Reset = "\x1b[0m", Red = "\x1b[31m", Green = "\x1b[32m",
                     Yellow = "\x1b[33m", Blue = "\x1b[34m", Cyan = "\x1b[36m",
                     White = "\x1b[37m", Bold = "\x1b[1m", Gray = "\x1b[90m", Magenta = "\x1b[35m";
        const string DarkOrange = "\x1b[38;5;202m";
        const string LightYellow = "\x1b[38;5;226m";
        const string CyanGlow = "\x1b[38;5;81m";
        const string Orange = "\x1b[38;5;208m";
        const string Purple = "\x1b[38;5;141m";
        const string DarkGray = "\x1b[38;5;240m";
        const string NeonGreen = "\x1b[38;5;82m";
        const string Black = "\x1b[30m";
        const string BgGreen = "\x1b[42m";
        const string BgCyan = "\x1b[46m";
        const string BgYellow = "\x1b[43m";
        const string BgRed = "\x1b[41m";
        const string Dim = "\x1b[2m";

        // --- TURBO SYSTEM VARIABLES ---
        bool isTurboActive = false;
        DateTime turboEndTime = DateTime.MinValue;
        DateTime nextTurboReadyTime = DateTime.MinValue; // Tracks the 15s recharge
        int turboCooldownSeconds = 15;

        const string ScoreFile = "highscores.txt";
        enum State { Menu, Settings, Playing, GameOver, Database }
        State currentState = State.Menu;
        private int spawnRate = 45;
        Player player = new Player();
        List<Entity> entities = new List<Entity>();
        List<ScoreEntry> scoreHistory = new List<ScoreEntry>();
        Random rng = new Random();
        Thread musicThread;

        int gameSpeed = 45;
        string difficultyName = "Easy";
        int healthLimit = 5;
        int maxSpawnCount = 1;
        int repairInterval = 200;
        int signInterval = 250;
        int lastRepairScore = 0;
        int lastSignScore = 0;

        bool isRunning = true;
        int roadOffset = 0;

        const int RoadLeft = 5, RoadRight = 65, RoadHeight = 22;
        readonly int[] spawnPositions = { 12, 20, 27, 35, 42, 50, 57 };
        readonly string[] brightColors = { Green, Yellow, Cyan, Magenta, Bold + Blue };
        readonly string[][] carModels = {
        new string[] { "o---o", "| A |", "o---o" },
        new string[] { "/---\\", "| S |", "\\---/" },
        new string[] { "H---H", "[ T ]", "H---H" },
        new string[] { "-=X=-", " |V| ", "-=X=-" }
        };

        void SoundMove() => Console.Beep(450, 15);
        void SoundCrash() => Console.Beep(180, 100);
        void SoundRepair() => Console.Beep(900, 60);
        void SoundGameOver() { Console.Beep(300, 200); Console.Beep(200, 200); Console.Beep(150, 400); }

        public void Start()
        {
            Console.OutputEncoding = Encoding.UTF8;
            LoadScoresFromFile();

            // Initialize buffer based on window size
            _bufferWidth = Console.WindowWidth;
            _bufferHeight = Console.WindowHeight;
            _screenBuffer = new Pixel[_bufferWidth, _bufferHeight];

            try { Console.CursorVisible = false; } catch { }
            Console.OutputEncoding = Encoding.UTF8;
            LoadScoresFromFile();
            try { Console.CursorVisible = false; } catch { }

            while (isRunning)
            {
                switch (currentState)
                {
                    case State.Menu: DrawMenu(); break;
                    case State.Settings: DrawSettings(); break;
                    case State.Database: DrawDatabase(); break;
                    case State.Playing: UpdateGame(); break;
                    case State.GameOver: DrawGameOver(); break;
                }
            }
        }

        void SaveScoreToFile(string name, int score) { try { File.AppendAllText(ScoreFile, $"{name}|{score}" + Environment.NewLine); scoreHistory.Add(new ScoreEntry { Name = name, Score = score }); } catch { } }

        void LoadScoresFromFile()
        {
            scoreHistory.Clear();
            try
            {
                if (File.Exists(ScoreFile))
                {
                    string[] lines = File.ReadAllLines(ScoreFile);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int s))
                            scoreHistory.Add(new ScoreEntry { Name = parts[0], Score = s });
                    }
                }
            }
            catch { }
        }

        void DrawCentered(string text)
        {
            int width = Console.WindowWidth;
            string cleanText = Regex.Replace(text, @"\x1b\[[0-9;]*m", "");
            int spaces = Math.Max(0, (width / 2) - (cleanText.Length / 2));
            Console.WriteLine(new string(' ', spaces) + text);
        }

        void DrawMenuButtonPair(
        int leftIdx, string leftLabel, string leftFg, string leftBg,
        int rightIdx, string rightLabel, string rightFg, string rightBg,
        int sel)
        {
            bool lOn = sel == leftIdx;
            bool rOn = sel == rightIdx;

            string lTop = lOn
                ? $"{leftBg}{Bold}{White}╔══════════════════╗{Reset}"
                : $"{Dim}{leftFg}╔══════════════════╗{Reset}";
            string rTop = rOn
                ? $"{rightBg}{Bold}{White}╔══════════════════╗{Reset}"
                : $"{Dim}{rightFg}╔══════════════════╗{Reset}";

            string lMid = lOn
                ? $"{leftBg}{Bold}{White}║█ {leftLabel,-14} █║{Reset}"
                : $"{Dim}{leftFg}║  {leftLabel,-14}  ║{Reset}";
            string rMid = rOn
                ? $"{rightBg}{Bold}{White}║█ {rightLabel,-14} █║{Reset}"
                : $"{Dim}{rightFg}║  {rightLabel,-14}  ║{Reset}";

            string lBot = lOn
                ? $"{leftBg}{Bold}{White}╚══════════════════╝{Reset}"
                : $"{Dim}{leftFg}╚══════════════════╝{Reset}";
            string rBot = rOn
                ? $"{rightBg}{Bold}{White}╚══════════════════╝{Reset}"
                : $"{Dim}{rightFg}╚══════════════════╝{Reset}";

            DrawCentered($"{lTop}  {rTop}");
            DrawCentered($"{lMid}  {rMid}");
            DrawCentered($"{lBot}  {rBot}");
        }

        void DrawMenu()
        {
            if (musicThread == null || !musicThread.IsAlive)
            {
                musicThread = new Thread(MusicPlayer.PlayHelmetBetter) { IsBackground = true };
                musicThread.Start();
            }

            int sel = 0; // 0=Start  1=Difficulty  2=HallOfFame  3=Terminate

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n\n");

                DrawCentered($"{Blue}{Bold}╔══════════════════════════════════════════╗{Reset}");
                DrawCentered($"{Blue}{Bold}║             🏎️  {Yellow}TURBO RACER{Blue}              ║{Reset}");
                DrawCentered($"{Blue}{Bold}╚══════════════════════════════════════════╝{Reset}");

                var best = scoreHistory.OrderByDescending(x => x.Score).FirstOrDefault();
                if (scoreHistory.Any())
                    DrawCentered($"{Green}{Bold}★ ALL-TIME BEST: {best.Name} ({best.Score}) ★{Reset}");

                Console.WriteLine("\n");

                DrawMenuButtonPair(
                    0, "START MISSION ", Green, BgGreen,
                    1, "DIFFICULTY    ", Cyan, BgCyan,
                    sel);
                Console.WriteLine();
                DrawMenuButtonPair(
                    2, "HALL OF FAME  ", Yellow, BgYellow,
                    3, "TERMINATE     ", Red, BgRed,
                    sel);

                Console.WriteLine("\n");
                DrawCentered($"{Gray}◄ ► Columns   ↑ ↓ Rows   ENTER Select{Reset}");

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow: if (sel % 2 == 1) sel--; break;
                    case ConsoleKey.RightArrow: if (sel % 2 == 0) sel++; break;
                    case ConsoleKey.UpArrow: if (sel >= 2) sel -= 2; break;
                    case ConsoleKey.DownArrow: if (sel < 2) sel += 2; break;
                    case ConsoleKey.Enter:
                        if (sel == 0) { StopMusic(); LoginAndStart(); return; }
                        else if (sel == 1) { currentState = State.Settings; return; }
                        else if (sel == 2) { currentState = State.Database; return; }
                        else if (sel == 3) { isRunning = false; return; }
                        break;
                }
            }
        }

        private void StopMusic()
        {
            if (musicThread != null && musicThread.IsAlive)
            {
                musicThread.Interrupt(); // Sends the signal to the catch block above
                musicThread.Join();      // Wait for it to actually finish
            }
        }

        void LoginAndStart()
        {
            StopMusic();
            DrawLoginBackground();

            int width = Console.WindowWidth;
            int boxY = 18; // Must match the boxY in DrawLoginBackground
            int startX = Math.Max(0, (width / 2) - 22);

            // Set cursor exactly inside the box, after "DRIVER ID: "
            // "DRIVER ID: " is 11 characters, so we start at +13 for a little padding
            Console.SetCursorPosition(startX + 13, boxY + 1);

            Console.CursorVisible = true;
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = "Driver_One";
            Console.CursorVisible = false;

            ResetGame();
            player.Name = name;
            Console.Clear();
            currentState = State.Playing;
        }

        void DrawLoginBackground()
        {
            Console.Clear();
            int width = Console.WindowWidth;
            int height = Console.WindowHeight > 0 ? Console.WindowHeight : 30;
            StringBuilder canvas = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                string line = "";
                for (int x = 0; x < width; x++)
                {
                    double dx = (x - (width / 2.0)) * 0.45;
                    double dy = (y - 9);
                    double dist = dx * dx + dy * dy;
                    string treePart = GetPalmTreePixel(x, y, width);

                    // --- 1. PALM TREES ---
                    if (treePart != null) line += $"{Green}{treePart}{Reset}";

                    // --- 2. COOL SCANLINE SUN ---
                    else if (dist < 40 && y < 14)
                    {
                        // Every 2nd line is slightly darker to create a "retro monitor" effect
                        bool scanline = y % 2 == 0;
                        string sunCol = (y > 11) ? "\x1b[38;2;255;210;0m" : "\x1b[38;2;255;50;0m";
                        if (!scanline) sunCol = (y > 11) ? "\x1b[38;2;200;160;0m" : "\x1b[38;2;180;30;0m";

                        line += $"{sunCol}█{Reset}";
                    }

                    // --- 3. DETAILED SKYSCRAPERS ---
                    else if (y >= 10 && y <= 14 && (x % 18 < 8))
                    {
                        int buildingID = x / 18;
                        // Vary building heights
                        int heightVar = (buildingID % 3 == 0) ? 10 : 12;

                        if (y >= heightVar)
                        {
                            // Random glowing windows
                            if ((x + y) % 3 == 0 && x % 2 == 0 && y < 14)
                                line += $"{LightYellow}■{Reset}";
                            else
                                line += $"{DarkGray}▓{Reset}";
                        }
                        else line += GetSkyColor(y);
                    }

                    // --- 4. SMOOTH SKY ---
                    else if (y < 15) line += GetSkyColor(y);

                    // --- 5. NEON GRID VOID ---
                    else
                    {
                        int fade = Math.Max(0, 255 - ((y - 15) * 15));
                        string gridMagenta = $"\x1b[38;2;{fade};0;{fade}m";
                        string gridCyan = $"\x1b[38;2;0;{fade};{fade}m";

                        if (y % 2 == 0) line += $"{gridMagenta}═{Reset}";
                        else if (x % 10 == 0) line += $"{gridCyan}║{Reset}";
                        else line += " ";
                    }
                }
                canvas.AppendLine(line);
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(canvas.ToString());

            // --- CENTERING THE LOGIN BOX ---
            int boxY = 18;
            int startX = Math.Max(0, (width / 2) - 22);
            Console.SetCursorPosition(startX, boxY);
            Console.Write($"{CyanGlow}╔══════════════════════════════════════════╗{Reset}");
            Console.SetCursorPosition(startX, boxY + 1);
            Console.Write($"{CyanGlow}║ DRIVER ID:                               ║{Reset}");
            Console.SetCursorPosition(startX, boxY + 2);
            Console.Write($"{CyanGlow}╚══════════════════════════════════════════╝{Reset}");
        }

        // Helper to keep the main loop clean
        string GetSkyColor(int y)
        {
            int r = Math.Min(255, 80 + (y * 12));
            int g = 10;
            int b = Math.Max(0, 160 - (y * 10));
            return $"\x1b[38;2;{r};{g};{b}m█{Reset}";
        }
        string GetPalmTreePixel(int x, int y, int screenWidth)
        {
            // Place trees at specific intervals
            int[] trunks = { screenWidth / 5, (screenWidth / 5) * 4 };

            foreach (int trunkX in trunks)
            {
                // 1. THE TRUNK (Shaded and Curved)
                // We add a slight Sine wave to make the trunk look like it's leaning
                int curve = (int)(Math.Sin(y * 0.4) * 1.5);
                int currentTrunkX = trunkX + curve;

                if (y >= 9 && y <= 14)
                {
                    if (x == currentTrunkX)
                    {
                        // Darker green for the trunk silhouette
                        return $"\x1b[38;2;20;80;20m║{Reset}";
                    }
                    if (x == currentTrunkX + 1)
                    {
                        // Highlight on the right side of the trunk
                        return $"\x1b[38;2;40;120;40m░{Reset}";
                    }
                }

                // 2. THE FRONDS (The Leafy Crown)
                int crownY = 8; // Height where leaves start
                int relY = y - crownY;
                int relX = x - (trunkX + (int)(Math.Sin(crownY * 0.4) * 1.5));

                // Use different greens to simulate light and shadow
                string brightGreen = "\x1b[38;2;50;255;50m";
                string midGreen = "\x1b[38;2;0;180;0m";
                string darkGreen = "\x1b[38;2;0;100;0m";

                if (relY == 0) // Center of the palm crown
                {
                    if (relX == 0) return $"{brightGreen}*{Reset}";
                    if (Math.Abs(relX) <= 2) return $"{midGreen}═{Reset}";
                }
                else if (relY == -1) // Upper fronds
                {
                    if (relX == -3 || relX == 3) return $"{darkGreen}/{Reset}";
                    if (relX == -4 || relX == 4) return $"{darkGreen}\\{Reset}";
                }
                else if (relY == 1) // Drooping lower fronds
                {
                    if (Math.Abs(relX) == 4) return $"{midGreen}┿{Reset}";
                    if (relX == -5) return $"{darkGreen}╱{Reset}";
                    if (relX == 5) return $"{darkGreen}╲{Reset}";
                }
            }
            return null;
        }

        string GetBackgroundCarPixel(int x, int y, int screenWidth)
        {
            // We place cars at y = 14 (the horizon line)
            if (y == 14)
            {
                // Define 3 car positions across the screen
                int[] carPositions = { screenWidth / 3, screenWidth / 2 + 10, (screenWidth / 4) * 3 };

                foreach (int carX in carPositions)
                {
                    // Simple low-profile sports car silhouette
                    if (x == carX) return $"{White}█{Reset}"; // Cabin
                    if (x == carX - 1 || x == carX + 1) return $"{Blue}▄{Reset}"; // Body
                    if (x == carX - 2) return $"{Red}◀{Reset}"; // Tail lights
                    if (x == carX + 2) return $"{Yellow}▶{Reset}"; // Headlights
                }
            }
            return null;
        }


        void DrawDatabase()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            DrawCentered($"{Yellow}═══ 🏆 GLOBAL HALL OF FAME 🏆 ═══{Reset}");
            var top5 = scoreHistory.OrderByDescending(x => x.Score).Take(5).ToList();
            if (!top5.Any()) DrawCentered($"{Gray}No driver data available yet.{Reset}");
            else
            {
                for (int i = 0; i < top5.Count; i++)
                    DrawCentered($"{(i == 0 ? Yellow : (i == 1 ? White : Gray))}#{i + 1} {top5[i].Name.PadRight(12)} : {top5[i].Score:D6}{Reset}");
            }
            Console.WriteLine("\n\n" + Gray + "Press any key to go back" + Reset);
            Console.ReadKey(true);
            currentState = State.Menu;
        }

        void DrawDifficultyOption(int idx, int sel,
                          string modeName, string description,
                          string fg, string bg, string tc)
        {
            bool on = idx == sel;

            if (on)
            {
                string namePad = ("► " + modeName).PadRight(41) + "◄";
                string descPad = ("  (" + description + ")").PadRight(42);
                if (descPad.Length > 42) descPad = descPad.Substring(0, 42);

                DrawCentered($"{bg}{Bold}{tc}╔══════════════════════════════════════════╗{Reset}");
                DrawCentered($"{bg}{Bold}{tc}║{namePad}║{Reset}");
                DrawCentered($"{bg}{Bold}{tc}╠══════════════════════════════════════════╣{Reset}");
                DrawCentered($"{bg}{Bold}{tc}║{descPad}║{Reset}");
                DrawCentered($"{bg}{Bold}{tc}╚══════════════════════════════════════════╝{Reset}");
            }
            else
            {
                string namePad = ("  " + modeName).PadRight(42);
                if (namePad.Length > 42) namePad = namePad.Substring(0, 42);
                string descPad = ("     (" + description + ")").PadRight(42);
                if (descPad.Length > 42) descPad = descPad.Substring(0, 42);

                DrawCentered($"{Dim}{fg}╔══════════════════════════════════════════╗{Reset}");
                DrawCentered($"{Dim}{fg}║{namePad}║{Reset}");
                DrawCentered($"{Dim}{fg}╠══════════════════════════════════════════╣{Reset}");
                DrawCentered($"{Dim}{fg}║{descPad}║{Reset}");
                DrawCentered($"{Dim}{fg}╚══════════════════════════════════════════╝{Reset}");
            }
        }

        void DrawSettings()
        {
            int sel = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n\n");
                DrawCentered($"{Bold}{White}─── {Cyan}SELECT YOUR INTENSITY{White} ───{Reset}");
                Console.WriteLine("\n");

                DrawDifficultyOption(0, sel, "EASY MODE", "HP: 5 / Repair Kit Interval: 200",
                                     Green, BgGreen, White);
                Console.WriteLine();
                DrawDifficultyOption(1, sel, "HARD MODE", "HP: 3 / Repair Kit Interval: 300",
                                     Yellow, BgYellow, White);
                Console.WriteLine();
                DrawDifficultyOption(2, sel, "EXTREME MODE", "HP: 2 / Repair Kit Interval: 400",
                                     Red, BgRed, White);

                Console.WriteLine("\n");
                DrawCentered($"{Gray}↑ ↓ Navigate   ENTER Confirm   ESC Back{Reset}");

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow: sel = (sel + 2) % 3; break;
                    case ConsoleKey.DownArrow: sel = (sel + 1) % 3; break;
                    case ConsoleKey.Enter:
                        if (sel == 0) { gameSpeed = 45; difficultyName = "Easy"; healthLimit = 5; maxSpawnCount = 1; repairInterval = 200; }
                        else if (sel == 1) { gameSpeed = 25; difficultyName = "Hard"; healthLimit = 3; maxSpawnCount = 2; repairInterval = 300; }
                        else if (sel == 2) { gameSpeed = 20; difficultyName = "Extreme"; healthLimit = 2; maxSpawnCount = 3; repairInterval = 400; }
                        currentState = State.Menu; return;
                    case ConsoleKey.Escape:
                        currentState = State.Menu; return;
                }
            }
        }

        void ResetGame() { player = new Player(); player.Lives = healthLimit; player.CarColor = brightColors[rng.Next(brightColors.Length)]; player.Sprite = carModels[rng.Next(carModels.Length)]; entities.Clear(); lastRepairScore = 0; lastSignScore = 0; }

        void UpdateGame()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var key = keyInfo.Key;

                if (key == ConsoleKey.LeftArrow) { player.MoveLeft(); SoundMove(); }
                if (key == ConsoleKey.RightArrow) { player.MoveRight(); SoundMove(); }
                if (key == ConsoleKey.Escape) { StopMusic(); currentState = State.Menu; return; }

                // --- TURBO ACTIVATION WITH RECHARGE CHECK ---
                if (key == ConsoleKey.Spacebar || key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                {
                    // Only activate if not active AND recharge is finished
                    if (!isTurboActive && DateTime.Now >= nextTurboReadyTime)
                    {
                        isTurboActive = true;
                        turboEndTime = DateTime.Now.AddSeconds(5);
                        // Set the next ready time to 15 seconds from NOW
                        nextTurboReadyTime = DateTime.Now.AddSeconds(turboCooldownSeconds);
                        Console.Beep(800, 50); // Optional: "Engaged" sound
                    }
                }
            }

            // --- ENTITY & ROAD UPDATE ---
            roadOffset = (roadOffset + 1) % 4;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                entities[i].Update();
                if (entities[i].CheckCollision(player))
                {
                    if (entities[i] is Obstacle)
                    {
                        player.Lives -= 1; SoundCrash();
                        if (player.Lives <= 0)
                        {
                            player.Lives = 0;
                            SaveScoreToFile(player.Name, player.Score);
                            SoundGameOver();
                            StopMusic();
                            currentState = State.GameOver;
                            return;
                        }
                    }
                    else if (entities[i] is RepairKit)
                    {
                        if (player.Lives < healthLimit) { player.Lives++; SoundRepair(); }
                    }
                    entities.RemoveAt(i);
                }
                else if (entities[i].Y > RoadHeight + 5) entities.RemoveAt(i);
            }

            // --- SPAWN LOGIC ---
            if (player.Score >= lastSignScore + signInterval) { entities.Add(new TrafficSign(35, 0)); lastSignScore = player.Score; }

            bool spawnRepairNow = false;
            if (player.Score >= lastRepairScore + repairInterval) { spawnRepairNow = true; lastRepairScore = player.Score; }

            if (!entities.Any(e => e.Y < 5 && !(e is TrafficSign)) && rng.Next(0, 10) > 5)
            {
                int spawns = rng.Next(1, maxSpawnCount + 1);
                List<int> used = new List<int>();
                for (int i = 0; i < spawns; i++)
                {
                    int posIdx = rng.Next(spawnPositions.Length);
                    if (!used.Contains(posIdx))
                    {
                        if (spawnRepairNow) { entities.Add(new RepairKit(spawnPositions[posIdx], 0)); spawnRepairNow = false; }
                        else { entities.Add(new Obstacle(spawnPositions[posIdx], 0)); }
                        used.Add(posIdx);
                    }
                }
            }

            // --- TURBO DURATION CHECK & SCORE ---
            if (isTurboActive && DateTime.Now >= turboEndTime)
            {
                isTurboActive = false;
            }

            // Determine speed and score
            int currentSleepTime = isTurboActive ? gameSpeed / 2 : gameSpeed;
            int scoreMultiplier = isTurboActive ? 2 : 1;

            int baseScoreGain = (difficultyName == "Extreme" ? 10 : (difficultyName == "Hard" ? 5 : 2));
            player.Score += (baseScoreGain * scoreMultiplier);

            DrawFrame();
            Thread.Sleep(currentSleepTime);
        }

        void DrawFrame()
        {
            ClearBuffer();

            // 1. DYNAMIC DIFFICULTY CALCULATION
            string currentStatus = "NOMINAL";
            string statusColor = NeonGreen;

            if (spawnRate <= 15) { currentStatus = "CRITICAL"; statusColor = Red; }
            else if (spawnRate <= 25) { currentStatus = "HAZARDOUS"; statusColor = Orange; }
            else if (spawnRate <= 35) { currentStatus = "CAUTION"; statusColor = Yellow; }

            // 2. TOP HEADER & BORDER
            // Updated to show the dynamic difficulty status
            WriteToBuffer(2, 1, $"LIVE TELEMETRY | MPH: {(player.Score / 10) + 60:D3} | STATUS: {statusColor}{currentStatus}{Reset} | GEAR: 5", CyanGlow);
            WriteToBuffer(2, 2, $"╔{new string('═', RoadRight - RoadLeft + 1)}╗", Blue);

            // 3. DRAW THE DATA PANEL (Right Side)
            int dataX = RoadRight - RoadLeft + 6;
            WriteToBuffer(dataX, 4, $"ID: {player.Name.ToUpper()}", White);
            WriteToBuffer(dataX, 5, $"SC: {player.Score:D6}", Yellow);
            WriteToBuffer(dataX, 6, $"HP: {player.GetHearts()}", Red);
            WriteToBuffer(dataX, 8, "----------", DarkGray);
            WriteToBuffer(dataX, 9, "OIL: 190°F", Orange);

            int rpm = (roadOffset) % 8;
            WriteToBuffer(dataX, 12, $"RPM: [{NeonGreen}{new string('|', rpm)}{Gray}{new string('.', 7 - rpm)}]", Gray);

            // --- RE-ADDED NOS BAR LOGIC ---
            int nosY = 15 + 3; // Positioned lower on the right panel
            if (isTurboActive)
            {
                double timeLeft = (turboEndTime - DateTime.Now).TotalSeconds;
                WriteToBuffer(dataX, nosY, $"NOS: {timeLeft:F1}s ACTIVE", NeonGreen);
            }
            else if (DateTime.Now < nextTurboReadyTime)
            {
                double totalWait = turboCooldownSeconds;
                double remainingWait = (nextTurboReadyTime - DateTime.Now).TotalSeconds;
                double progress = 1.0 - (remainingWait / totalWait);
                int barLen = 8;
                int filled = Math.Min((int)(progress * barLen), barLen);
                string bar = new string('█', filled) + new string('░', barLen - filled);
                WriteToBuffer(dataX, nosY, $"NOS: [{bar}]", Yellow);
            }
            else
            {
                WriteToBuffer(dataX, nosY, "NOS: READY", CyanGlow);
            }

            // 4. DRAW THE ROAD
            for (int y = 0; y < RoadHeight; y++)
            {
                int screenY = y + 3;
                WriteToBuffer(2, screenY, "║", Blue);
                WriteToBuffer(RoadRight - RoadLeft + 3, screenY, "║", Blue);

                for (int x = RoadLeft; x <= RoadRight; x++)
                {
                    int screenX = x - RoadLeft + 3;
                    if ((x == 20 || x == 35 || x == 50) && (y + roadOffset) % 4 == 0)
                        WriteToBuffer(screenX, screenY, "¦", White);
                }
            }

            // 5. DRAW ENTITIES (With Clipping)
            foreach (var ent in entities)
            {
                int h = ent.Sprite.Length / 2;
                int w = ent.Sprite[0].Length / 2;
                for (int sy = 0; sy < ent.Sprite.Length; sy++)
                {
                    int spriteY = ent.Y + sy - h;
                    if (spriteY >= 0 && spriteY < RoadHeight)
                    {
                        WriteToBuffer(ent.X - RoadLeft + 3 - w, spriteY + 3, ent.Sprite[sy], ent.Color);
                    }
                }
            }

            // 6. DRAW PLAYER
            for (int py = 0; py < player.Sprite.Length; py++)
            {
                int playerScreenY = player.Y + py + 3 - 1;
                if (playerScreenY > 2 && playerScreenY < RoadHeight + 3)
                {
                    WriteToBuffer(player.X - RoadLeft + 3 - 2, playerScreenY, player.Sprite[py], player.CarColor);
                }
            }

            // 7. BOTTOM BORDER & FOOTER
            int bottomLineY = RoadHeight + 3;
            WriteToBuffer(2, bottomLineY, $"╚{new string('═', RoadRight - RoadLeft + 1)}╝", Blue);
            WriteToBuffer(2, bottomLineY + 1, "VITAL_LINK_ESTABLISHED // SCANNING_OBSTACLES...", DarkGray);

            RenderBuffer();
        }
        void DrawGameOver()
        {
            Console.Clear();
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            // 1. FULL SCREEN SOLID GRADIENT
            for (int y = 0; y < height; y++)
            {
                // Calculate the color shift
                int r = Math.Clamp(100 - (y * 2), 20, 255);
                int b = Math.Clamp(150 - (y * 6), 0, 255);

                // \x1b[48;2;R;G;Bm sets the BACKGROUND color (TrueColor)
                // \x1b[38;2;R;G;Bm sets the FOREGROUND color
                string bgCol = $"\x1b[48;2;{r / 5};0;{b / 5}m";
                string fgCol = $"\x1b[38;2;{r};0;{b}m";

                Console.SetCursorPosition(0, y);

                // Write a full line of "░" or spaces with the background color set
                // This ensures the color fills the entire width of the console
                string lineContent = new string('░', width);
                Console.Write(bgCol + fgCol + lineContent);
            }

            // 2. UI OVERLAY (Reset background to transparent/gradient-friendly)
            // We use \x1b[49m to reset only the background color while keeping our text colors
            string bgReset = "\x1b[49m";
            Console.SetCursorPosition(0, 2);

            DrawCentered($"{bgReset}{Red}{Bold}╔══════════════════════════════════════════╗{Reset}");
            DrawCentered($"{bgReset}{Red}{Bold}║  {White}⚡ {Red}CRITICAL SYSTEM FAILURE {White}⚡  {Red}║{Reset}");
            DrawCentered($"{bgReset}{Red}{Bold}╚══════════════════════════════════════════╝{Reset}");

            // 3. DIAGNOSTICS
            var best = scoreHistory.OrderByDescending(x => x.Score).FirstOrDefault();
            bool hasHistory = !best.Equals(default(ScoreEntry));
            int topScore = hasHistory ? best.Score : 0;
            string topName = hasHistory ? (best.Name ?? "---") : "---";

            Console.WriteLine("\n");
            DrawCentered($"{bgReset}{Gray}———————————————— POST-CRASH DIAGNOSTIC ————————————————{Reset}");
            Console.WriteLine();

            DrawCentered($"{bgReset}{Gray}OPERATOR ID:      {Cyan}{player.Name.ToUpper()}{Reset}");
            DrawCentered($"{bgReset}{Gray}TOTAL UNITS:      {Yellow}{player.Score:D6}{Reset}");

            if (player.Score >= topScore && player.Score > 0)
            {
                DrawCentered($"{bgReset}\x1b[1;5;38;5;201m▶▶ NEW SECTOR RECORD ESTABLISHED ◀◀{Reset}");
            }
            else
            {
                DrawCentered($"{bgReset}{Gray}SECTOR RECORD:    {topScore:D6} ({topName}){Reset}");
            }

            // 4. DAMAGE REPORT
            Console.WriteLine("\n");
            DrawCentered($"{bgReset}{Red}VEHICLE INTEGRITY: [ !!!!!!!!!! ] 0% - TOTAL LOSS{Reset}");
            DrawCentered($"{bgReset}{Gray}CORE_TEMP: OVERHEAT | LOG_RECOVERY: SUCCESSFUL{Reset}");

            // 5. FOOTER
            Console.SetCursorPosition(0, height - 2);
            DrawCentered($"{bgReset}{NeonGreen}>> PRESS ANY KEY TO REBOOT SYSTEM <<{Reset}");

            Console.ResetColor();
            Console.ReadKey(true);
            currentState = State.Menu;
        }
        void ClearBuffer()
        {
            for (int y = 0; y < _bufferHeight; y++)
            {
                for (int x = 0; x < _bufferWidth; x++)
                {
                    _screenBuffer[x, y] = new Pixel { Char = ' ', Color = White, BgColor = "\x1b[40m" };
                }
            }
        }

        void WriteToBuffer(int x, int y, string text, string fg = White, string bg = "\x1b[40m")
        {
            if (y < 0 || y >= _bufferHeight) return;
            // Strip ANSI from text to get true length for positioning
            string cleanText = Regex.Replace(text, @"\x1b\[[0-9;]*m", "");
            for (int i = 0; i < cleanText.Length; i++)
            {
                int posX = x + i;
                if (posX >= 0 && posX < _bufferWidth)
                    _screenBuffer[posX, y] = new Pixel { Char = cleanText[i], Color = fg, BgColor = bg };
            }
        }

        void RenderBuffer()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\x1b[H"); // Move cursor to top-left without flickering
            string lastFg = "", lastBg = "";

            for (int y = 0; y < _bufferHeight; y++)
            {
                for (int x = 0; x < _bufferWidth; x++)
                {
                    var p = _screenBuffer[x, y];
                    if (p.Color != lastFg) { sb.Append(p.Color); lastFg = p.Color; }
                    if (p.BgColor != lastBg) { sb.Append(p.BgColor); lastBg = p.BgColor; }
                    sb.Append(p.Char);
                }
                if (y < _bufferHeight - 1) sb.Append('\n');
            }
            Console.Write(sb.ToString());
            //
        }
    }
}
