#nullable enable
using System;

namespace StudioCore.Renderer;

internal enum ContainerFormat
{
    /// <summary>Not a container: a resource file (model, collision, navmesh) or something unsupported.</summary>
    None,

    /// <summary>A single-file binder (BND3/BND4).</summary>
    Binder,

    /// <summary>A header/data pair (BXF3/BXF4), given by the header (*bhd). The data file is the sibling *bdt.</summary>
    BinderSplit,

    /// <summary>A texture pack.</summary>
    Tpf
}

internal enum ResourceKind
{
    None,
    Flver,
    Collision,
    NavmeshNVM,
    NavmeshHKX
}

/// <summary>
///     Decides what a file is from its name. This is the only place that maps file names to loaders.
/// </summary>
internal static class ResourceClassifier
{
    private static string Lower(string fileName)
    {
        var n = ResourceName.FileName(fileName).ToLowerInvariant();
        return n.EndsWith(".dcx") ? n.Substring(0, n.Length - 4) : n;
    }

    public static ContainerFormat DetectContainer(string pathOrName)
    {
        var n = Lower(pathOrName);

        if (n.EndsWith(".tpf"))
        {
            return ContainerFormat.Tpf;
        }

        if (n.EndsWith("bhd"))
        {
            return ContainerFormat.BinderSplit;
        }

        if (n.EndsWith("bnd"))
        {
            return ContainerFormat.Binder;
        }

        return ContainerFormat.None;
    }

    /// <param name="entryFileName">File name of the entry</param>
    /// <param name="containerPath">Path of the container it was found in (used to tell collision from navmesh hkx)</param>
    /// <param name="isLooseFile">The entry is not inside a binder</param>
    /// <param name="types">Kinds of resource the caller wants</param>
    public static ResourceKind Classify(string entryFileName, string containerPath, bool isLooseFile,
        ResourceType types)
    {
        var n = Lower(entryFileName);

        if (n.EndsWith(".flver") || n.EndsWith(".flv"))
        {
            return types.HasFlag(ResourceType.Flver) ? ResourceKind.Flver : ResourceKind.None;
        }

        if (n.EndsWith(".nvm"))
        {
            return types.HasFlag(ResourceType.Navmesh) ? ResourceKind.NavmeshNVM : ResourceKind.None;
        }

        if (n.EndsWith(".hkx"))
        {
            var wantsCollision = types.HasFlag(ResourceType.CollisionHKX);
            var wantsNavmesh = types.HasFlag(ResourceType.NavmeshHKX);

            // The caller was specific
            if (wantsCollision != wantsNavmesh)
            {
                return wantsCollision ? ResourceKind.Collision : ResourceKind.NavmeshHKX;
            }

            if (!wantsCollision)
            {
                return ResourceKind.None;
            }

            // Both allowed: hkx is used for many things (animations, skeletons...), so go by the container
            var c = containerPath.Replace('\\', '/').ToLowerInvariant();

            if (c.Contains("nvmhktbnd") || c.Contains("/nav"))
            {
                return ResourceKind.NavmeshHKX;
            }

            if (isLooseFile || c.Contains("hkxb") || c.Contains("/hit") || c.Contains("/connect"))
            {
                return ResourceKind.Collision;
            }
        }

        return ResourceKind.None;
    }

    public static Type? ResourceTypeOf(ResourceKind kind)
    {
        return kind switch
        {
            ResourceKind.Flver => typeof(FlverResource),
            ResourceKind.Collision => typeof(HavokCollisionResource),
            ResourceKind.NavmeshNVM => typeof(NVMNavmeshResource),
            ResourceKind.NavmeshHKX => typeof(HavokNavmeshResource),
            _ => null
        };
    }
}
