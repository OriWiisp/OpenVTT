namespace OpenVTT.Models
{
    public enum MapTile { Empty, Floor, Wall, Door, Water, Trap }

    public class MapModel
    {
        public int Width { get; }
        public int Height { get; }
        public MapTile[,] Tiles { get; }

        public MapModel(int width = 24, int height = 16)
        {
            Width = width; Height = height;
            Tiles = new MapTile[Width, Height];
        }

        public MapTile Get(int x, int y) => InBounds(x,y) ? Tiles[x,y] : MapTile.Empty;
        public void Set(int x, int y, MapTile t) { if (InBounds(x,y)) Tiles[x,y] = t; }
        public bool InBounds(int x, int y) => x>=0 && y>=0 && x<Width && y<Height;
        public void Fill(MapTile t)
        {
            for (int y=0;y<Height;y++) for (int x=0;x<Width;x++) Tiles[x,y]=t;
        }
    }
}
