using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turbo_Racer_Alfa1.Models;

namespace Turbo_Racer_Alfa1.Entities
{
    class TrafficSign : Entity
    {
        public override string[] Sprite => new string[]
        {
        "|]=======================================================[|",
        "||    /STOP\\          /STOP\\          /STOP\\          /STOP\\ ||",
        "|]=======================================================[|",
        "||                                                       ||"
        };
        public override string Color => "\x1b[90m";
        public TrafficSign(int x, int y) : base(x, y) { }
        public override bool CheckCollision(Player p) => false;
    }
}
