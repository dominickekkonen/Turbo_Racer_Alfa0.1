using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turbo_Racer_Alfa1.Entities
{
    class Obstacle : Entity
    {
        public override string[] Sprite => new string[] { "o---o", "| X |", "o---o" };
        public override string Color => "\x1b[31m";
        public Obstacle(int x, int y) : base(x, y) { }
    }
}
