using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Writing DLSE effects back into their sfx binders (DES BND3 big-endian + DCX_EDGE, DS2 BND4 + DFLT). Here rather
/// than in FxBank so Claude/Tools/FxModelCheck can run the same code over vanilla binders (FFX Older Games Plan §15).
/// </summary>
public static class FfxBinders
{
    /// <summary>
    /// The binder with some files replaced by name (every file of that name): the bytes to write (with the binder's own compression), and the
    /// same binder uncompressed in <paramref name="plain"/> (for the x.ffxbnd twin the game ships next to
    /// x.ffxbnd.dcx). Names the binder lacks are listed in <paramref name="missing"/> and skipped.
    /// </summary>
    public static byte[] Write(Memory<byte> raw, IReadOnlyList<(string Name, byte[] Bytes)> files, out List<string> missing, out byte[] plain)
    {
        Memory<byte> container = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        bool bnd3 = container.Length >= 4 && container.Span[..4].SequenceEqual("BND3"u8);
        IBinder bnd = bnd3 ? BND3.Read(raw) : BND4.Read(raw);

        // Every file with the name: DS2S's sfx9999_Append stores 54 effects twice under one name (identical copies),
        // which stay identical
        missing = new List<string>();
        foreach (var (name, data) in files)
        {
            bool found = false;
            foreach (var file in bnd.Files.Where(f => f.Name == name))
            {
                file.Bytes = data;
                found = true;
            }
            if (!found)
                missing.Add(name);
        }

        if (bnd3)
        {
            var b3 = (BND3)bnd;
            plain = b3.Write(DCX.Type.None);
            return b3.Write();
        }
        var b4 = (BND4)bnd;
        plain = b4.Write(DCX.Type.None);
        return b4.Write();
    }

    /// <summary>The uncompressed twin of a binder path (x.ffxbnd for x.ffxbnd.dcx), or null for a path without .dcx.</summary>
    public static string TwinOf(string path) =>
        path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? path[..^4] : null;
}
