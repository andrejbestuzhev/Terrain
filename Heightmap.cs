using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace TerrainGenerator
{
    public class Heightmap
    {
        
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public static Random Random { get; protected set; }

        protected int Seed;
        
        protected Bitmap HeightmapTexture;
        protected int MinDistanceBetweenTriangles;

        /// <summary>
        /// Creates heightmap object
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="seed">Seed for random number generator</param>
        public Heightmap(int width, int height, int seed)
        {
            this.HeightmapTexture = new Bitmap(width, height);
            this.Width = width;
            this.Height = height;
            this.Seed = seed;
            Random = new Random(seed);
            this.MinDistanceBetweenTriangles = (int)((float)width * 0.05f);
        }

        public void Generate()
        {
            float multiplier = 10f; // magic number. Change
            int steps = 3; // magic number. Change
            BiomeCollection biomes = this.CreateBiomes(this.CreatePoints());
            biomes.SetBiomeTypes();
            for (int i = 0; i < biomes.Count; i++) {
                biomes[i].Set();
            }

            // Drawing biome based heightmap
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
            
            for (int i = 1; i < steps; i++) {
                for (int x = 0; x < this.Width; x++) {
                    for (int y = 0; y < this.Height; y++) {

                        // height for each pixel
                        int heightAspect = (int)(
                            (
                                SimplexNoise.Generate(
                                    (float)x / (float)Math.Ceiling(Math.Pow(this.Width, (double) 1 / i)),
                                    (float)y / (float)Math.Ceiling(Math.Pow(this.Height, (double) 1 / i))
                                    ) + 1f // avoid negative height
                            ) * Math.Pow(multiplier, steps-i)
                        );
                        Color px = this.HeightmapTexture.GetPixel(x, y);
                        int newColor = px.R + heightAspect;
                        if (newColor > 255) {
                            newColor = 255;
                        }
                        this.HeightmapTexture.SetPixel(x, y, Color.FromArgb(newColor, newColor, newColor));
                    }
                }
                for (int a = 0; a < i; a++) {
                    // blur for each step
                    this.HeightmapTexture = this.Blur(this.HeightmapTexture);
                }
            }

            // blurring
            this.HeightmapTexture = this.Blur(this.HeightmapTexture);
            //this.removeBlurArtifacts();
            //this.Lowerize();
        }

        /// <summary>
        /// Image blur. Need to be changed because of artefacts on sides
        /// </summary>
        /// <param name="source">Source image</param>
        /// <returns></returns>
        protected Bitmap Blur(Bitmap source)
        {
            return ExtentedBitmap.ExtBitmap.ImageBlurFilter(this.HeightmapTexture, ExtentedBitmap.ExtBitmap.BlurType.Mean9x9);
        }

        /*protected void removeBlurArtifacts()
        {
            int offset = 6;  // magic number. Change
            this.HeightmapTexture = this.HeightmapTexture.Clone(
                new Rectangle(offset, offset, this.Width - offset * 2, this.Height - offset * 2),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );
            this.HeightmapTexture = new Bitmap(this.HeightmapTexture, this.Width, this.Height);
        }

        protected void Lowerize()
        {
            int minPoint = int.MaxValue;
            for (int x = 0; x < this.Width; x++) {
                for (int y = 0; y < this.Height; y++) {
                    int minPointTest = this.HeightmapTexture.GetPixel(x, y).R;
                    if (minPointTest < minPoint && minPointTest != 0) {
                        minPoint = minPointTest;
                    }
                }
            }

            for (int x = 0; x < this.Width; x++) {
                for (int y = 0; y < this.Height; y++) {
                    int setColor = this.HeightmapTexture.GetPixel(x, y).R - minPoint;
                    this.HeightmapTexture.SetPixel(x, y, Color.FromArgb(setColor, setColor, setColor));
                }
            }
        }*/

        /// <summary>
        /// Preparing points for Voronoi triangles
        /// </summary>
        protected List<Point> CreatePoints()
        {
            List<Point> points = new List<Point>();
            int _try = 0;
            int pointsCount = this.Width / 2;
            
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

        protected TriangleNet.Tools.Voronoi GenerateVoronoiTriangles(List<Point> points)
        {
            TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry();
            foreach (Point p in points) {
                geometry.AddPoint(p.X, p.Y);
            }

            TriangleNet.Mesh mesh = new TriangleNet.Mesh();
            mesh.Behavior.ConformingDelaunay = true;
            mesh.Triangulate(geometry);

            return new TriangleNet.Tools.Voronoi(mesh);

        }

        protected BiomeCollection CreateBiomes(List<Point> points)
        {
            TriangleNet.Tools.Voronoi voronoi = this.GenerateVoronoiTriangles(points);
            
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
