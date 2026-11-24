using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Whole-effect helpers for the older formats (FX.md, FFX §22, F8): reading and writing an effect file of any
/// format, its id, the effect calls that point at another id, and what kind of file it is. SoulsFormats + BCL only, so
/// Claude/Tools/FxModelCheck runs the same code.
/// </summary>
public static class FfxEffectFile
{
    /// <summary>The file object of an effect of the given format: <see cref="FXR"/>, <see cref="FFXDLSE"/> or <see cref="FXR1"/>.</summary>
    public static object Read(FxFormat format, Memory<byte> bytes) => format switch
    {
        FxFormat.Dlse => FFXDLSE.Read(bytes),
        FxFormat.Fxr1 => FXR1.Read(bytes),
        _ => FXR.Read(bytes),
    };

    /// <summary>The file's bytes.</summary>
    public static byte[] Write(object file)
    {
        switch (file)
        {
            case FXR fxr:
                if (fxr.Version == FXR.FXRVersion.Sekiro)
                    fxr.RebuildReferences();
                return fxr.Write();
            case FFXDLSE ffx:
                return ffx.Write();
            case FXR1 fxr1:
                return fxr1.Write();
            default:
                throw new ArgumentException($"Not an effect file: {file?.GetType().Name ?? "null"}");
        }
    }

    /// <summary>The format of a file object.</summary>
    public static FxFormat FormatOf(object file) => file switch
    {
        FFXDLSE => FxFormat.Dlse,
        FXR1 => FxFormat.Fxr1,
        _ => FxFormat.Fxr3,
    };

    /// <summary>A copy that shares nothing with the original (written and read back).</summary>
    public static object Copy(object file) => Read(FormatOf(file), Write(file));

    /// <summary>The effect id the file stores (the root's), or null.</summary>
    public static int? GetId(object file) => file switch
    {
        FXR fxr => fxr.Id,
        FFXDLSE ffx => ffx.Effect?.ID,
        FXR1 { RootFXNode: FXR1.FXNode.EffectCreateNode133 r } => r.EffectID,
        FXR1 { RootFXNode: FXR1.FXNode.EffectCreateNode134 m } => m.EffectID,
        _ => null,
    };

    /// <summary>
    /// Sets the effect id everywhere the file stores it. In every vanilla file of the six older games these all equal
    /// the id in the file name (§22.6): DLSE FXEffect.ID and each DES variant's effect; FXR1 root 133 or 134, and each
    /// 133 under a 134.
    /// </summary>
    public static void SetId(object file, int id)
    {
        switch (file)
        {
            case FXR fxr:
                fxr.Id = id;
                break;
            case FFXDLSE ffx:
                ffx.Effect.ID = id;
                if (ffx.Effect.Variants != null)
                    foreach (var v in ffx.Effect.Variants)
                        v.Effect.ID = id;
                break;
            case FXR1 { RootFXNode: FXR1.FXNode.EffectCreateNode133 r }:
                r.EffectID = id;
                break;
            case FXR1 { RootFXNode: FXR1.FXNode.EffectCreateNode134 m }:
                m.EffectID = id;
                foreach (var n in m.Nodes.OfType<FXR1.FXNode.EffectCreateNode133>())
                    n.EffectID = id;
                break;
        }
    }

    /// <summary>
    /// Points every effect call to <paramref name="from"/> at <paramref name="to"/> (DLSE Param37, FXR1 EffectCallNode);
    /// returns how many changed. FXR3 Proxy nodes are FxProxyOps' job.
    /// </summary>
    public static int RetargetCalls(object file, int from, int to)
    {
        int changed = 0;
        switch (file)
        {
            case FFXDLSE ffx:
                foreach (var call in EffectCalls(ffx))
                    if (call.EffectID == from)
                    {
                        call.EffectID = to;
                        changed++;
                    }
                break;
            case FXR1 fxr1:
                foreach (var call in EffectCalls(fxr1))
                    if (call.EffectID == from)
                    {
                        call.EffectID = to;
                        changed++;
                    }
                break;
        }
        return changed;
    }

    /// <summary>The effect ids a file calls, with repeats.</summary>
    public static IEnumerable<int> CalledIds(object file) => file switch
    {
        FFXDLSE ffx => EffectCalls(ffx).Select(c => c.EffectID),
        FXR1 fxr1 => EffectCalls(fxr1).Select(c => c.EffectID),
        _ => Enumerable.Empty<int>(),
    };

    /// <summary>Every DLSE effect call (Param37), through every param list of the effect, its variants, states and nested calls.</summary>
    public static IEnumerable<FFXDLSE.Param37> EffectCalls(FFXDLSE ffx)
    {
        var effects = ffx.Effect.Variants?.Count > 0 ? ffx.Effect.Variants.Select(v => v.Effect) : new[] { ffx.Effect };
        var lists = new Stack<FFXDLSE.ParamList>();
        foreach (var e in effects)
        {
            if (e.ParamList1 != null) lists.Push(e.ParamList1);
            if (e.ParamList2 != null) lists.Push(e.ParamList2);
            if (e.StateMap != null)
                foreach (var s in e.StateMap.States)
                    foreach (var a in s.Actions)
                        if (a.ParamList != null) lists.Push(a.ParamList);
        }
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var found = new List<FFXDLSE.Param37>();
        while (lists.Count > 0)
        {
            var list = lists.Pop();
            if (!seen.Add(list))
                continue;
            foreach (var p in list.Params)
            {
                switch (p)
                {
                    case FFXDLSE.Param37 c:
                        found.Add(c);
                        if (c.ParamList != null) lists.Push(c.ParamList);
                        break;
                    case FFXDLSE.Param38 c when c.ParamList != null:
                        lists.Push(c.ParamList);
                        break;
                }
            }
        }
        return found;
    }

    /// <summary>Every FXR1 effect call node, read fresh from the node graph.</summary>
    public static IEnumerable<FXR1.FXNode.EffectCallNode> EffectCalls(FXR1 fxr1) =>
        Fxr1Structure.Walk(fxr1.RootFXNode).OfType<FXR1.FXNode.EffectCallNode>();

    /// <summary>
    /// What kind of file this is, for refusing an import that can't play in the project: DLSE's stream format (DES
    /// 2.1, DS2 / DS2S 1.3), FXR1's version and pointer width (DS1 narrow, DS1R wide, BB v2). DS2 and DS2S can't be
    /// told apart from the bytes.
    /// </summary>
    public static string Signature(object file) => file switch
    {
        FXR fxr => $"FXR {fxr.Version}",
        FFXDLSE ffx => $"DLSE {ffx.Format}",
        FXR1 fxr1 => fxr1.Version == 2 ? "FXR v2" : fxr1.Wide ? "FXR v1 64-bit" : "FXR v1 32-bit",
        _ => "?",
    };

    /// <summary>
    /// The file name of an effect: f + the id padded to <paramref name="width"/> digits (7 for DES, DS1, DS1R, DS2;
    /// 9 for BB and the FXR3 games), longer ids at their own length (§22.6), plus the extension.
    /// </summary>
    public static string Stem(int id, int width) => "f" + id.ToString().PadLeft(width, '0');
}
