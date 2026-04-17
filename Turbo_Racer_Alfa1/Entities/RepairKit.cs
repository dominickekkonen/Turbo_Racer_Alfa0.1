using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turbo_Racer_Alfa1.Entities
{
    class RepairKit : Entity
    {
        public override string[] Sprite => new string[] { " +++ ", "(🔧)", " +++ " };
        public override string Color => "\x1b[32m";
        public RepairKit(int x, int y) : base(x, y) { }
    }
}
