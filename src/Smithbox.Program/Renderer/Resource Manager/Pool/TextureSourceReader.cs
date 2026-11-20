#nullable enable
using SoulsFormats;
using StudioCore.Application;
using System;
using System.IO;

namespace StudioCore.Renderer;

/// <summary>
///     Reads a loaded texture again from the container it was loaded from. Textures loaded into a pool don't keep
///     their file data once they are on the GPU, so anything that needs the data itself (e.g. exporting) reads it
///     with this.
/// </summary>
public static class TextureSourceReader
{
    private const int MaxNesting = 4;

    /// <summary>
    ///     The texture a pool's handle was loaded from, with its data, or null if it can't be found (the handle isn't
    ///     loaded, or it came from bytes handed to the pool rather than a file).
    /// </summary>
    public static TPF.Texture? Read(ProjectEntry project, IResourceHandle handle)
    {
        ResourceLoadContext? source = handle.Source;
        if (source == null || string.IsNullOrEmpty(source.ContainerPath))
        {
            return null;
        }

        try
        {
            return Read(project, source.ContainerPath, source.Name);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(TextureSourceReader), $"Failed to read texture '{source.Name}' from '{source.ContainerPath}'.", e);
            return null;
        }
    }

    /// <summary>
    ///     Finds a texture by its pool name (see <see cref="ResourceName.Normalize" />) in a container: a TPF, a
    ///     binder, or a split binder, given by its project relative or absolute path.
    /// </summary>
    public static TPF.Texture? Read(ProjectEntry project, string containerPath, string textureName)
    {
        var name = ResourceName.Normalize(textureName);

        switch (ResourceClassifier.DetectContainer(containerPath))
        {
            case ContainerFormat.Tpf:
            {
                var bytes = ReadFile(project, containerPath);
                return bytes == null ? null : Find(TPF.Read(bytes.Value), name);
            }
            case ContainerFormat.Binder:
            {
                var bytes = ReadFile(project, containerPath);
                if (bytes == null)
                {
                    return null;
                }

                using BinderReader binder = OpenBinder(project, bytes.Value);
                return Find(project, binder, containerPath, name, 0);
            }
            case ContainerFormat.BinderSplit:
            {
                var i = containerPath.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);
                var dataPath = string.Concat(containerPath.AsSpan(0, i), "bdt", containerPath.AsSpan(i + 3));

                var bhd = ReadFile(project, containerPath);
                var bdt = ReadFile(project, dataPath);
                if (bhd == null || bdt == null)
                {
                    return null;
                }

                using BinderReader binder = UsesLegacyBinders(project)
                    ? new BXF3Reader(bhd.Value, bdt.Value)
                    : new BXF4Reader(bhd.Value, bdt.Value);
                return Find(project, binder, containerPath, name, 0);
            }
            default:
                return null;
        }
    }

    private static TPF.Texture? Find(ProjectEntry project, BinderReader binder, string containerPath, string name, int depth)
    {
        foreach (BinderFileHeader header in binder.Files)
        {
            var fileName = ResourceName.FileName(header.Name ?? "");
            if (fileName.Length == 0)
            {
                continue;
            }

            switch (ResourceClassifier.DetectContainer(fileName))
            {
                case ContainerFormat.Tpf:
                {
                    TPF.Texture? texture = Find(TPF.Read(binder.ReadFile(header)), name);
                    if (texture != null)
                    {
                        return texture;
                    }

                    break;
                }
                case ContainerFormat.Binder when depth < MaxNesting:
                {
                    using BinderReader nested = OpenBinder(project, binder.ReadFile(header));
                    TPF.Texture? texture = Find(project, nested, containerPath, name, depth + 1);
                    if (texture != null)
                    {
                        return texture;
                    }

                    break;
                }
                case ContainerFormat.BinderSplit when depth < MaxNesting:
                {
                    // A header whose data sits beside the binder, e.g. DS1's chrtpfbhd in a chrbnd (ContainerLoadAction)
                    var bdt = ReadFile(project, ResourceName.NestedSplitDataPath(containerPath, fileName));
                    if (bdt == null)
                    {
                        break;
                    }

                    using BinderReader nested = UsesLegacyBinders(project)
                        ? new BXF3Reader(binder.ReadFile(header), bdt.Value)
                        : new BXF4Reader(binder.ReadFile(header), bdt.Value);
                    TPF.Texture? texture = Find(project, nested, containerPath, name, depth + 1);
                    if (texture != null)
                    {
                        return texture;
                    }

                    break;
                }
            }
        }

        return null;
    }

    private static TPF.Texture? Find(TPF tpf, string name)
    {
        foreach (TPF.Texture texture in tpf.Textures)
        {
            if (string.Equals(ResourceName.Normalize(texture.Name), name, StringComparison.OrdinalIgnoreCase))
            {
                // Console textures are headerized by their platform, which a texture read on its own doesn't know
                texture.Platform = tpf.Platform;
                return texture;
            }
        }

        return null;
    }

    private static Memory<byte>? ReadFile(ProjectEntry project, string path)
    {
        // Containers outside the project (e.g. DS1's loose map textures, the bundled Smithbox textures) are absolute
        if (Path.IsPathRooted(path))
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        return project.VFS.FS.FileExists(path) ? project.VFS.FS.ReadFile(path) : null;
    }

    private static BinderReader OpenBinder(ProjectEntry project, Memory<byte> bytes)
    {
        return UsesLegacyBinders(project) ? new BND3Reader(bytes) : new BND4Reader(bytes);
    }

    private static bool UsesLegacyBinders(ProjectEntry project)
    {
        return project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }
}
