#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen model of the project's game, from /facegen/facegen.fgbnd: for now its controls (si.ctl), whose
/// labels name the shape and texture sliders. Read once per project, through the project's file system, so a mod's
/// fgbnd is used when it has one.
/// </summary>
public sealed class FaceGenModelData
{
    // The fgbnd's name and compression differ between the games
    private static readonly string[] BinderPaths =
    {
        "/facegen/facegen.fgbnd.dcx",
        "/facegen/facegen.fgbnd",
        "/facegen/FaceGen.fgbnd",
        "/facegen/FaceGen.fgbnd.dcx",
    };

    private static readonly ConditionalWeakTable<ProjectEntry, FaceGenModelData> Cache = new();

    /// <summary>The game's controls, or null if it has no fgbnd (Nightreign) or it can't be read</summary>
    public CTL? Controls { get; private set; }

    public static FaceGenModelData Get(ProjectEntry project) => Cache.GetValue(project, Load);

    private static FaceGenModelData Load(ProjectEntry project)
    {
        var data = new FaceGenModelData();

        foreach (var path in BinderPaths)
        {
            try
            {
                if (!project.VFS.FS.FileExists(path) || project.VFS.FS.ReadFile(path) is not { } raw)
                    continue;

                var bytes = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
                var isBnd4 = bytes.Length >= 4 && bytes.Span[..4].SequenceEqual("BND4"u8);
                IBinder binder = isBnd4 ? BND4.Read(bytes) : BND3.Read(bytes);

                // si.ctl, or si.ctlbe in Demon's Souls
                var ctl = binder.Files.FirstOrDefault(f =>
                    f.Name?.EndsWith(".ctl", StringComparison.OrdinalIgnoreCase) == true ||
                    f.Name?.EndsWith(".ctlbe", StringComparison.OrdinalIgnoreCase) == true);

                if (ctl != null)
                    data.Controls = CTL.Read(ctl.Bytes);

                break;
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(FaceGenModelData), LOC.Get("FACEGEN_Model_Read_Failed", path), e);
            }
        }

        return data;
    }

    /// <summary>The label of shape control <paramref name="index"/>, e.g. "Nose - down / up"</summary>
    public string? GeometryLabel(int index) =>
        Controls is { } ctl && index >= 0 && index < ctl.GeometrySymmetricControls.Count
            ? ctl.GeometrySymmetricControls[index].Label
            : null;

    /// <summary>The label of texture control <paramref name="index"/>, e.g. "Skin Shade - Dark / Light"</summary>
    public string? TextureLabel(int index) =>
        Controls is { } ctl && index >= 0 && index < ctl.TextureSymmetricControls.Count
            ? ctl.TextureSymmetricControls[index].Label
            : null;
}
