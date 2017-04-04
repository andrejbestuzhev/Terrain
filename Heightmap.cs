using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TerrainGenerator
{
    public class Heightmap
    {
        
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public static Random Random { get; protected set; }

        protected int Seed;
        
        protected TerrainType TerrainType;
        protected Bitmap HeightmapTexture;
        protected int MinDistanceBetweenTriangles;
        protected Detalization Detalization;

        public Heightmap(int width, int height, int seed, TerrainType terrainType)
        {
            this.HeightmapTexture = new Bitmap(width, height);
            this.Width = width;
            this.Height = height;
            this.Seed = seed;
            Random = new Random(seed);
            this.TerrainType = terrainType;
            this.MinDistanceBetweenTriangles = (int)((float)width * 0.05f);
            this.Detalization = Detalization.Low;
        }

        public void SetDetalization(Detalization detalization)
        {
            this.Detalization = detalization;
        }

        public void Generate()
        {
            BiomeCollection biomes = this.CreateBiomes(this.CreatePoints());
            biomes.SetBiomeTypes();
            for (int i = 0; i < biomes.Count; i++) {
                biomes[i].Set();
            }

            Dictionary<BiomeType, Bitmap> BiomeLayers = new Dictionary<BiomeType, Bitmap>();
            using (Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb)) {
                using (Graphics g = Graphics.FromImage(b)) {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    foreach (Biome s in biomes) {
                        s.Draw(b, g, true);
                    }
                }
                this.HeightmapTexture = (Bitmap)b.Clone();
            }
        }

        /// <summary>
        /// Preparing points for Voronoi triangles
        /// </summary>
        protected List<Point> CreatePoints()
        {
            List<Point> points = new List<Point>();
            int _try = 0;
            int pointsCount = (int)Math.Sqrt(this.Width) * (int)this.Detalization; // total triangles
            float minDistance = float.MaxValue;
            for (int i = 0; i < pointsCount; i++) {
                int x = (int)(Random.NextDouble() * this.Width);
                int y = (int)(Random.NextDouble() * this.Height);
                Point p = new Point(x, y);
                for (int n = 0; n < points.Count; n++) {
                    float distance = Heightmap.Distance(p, points[n]); // distance between triangle center
                    if (distance < this.MinDistanceBetweenTriangles) {
                        minDistance = distance;
                    }
                }

                if (minDistance < MinDistanceBetweenTriangles) {
                    if (_try < 10) {
                        _try++;
                        i--;
                        continue;
                    }
                    _try = 0;
                }
                points.Add(p);
            }
            return points;
        }

        protected BiomeCollection CreateBiomes(List<Point> points)
        {
            // Generating triangle mesh

            TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry();
            foreach (Point p in points) {
                geometry.AddPoint(p.X, p.Y);
            }

            TriangleNet.Mesh mesh = new TriangleNet.Mesh();
            mesh.Behavior.ConformingDelaunay = true;
            mesh.Triangulate(geometry);

            TriangleNet.Tools.Voronoi voronoi = new TriangleNet.Tools.Voronoi(mesh);
            BiomeCollection biomes = new BiomeCollection(this.Width, this.Height);
            foreach (TriangleNet.Tools.VoronoiRegion region in voronoi.Regions) {
                List<Point> biomePoints = new List<Point>();
                for (int i = 0; i < region.Vertices.Count; i++) {
                    biomePoints.Add(
                        new Point(
                            (int)region.Vertices.ElementAt(i).X,
                            (int)region.Vertices.ElementAt(i).Y
                        )
                    );
                }
                biomes.Add(new Biome(
                    biomePoints,
                    new Point((int)region.Generator.X, (int)region.Generator.Y),
                    region.Bounded,
                    this
                ));
            }
            biomes.PrepareNeightbours();
            biomes.SetBiomeTypes();
            return biomes;
        }

        public static float Distance(Point p1, Point p2)
        {
            int x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            return (float)(Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2)));
        }

        public Bitmap Result()
        {
            return this.HeightmapTexture;
        }
    }
}
