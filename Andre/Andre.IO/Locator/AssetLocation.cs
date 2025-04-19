using Andre.IO.VFS;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO
{
    /// <summary>
    /// Describes the location and name of an asset which may or may not be contained within an archive
    /// </summary>
    public class AssetLocation(AssetLocation? containingArchive, string? assetPath, string? assetName)
        : IComparable<AssetLocation>
    {
        /// <summary>
        /// The path to the archive that contains this asset, if this asset is contained by an archive.
        /// </summary>
        public readonly AssetLocation? ContainingArchive = containingArchive;

        /// <summary>
        /// The path to this asset.
        /// If this asset is contained by an archive, this path includes the "virtual path" of the archive.
        /// </summary>
        public readonly string? AssetPath = assetPath;

        /// <summary>
        /// The name of this asset.
        /// </summary>
        public readonly string? AssetName = assetName;

        public bool IsValid => AssetPath != null;

        public AssetLocation(string? assetPath, string? assetName) : this(null, assetPath, assetName) { }

        public override int GetHashCode()
        {
            return AssetPath?.GetHashCode() ?? base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not AssetLocation a)
                return base.Equals(obj);

            if (AssetPath == null)
                return a.AssetPath == null;
            return AssetPath.Equals(a.AssetPath);
        }

        public int CompareTo(AssetLocation? a)
            => string.Compare(AssetName, a?.AssetName, StringComparison.CurrentCulture);

        public static bool operator ==(AssetLocation left, AssetLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetLocation left, AssetLocation right)
        {
            return !(left == right);
        }
    }

    public class BndLocation(AssetLocation? containingArchive, string? assetPath, string? assetName,
        string? archiveVirtualPath)
        : AssetLocation(containingArchive, assetPath, assetName)
    {
        /// <summary>
        /// The directory path that serves as the root for all files in this bnd.
        /// For example, if ArchiveVirtualPath = "map/tex/m00/0000", and a file it contains is named "parts/tex0.tpf",
        /// that texture's path is considered to be "map/tex/m00/m0000/parts/tex0.tpf".
        /// </summary>
        public string? ArchiveVirtualPath = archiveVirtualPath;

        public BndLocation(string? assetPath, string? assetName, string? archiveVirtualPath)
            : this(null, assetPath, assetName, archiveVirtualPath)
        { }
    }

    public class BxfLocation(AssetLocation? bhdContainingArchive, string? bhdPath, string? assetName,
        AssetLocation? bdtLocation, string? archiveVirtualPath)
        : AssetLocation(bhdContainingArchive, bhdPath, assetName)
    {
        public AssetLocation? BdtLocation = bdtLocation;
        public AssetLocation BhdLocation => this;
        /// <summary>
        /// The directory path that serves as the root for all files in this bnd.
        /// For example, if ArchiveVirtualPath = "map/tex/m00/0000", and a file it contains is named "parts/tex0.tpf",
        /// that texture's path is considered to be "map/tex/m00/m0000/parts/tex0.tpf".
        /// </summary>
        public string? ArchiveVirtualPath = archiveVirtualPath;

        public BxfLocation(string? bhdPath, string? assetname, AssetLocation? bdtLocation, string? archiveVirtualPath)
            : this(null, bhdPath, assetname, bdtLocation, archiveVirtualPath)
        { }
    }

    public class CombinedBxfLocation(string name, params BxfLocation[] locations)
        : AssetLocation(locations[0].AssetPath, name)
    {
        public BxfLocation[] Locations { get; } = locations;
    }

    public class TpfLocation(
        AssetLocation? containingArchive,
        string? assetPath,
        string? assetName,
        string? archiveVirtualPath)
        : AssetLocation(containingArchive, assetPath, assetName)
    {
        /// <summary>
        /// The directory path that serves as the root for all files in this bnd.
        /// For example, if ArchiveVirtualPath = "map/tex/m00/0000", and a file it contains is named "parts/tex0.tpf",
        /// that texture's path is considered to be "map/tex/m00/m0000/parts/tex0.tpf".
        /// </summary>
        public string? ArchiveVirtualPath = archiveVirtualPath;

        public TpfLocation(string? assetPath, string? assetName, string? archiveVirtualPath)
            : this(null, assetPath, assetName, archiveVirtualPath)
        { }

    }

    /// <summary>
    /// Represents the location of a file that may or may not exist.
    /// If the asset is contained by one or more asset(s), any asset in that chain may or may not exist -
    /// e.g., if the location is for a tpf in a tpfbnd, that tpfbnd may or may not exist.
    /// </summary>
    /// <param name="location"></param>
    /// <typeparam name="T"></typeparam>
    public class MaybeLocation<T>(T location) where T : AssetLocation
    {
        public T Location { get; } = location;
    }
}