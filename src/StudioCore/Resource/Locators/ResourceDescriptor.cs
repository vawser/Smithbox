using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;

/// <summary>
///     Generic asset description for a generic game asset
/// </summary>
public class ResourceDescriptor : IComparable<ResourceDescriptor>
{
    public string AssetArchiveVirtualPath;

    /// <summary>
    ///     Where applicable, the numeric asset ID. Usually applies to chrs, objs, and various map pieces
    /// </summary>
    public int AssetID;

    /// <summary>
    ///     Pretty UI friendly name for an asset. Usually the file name without an extention i.e. c1234
    /// </summary>
    public string AssetName;

    /// <summary>
    ///     Absolute path of where the full asset is located. If this asset exists in a mod override directory,
    ///     then this path points to that instead of the base game asset.
    /// </summary>
    public string AssetPath;

    /// <summary>
    ///     Virtual friendly path for this asset to use with the resource manager
    /// </summary>
    public string AssetVirtualPath;


    public override int GetHashCode()
    {
        if (AssetVirtualPath != null)
            return AssetVirtualPath.GetHashCode();

        if (AssetPath != null)
            return AssetPath.GetHashCode();

        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is ResourceDescriptor ad)
        {
            if (AssetVirtualPath != null)
                return AssetVirtualPath.Equals(ad.AssetVirtualPath);

            if (AssetPath != null)
                return AssetPath.Equals(ad.AssetPath);
        }

        return base.Equals(obj);
    }

    public int CompareTo(ResourceDescriptor other)
    {
        // Use string default CompareTo logic
        return AssetName.CompareTo(other.AssetName);
    }

    public bool IsValid()
    {
        if (AssetVirtualPath == null && AssetArchiveVirtualPath == null)
            return false;

        return true;
    }
}
