using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turbo_Racer_Alfa1.Models
{
    class Player
    {
        private readonly int[] validPositions = { 12, 20, 27, 35, 42, 50, 57 };
        private int positionIndex = 3;

        public string Name { get; set; } = "Driver";
        public int X => validPositions[positionIndex];
        public int Y { get; } = 18;
        public int Lives { get; set; } = 3;
        public int Score { get; set; } = 0;

        public string CarColor { get; set; } = "\x1b[36m";
        public string[] Sprite { get; set; } = {
        "\x1b[90mo\x1b[0m---\x1b[90mo\x1b[0m",
        "| A |",
        "\x1b[90mo\x1b[0m---\x1b[90mo\x1b[0m"
        };

        public void MoveLeft() { if (positionIndex > 0) positionIndex--; }
        public void MoveRight() { if (positionIndex < validPositions.Length - 1) positionIndex++; }
        public string GetHearts() => new string('♥', Math.Max(0, Lives));
    }

}
