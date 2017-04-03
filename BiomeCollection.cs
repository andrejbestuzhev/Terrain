using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace TerrainGenerator
{

    /// <summary>
    /// Biome collection
    /// </summary>
    public class BiomeCollection : List<Biome>
    {
        public int Width;
        public int Height;

        public BiomeCollection(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public static BiomeCollection Fill(List<Biome> biomes, int width, int height)
        {
            BiomeCollection bc = new BiomeCollection(width, height);
            foreach (Biome b in biomes) {
                bc.Add(b);
            }
            return bc;
        }

        /// <summary>
        /// Getting biome in selected point. Not used yet
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>

        public Biome GetAt(int x, int y)
        {
            foreach (Biome b in this) {
                Point[] p = new Point[b.Points.Count];
                for (int i = 0; i < p.Length; i++) {
                    p[i] = b.Points[i];
                }
                if (Biome.IsPointInPolygon(p, b.Center)) {
                    return b;
                }
            }
            return default(Biome);
        }

        public void PrepareNeightbours()
        {
            foreach (Biome b in this) {
                b.PrepareNeightbours(this);
            }
        }

        public void SetBiomeTypes()
        {
            SetForest();
            SetHills();
            SetMountains();
        }

        private void SetForest()
        {
            foreach (Biome b in this.Where(b => b.BiomeType == BiomeType.Plains)) {
                if ((float)Heightmap.Random.NextDouble() > 0.7f) b.SetBiomeType(BiomeType.Forest);
            }
        }

        private void SetHills()
        {
            foreach (Biome b in this.Where(b => b.BiomeType == BiomeType.Plains)) {
                if ((float)Heightmap.Random.NextDouble() > 0.9f) b.SetBiomeType(BiomeType.Hills);
            }
        }

        private void SetMountains()
        {
            foreach (Biome b in this.Where(b => b.BiomeType == BiomeType.Hills)) {
                if ((float)Heightmap.Random.NextDouble() > 0.9f) {
                    b.SetBiomeType(BiomeType.Mountains);
                    foreach (Neightbour n in b.Neightbours) {
                        n.Biome.SetBiomeType(BiomeType.Hills);
                    }
                }
            }
        }
    }
}
