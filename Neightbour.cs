using System.Collections.Generic;
using System.Drawing;

namespace TerrainGenerator
{
    public class Neightbour
    {

        public List<Point> Points { get; protected set; }
        public Biome Biome { get; protected set; }

        public Neightbour(List<Point> points, Biome biome)
        {
            Biome = biome;
            Points = points;
        }
    }
}
