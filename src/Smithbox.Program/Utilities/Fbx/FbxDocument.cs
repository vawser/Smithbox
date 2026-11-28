#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Utilities.Fbx;

/// <summary>
///     An FBX file as its node tree. Reads binary FBX (6.x–7.7) and ASCII FBX (6.x, 7.x); writes binary FBX.
///     The scene meaning of the nodes (models, curves, connections) is a separate layer.
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public sealed class FbxDocument
{
    /// <summary>The version written by default: 7.4, read by every DCC tool since 2014.</summary>
    public const int DefaultVersion = 7400;

    /// <summary>File version, e.g. 7400 for 7.4. Versions from 7500 use 64-bit offsets in binary files.</summary>
    public int Version = DefaultVersion;

    /// <summary>The source file was binary (false: ASCII, or built in code).</summary>
    public bool IsBinary;

    /// <summary>The top-level nodes (FBXHeaderExtension, GlobalSettings, Objects, Connections, Takes...).</summary>
    public List<FbxNode> Nodes = new();

    /// <summary>
    ///     Binary only: the bytes after the last top-level node. Its first 16 bytes (the FBX SDK's per-file id) are
    ///     written back; null writes the standard id.
    /// </summary>
    public byte[]? Footer;

    /// <summary>
    ///     Diagnostics from a binary read: nodes whose end marker didn't follow the writer's rule (a marker when the
    ///     node has children or no values). Such a node is read correctly but won't be written back byte for byte.
    /// </summary>
    public int SentinelRuleMismatches;

    /// <summary>The first few such nodes, as "name (values, children, marker)", for diagnostics.</summary>
    public List<string> SentinelRuleMismatchNodes = new();

    /// <summary>The first top-level node with this name, or null.</summary>
    public FbxNode? Find(string name) => Nodes.FirstOrDefault(n => n.Name == name);

    public int CountNodes() => Nodes.Sum(n => n.CountNodes());

    public static bool IsBinaryData(ReadOnlySpan<byte> data) => FbxBinary.HasMagic(data);

    /// <summary>Reads a binary or ASCII FBX file.</summary>
    public static FbxDocument Read(byte[] data) =>
        IsBinaryData(data) ? FbxBinary.Read(data) : FbxAscii.Read(data);

    public static FbxDocument Load(string path) => Read(File.ReadAllBytes(path));

    /// <summary>Writes the document as binary FBX at <paramref name="version" /> (default: its own version).</summary>
    public byte[] Write(int? version = null) => FbxBinary.Write(this, version ?? Version);

    public void Save(string path, int? version = null) => File.WriteAllBytes(path, Write(version));

    /// <summary>
    ///     The first difference between two node trees (names, values and children, in order), or null when they are
    ///     equal. The path names the node, e.g. "Objects/Model[3]/Properties70/P[5]".
    /// </summary>
    public static string? Difference(FbxDocument a, FbxDocument b) => Difference(a.Nodes, b.Nodes, "");

    private static string? Difference(List<FbxNode> a, List<FbxNode> b, string path)
    {
        if (a.Count != b.Count)
            return $"{(path == "" ? "(root)" : path)}: {a.Count} vs {b.Count} child nodes";

        var seen = new Dictionary<string, int>();
        for (var i = 0; i < a.Count; i++)
        {
            var x = a[i];
            var y = b[i];
            seen.TryGetValue(x.Name, out var n);
            seen[x.Name] = n + 1;
            var here = $"{path}{(path == "" ? "" : "/")}{x.Name}{(n > 0 ? $"[{n}]" : "")}";

            if (x.Name != y.Name)
                return $"{here}: name {x.Name} vs {y.Name}";
            if (x.EmptyBody != y.EmptyBody)
                return $"{here}: empty body {x.EmptyBody} vs {y.EmptyBody}";
            if (x.Properties.Count != y.Properties.Count)
                return $"{here}: {x.Properties.Count} vs {y.Properties.Count} values";
            for (var p = 0; p < x.Properties.Count; p++)
            {
                if (!x.Properties[p].ContentEquals(y.Properties[p]))
                    return $"{here}: value {p}: {x.Properties[p]} vs {y.Properties[p]}";
            }

            var d = Difference(x.Children, y.Children, here);
            if (d != null)
                return d;
        }

        return null;
    }
}
