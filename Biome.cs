using System;
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
                case BiomeType.River:
                    Height = 0;
                    break;
                case BiomeType.Plains:
                    Height = 10;
                    break;
                case BiomeType.Hills:
                    Height = 20;
                    break;
                case BiomeType.Forest:
                    Height = 20;
                    break;
                case BiomeType.Mountains:
                    Height = 40;
                    break;
            }
        }

        public void Draw(Bitmap bitmap, Graphics graphics, bool fill = true)
        {
            if (fill) {
                Color c = Color.White;
                switch (BiomeType) {
                    case BiomeType.Forest:
                        c = Color.ForestGreen;
                        break;
                    case BiomeType.Plains:
                        c = Color.Green;
                        break;
                    case BiomeType.Hills:
                        c = Color.LightGreen;
                        break;
                    case BiomeType.River:
                        c = Color.Blue;
                        break;
                    case BiomeType.Mountains:
                        c = Color.Black;
                        break;
                }
                SolidBrush s = new SolidBrush(c);
                graphics.FillPolygon(s, Points.ToArray());
            }
        }

        /*public void HeightmapFill(Bitmap bitmap, Graphics graphics)
        {
            Color c = Color.FromArgb(0, 0, 0);
            this.Height /= 2;
            switch (BiomeType) {
                case BiomeType.Mountains:
                    System.Drawing.Drawing2D.PathGradientBrush pgb = new System.Drawing.Drawing2D.PathGradientBrush(Points.ToArray());
                    pgb.CenterColor = Color.FromArgb(this.Height + 20, this.Height + 20, this.Height + 20);
                    pgb.CenterPoint = Center;

                    pgb.SurroundColors = new[] {
                        Color.FromArgb(this.Height,this.Height,this.Height)
                    };
                    graphics.FillPolygon(pgb, Points.ToArray());
                    return;
                case BiomeType.Lake:
                    System.Drawing.Drawing2D.PathGradientBrush pgba = new System.Drawing.Drawing2D.PathGradientBrush(Points.ToArray());
                    pgba.CenterColor = Color.FromArgb(0, 0, 0);
                    pgba.CenterPoint = Center;

                    pgba.SurroundColors = new[] {
                        Color.FromArgb(this.Height,this.Height,this.Height)
                    };
                    graphics.FillPolygon(pgba, Points.ToArray());
                    return;
                default:
                    c = Color.FromArgb(this.Height, this.Height, this.Height);
                    break;
            }
            SolidBrush b = new SolidBrush(c);
            graphics.FillPolygon(b, Points.ToArray());
        }*/

        public void PrepareNeightbours(BiomeCollection biomes)
        {
            this.Neightbours = new List<Neightbour>();
            foreach (Biome b in biomes) {
                if (!b.Points.Equals(this.Points)) {
                    if (b.isNeightbour(this.Points)) {
                        this.Neightbours.Add(new Neightbour(new List<Point>(), b));
                    }
                }
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
