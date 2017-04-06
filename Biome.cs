using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace TerrainGenerator
{
    public class Biome
    {
        public List<Point> Points;
        public BiomeType BiomeType { get; private set; }
        public Point Center { get; private set; }
        public bool Bounded { get; private set; }
        public int Height { get; private set; }
        public List<Neightbour> Neightbours { get; private set; }

        protected Point MapSize;
        
        public Biome(List<Point> points, Point center, bool bounded, Heightmap heightMap)
        {
            this.MapSize = new Point(heightMap.Width, heightMap.Height);
            this.Points = points;
            int MinDistanceFromBorder = 10;
            this.Center = center;
            this.Bounded = bounded;
            for (int i = 0; i < this.Points.Count; i++) {
                if (
                    this.Points[i].X > this.MapSize.X - MinDistanceFromBorder 
                    || this.Points[i].X < MinDistanceFromBorder 
                    || this.Points[i].Y < MinDistanceFromBorder 
                    || this.Points[i].Y > this.MapSize.X - MinDistanceFromBorder
                ) {
                    Bounded = false;
                    break;
                }
            }
        }

        public void SetBiomeType(BiomeType t)
        {
            BiomeType = t;
        }

        public void Set()
        {
            switch (BiomeType) {
                /*case BiomeType.River:
                    Height = 0;
                    break;*/
                case BiomeType.Plains:
                    Height = 10;
                    break;
                case BiomeType.Hills:
                    Height = 30;
                    break;
                case BiomeType.Forest:
                    Height = 10;
                    break;
                case BiomeType.Mountains:
                    Height = 80;
                    break;
            }
        }

        public void Draw(Bitmap bitmap, Graphics graphics, bool fill = true)
        {
            if (fill) {
                SolidBrush s = new SolidBrush(Color.FromArgb(this.Height, this.Height, this.Height));
                graphics.FillPolygon(s, Points.ToArray());
            }
        }

        public void PrepareNeightbours(BiomeCollection biomes)
        {
            this.Neightbours = new List<Neightbour>();
            foreach (Biome b in biomes) {
                bool found = false;
                List<Point> ready = new List<Point>();
                for (int x = 0; x < this.Points.Count; x++) {
                    for (int y = 0; y < b.Points.Count; y++) {
                        if (this.Points[x].Equals(b.Points[y])) {
                            ready.Add(b.Points[y]);
                            found = true;
                        }
                    }
                }
                if (found && !ready.SequenceEqual(this.Points)) this.Neightbours.Add(new Neightbour(ready, b));
            }
        }

        private bool isNeightbour(List<Point> points)
        {
            foreach (Point p1 in this.Points) {
                foreach (Point p2 in points) {
                    return p1.Equals(p2);
                }
            }
            return false;
        }

        public static bool IsPointInPolygon(Point[] polygon, Point point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++) {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) && (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)) {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        public bool Nearby(BiomeType type)
        {
            bool nearby = false;

            foreach (Neightbour n in Neightbours) {
                if (n.Biome.BiomeType == type) {
                    nearby = true;
                }
            }
            return nearby;
        }
    }
}
