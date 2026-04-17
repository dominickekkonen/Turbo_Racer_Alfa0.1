using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turbo_Racer_Alfa1.Models;

namespace Turbo_Racer_Alfa1.Entities
{
    abstract class Entity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public abstract string[] Sprite { get; }
        public abstract string Color { get; }
        public Entity(int x, int y) { X = x; Y = y; }
        public virtual void Update() => Y++;
        public virtual bool CheckCollision(Player p) => Math.Abs(Y - p.Y) < 2 && Math.Abs(X - p.X) < 3;
    }
}
