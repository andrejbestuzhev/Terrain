using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Terrain
{
    public class Heightmap
    {
        protected Bitmap HeightmapTexture;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        protected Random Random;

        public Heightmap(int width, int height, int seed)
        {
            this.HeightmapTexture = new Bitmap(width, height);
            this.Width = width;
            this.Height = height;
            this.Random = new Random(seed);
        }
    }
}
