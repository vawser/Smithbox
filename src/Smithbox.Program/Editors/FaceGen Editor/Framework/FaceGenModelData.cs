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

                data.Binder = binder;
                break;
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(FaceGenModelData), LOC.Get("FACEGEN_Model_Read_Failed", path), e);
            }
        }

        return data;
    }

    // The fgbnd, for the files read on demand (the texture model is only wanted by games that draw FaceGen textures)
    private IBinder? Binder;
    private EGT? TextureModel;
    private bool TextureModelRead;
    private readonly System.Collections.Generic.Dictionary<string, (int Width, int Height, byte[] Rgb)?> Images = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The texture modes (face.egt), or null if the fgbnd has none. Read on first use</summary>
    public EGT? Texture
    {
        get
        {
            lock (Images)
            {
                if (!TextureModelRead)
                {
                    TextureModelRead = true;
                    try
                    {
                        if (Binder?.Files.FirstOrDefault(f => f.Name?.EndsWith(".egt", StringComparison.OrdinalIgnoreCase) == true) is { } egt)
                            TextureModel = EGT.Read(egt.Bytes);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(typeof(FaceGenModelData), LOC.Get("FACEGEN_Model_Read_Failed", "face.egt"), e);
                    }
                }

                return TextureModel;
            }
        }
    }

    /// <summary>
    /// A 24-bit BMP of the fgbnd by name (face.bmp, face_M.bmp), as top-down RGB, or null. Read on first use
    /// </summary>
    public (int Width, int Height, byte[] Rgb)? Image(string name)
    {
        lock (Images)
        {
            if (Images.TryGetValue(name, out var cached))
                return cached;

            (int, int, byte[])? image = null;

            try
            {
                if (Binder?.Files.FirstOrDefault(f => f.Name?.EndsWith(name, StringComparison.OrdinalIgnoreCase) == true) is { } file)
                    image = ReadBmp(file.Bytes.ToArray());
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(FaceGenModelData), LOC.Get("FACEGEN_Model_Read_Failed", name), e);
            }

            Images[name] = image;
            return image;
        }
    }

    /// <summary>A 24-bit BMP as top-down RGB, or null if it is another depth</summary>
    private static (int, int, byte[])? ReadBmp(byte[] d)
    {
        if (d.Length < 54 || d[0] != 'B' || d[1] != 'M' || BitConverter.ToInt16(d, 28) != 24)
            return null;

        var offset = BitConverter.ToInt32(d, 10);
        var width = BitConverter.ToInt32(d, 18);
        var height = BitConverter.ToInt32(d, 22);
        var bottomUp = height > 0;
        height = Math.Abs(height);
        var stride = (width * 3 + 3) & ~3;
        var rgb = new byte[width * height * 3];

        for (var y = 0; y < height; y++)
        {
            var src = offset + (bottomUp ? height - 1 - y : y) * stride;

            for (var x = 0; x < width; x++)
            {
                rgb[(y * width + x) * 3] = d[src + x * 3 + 2];
                rgb[(y * width + x) * 3 + 1] = d[src + x * 3 + 1];
                rgb[(y * width + x) * 3 + 2] = d[src + x * 3];
            }
        }

        return (width, height, rgb);
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
