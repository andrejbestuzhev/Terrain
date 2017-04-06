# Terrain generator

Simple heightmap generator based on simplex noise.

## Usage
	HeightMapGenerator heightmapGenerator = new Heightmap(WorldWidth, WorldHeight, 1234);
	heightMapGenerator.Generate();
	Bitmap heightmap = heightmapGenerator.Result();

