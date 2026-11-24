using SoulsFormats;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO
{
    public class MapStaticLocationInfo
    {
        public string id = "";
        public AssetLocation? Msb;
        public List<AssetLocation> Btls = new();
        public AssetLocation? Nva;
        public List<AssetLocation> Btabs = new();
    }

    public class MapLocationInfo
    {
        public IMsb? msb;
    }
}