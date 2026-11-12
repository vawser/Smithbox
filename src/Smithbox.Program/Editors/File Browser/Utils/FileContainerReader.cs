using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.FileBrowser;

public enum FileContainerKind
{
    Binder,
    Tpf
}

/// <summary>
/// A file inside a binder. Read() returns its bytes on demand.
/// </summary>
public sealed class FileContainerEntry
{
    public string Name;
    public Func<Memory<byte>> Read;

    public bool IsTpf => LocatorUtils.IsTPF(Name);
}

/// <summary>
/// A container opened from the VFS: a binder (BND3/BND4/BXF3/BXF4) with its files, or a loose TPF
/// </summary>
public sealed class FileContainer
{
    public FileContainerKind Kind;

    // As stored: the DCX compression around it (None when uncompressed) and the format name from its magic
    public DCX.Type Compression = DCX.Type.None;
    public string Format = "";

    public List<FileContainerEntry> Files = new();

    public TPF Tpf;

    public FileContainerEntry FindFile(string name)
    {
        return Files.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// The one place the File Browser opens containers. The binder version is taken from the file's own magic, not the
/// game, and a BXF's data file is found by swapping the extension's "bhd" for "bdt".
/// </summary>
public static class FileContainerReader
{
    /// <summary>
    /// Whether a path could be a container, going by its extension. Used to avoid reading large files
    /// (movies, sound archives, .bdt halves) that can never be one.
    /// </summary>
    public static bool MayBeContainer(string path)
    {
        var name = Path.GetFileName(path ?? "").ToLowerInvariant();

        if (name.EndsWith(".dcx"))
            name = name[..^4];

        return name.EndsWith("bnd") || name.EndsWith("bhd") || name.EndsWith(".tpf");
    }

    /// <summary>
    /// The .bdt path for a .bhd path: only the extension changes ("/map/m10/m10.tpfbhd" -> "/map/m10/m10.tpfbdt")
    /// </summary>
    public static string GetBdtPath(string bhdPath)
    {
        var slash = bhdPath.LastIndexOfAny(new[] { '/', '\\' });
        var index = bhdPath.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);

        if (index < 0 || index < slash)
            return null;

        return bhdPath[..index] + "bdt" + bhdPath[(index + 3)..];
    }

    /// <summary>
    /// Opens the container at path, or returns null when it isn't one. Throws when the file can't be read or parsed.
    /// </summary>
    public static FileContainer Open(VirtualFileSystem fs, string path)
    {
        if (!MayBeContainer(path))
            return null;

        var raw = fs.ReadFile(path);
        if (raw == null)
            throw new FileNotFoundException($"File not found in the VFS: {path}");

        return OpenBytes(raw.Value, path, bdtPath => fs.ReadFile(bdtPath));
    }

    /// <summary>
    /// Opens container bytes (DCX or not), or returns null when they aren't a container. name is used to find a
    /// BXF's data file, which resolveSibling returns by path (from the VFS, or from the same parent binder).
    /// </summary>
    public static FileContainer OpenBytes(Memory<byte> raw, string name, Func<string, Memory<byte>?> resolveSibling)
    {
        var data = raw;
        var compression = DCX.Type.None;

        if (DCX.Is(data))
            data = DCX.Decompress(data, out compression);

        var magic = GetMagic(data);

        var container = OpenDecompressed(name, data, magic, resolveSibling);

        if (container != null)
        {
            container.Compression = compression;
            container.Format = magic.TrimEnd('\0');
        }

        return container;
    }

    /// <summary>
    /// Separates the names in a nested entry key ("c0000.chrtpfbhd|c0000_a.tpf")
    /// </summary>
    public const char KeySeparator = '|';

    /// <summary>
    /// Reads an entry inside a container by its key: entry names from the outer container inwards, joined by '|'.
    /// Returns null when a name on the way is missing or isn't a container.
    /// </summary>
    public static Memory<byte>? ReadByKey(FileContainer root, string key)
    {
        var parts = (key ?? "").Split(KeySeparator);
        var container = root;

        for (var i = 0; i < parts.Length; i++)
        {
            var entry = container?.FindFile(parts[i]);
            if (entry == null)
                return null;

            var data = entry.Read();

            if (i == parts.Length - 1)
                return data;

            container = OpenBytes(data, entry.Name, SiblingResolver(container));
        }

        return null;
    }

    /// <summary>
    /// Finds a BXF's data file next to it in the same binder, by file name
    /// </summary>
    public static Func<string, Memory<byte>?> SiblingResolver(FileContainer parent)
    {
        return path =>
        {
            var name = Path.GetFileName(path.Replace('\\', '/'));
            var sibling = parent.Files.FirstOrDefault(f =>
                string.Equals(Path.GetFileName(f.Name.Replace('\\', '/')), name, StringComparison.OrdinalIgnoreCase));

            return sibling?.Read();
        };
    }

    private static FileContainer OpenDecompressed(string path, Memory<byte> data, string magic, Func<string, Memory<byte>?> resolveSibling)
    {
        switch (magic)
        {
            case "BND3":
                return FromBinder(new BND3Reader(data));

            case "BND4":
                return FromBinder(new BND4Reader(data));

            case "BHF3":
            case "BHF4":
                {
                    var bdtPath = GetBdtPath(path);
                    var bdt = bdtPath != null ? resolveSibling?.Invoke(bdtPath) : null;

                    if (bdt == null)
                        throw new FileNotFoundException($"Data file for {path} not found: {bdtPath}");

                    BinderReader reader = magic == "BHF3"
                        ? new BXF3Reader(data, bdt.Value)
                        : new BXF4Reader(data, bdt.Value);

                    return FromBinder(reader);
                }

            case "TPF\0":
                return new FileContainer
                {
                    Kind = FileContainerKind.Tpf,
                    Tpf = TPF.Read(data)
                };
        }

        return null;
    }

    /// <summary>
    /// The game-archive entry for a path, found by walking the archive's folder tree, or null when the file isn't in
    /// the archives (loose games, loose files, project-only files). Unlike VFS.TryGetFile this never falls back to
    /// scanning every archived file when a path is missing.
    /// </summary>
    public static ArchiveBinderVirtualFileSystem.BinderVirtualFile FindArchiveFile(ProjectEntry project, string path)
    {
        if (project.VFS.VanillaBinderFS is not ArchiveBinderVirtualFileSystem archiveFs)
            return null;

        var parts = (path ?? "").ToLowerInvariant().Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return null;

        VirtualDirectory dir = archiveFs.FsRoot;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!dir.TryGetDirectory(parts[i], out dir))
                return null;
        }

        return dir.TryGetFile(parts[^1], out var file)
            ? file as ArchiveBinderVirtualFileSystem.BinderVirtualFile
            : null;
    }

    /// <summary>
    /// Reads a TPF from bytes (DCX or not)
    /// </summary>
    public static TPF ReadTpf(Memory<byte> data)
    {
        return TPF.Read(data);
    }

    private static FileContainer FromBinder(BinderReader reader)
    {
        var container = new FileContainer { Kind = FileContainerKind.Binder };

        // The reader keeps the binder's bytes; each entry reads its own slice when asked
        foreach (var header in reader.Files)
        {
            var captured = header;

            container.Files.Add(new FileContainerEntry
            {
                Name = header.Name ?? "",
                Read = () => reader.ReadFile(captured)
            });
        }

        return container;
    }

    private static string GetMagic(Memory<byte> data)
    {
        if (data.Length < 4)
            return "";

        return Encoding.ASCII.GetString(data.Span[..4]);
    }
}
