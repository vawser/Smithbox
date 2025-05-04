using Andre.Core;
using Andre.IO.VFS;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO.Asset
{
    public abstract class Asset<T>(AssetLocation location, VirtualFileSystem vfs, Game game)
        where T : Asset<T>
    {
        public bool IsLoaded { get; internal set; }
        internal Task<T>? loadingTask = null;
        public AssetLocation Location { get; } = location;
        public VirtualFileSystem Vfs { get; } = vfs;
        public Game Game { get; } = game;

        public abstract Task<T> Load();
    }

    public class MapAsset(AssetLocation location, VirtualFileSystem vfs, Game game)
        : Asset<MapAsset>(location, vfs, game)
    {
        public override async Task<MapAsset> Load()
        {
            throw new Exception();
        }
    }
    public class AssetManager
    {

    }
}