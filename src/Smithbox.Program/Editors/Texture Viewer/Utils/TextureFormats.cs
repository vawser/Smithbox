using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>A DXGI format the import can write.</summary>
public record TextureFormatChoice(int Dxgi, string Label);

/// <summary>What a TPF texture's payload is: the DDS header as the game stores it.</summary>
public record TextureInfo(int Width, int Height, int MipLevels, int Dxgi, bool Dx10Header, TPF.TexType Type, bool Readable, string FourCC = null)
{
    public string FormatName => Enum.IsDefined(typeof(DDS.DXGI_FORMAT), (uint)Dxgi) ? $"{(DDS.DXGI_FORMAT)Dxgi}" : $"{Dxgi}";
}

/// <summary>
/// The texture formats of the games, from the TextureSurvey census (Texture Editor plan §7.1): the DXGI formats the
/// import offers, and which TPF format byte goes with each in each game.
/// </summary>
public static class TextureFormats
{
    public static readonly List<TextureFormatChoice> Choices =
    [
        new(71, "BC1 (DXT1)"),
        new(72, "BC1 sRGB"),
        new(74, "BC2 (DXT3)"),
        new(77, "BC3 (DXT5)"),
        new(78, "BC3 sRGB"),
        new(80, "BC4 (ATI1)"),
        new(83, "BC5 (ATI2)"),
        new(95, "BC6H UF16"),
        new(98, "BC7"),
        new(99, "BC7 sRGB"),
        new(28, "R8G8B8A8"),
        new(29, "R8G8B8A8 sRGB"),
        new(87, "B8G8R8A8"),
        new(10, "R16G16B16A16 Float"),
    ];

    public static string GetLabel(int dxgi)
    {
        return Choices.FirstOrDefault(e => e.Dxgi == dxgi)?.Label
            ?? (Enum.IsDefined(typeof(DDS.DXGI_FORMAT), (uint)dxgi) ? $"{(DDS.DXGI_FORMAT)dxgi}" : $"DXGI {dxgi}");
    }

    /// <summary>
    /// The DDS header of a texture. Console textures are headerized first; a texture that can't be read reports
    /// Readable = false.
    /// </summary>
    public static TextureInfo GetInfo(TPF tpf, TPF.Texture tex)
    {
        try
        {
            var bytes = tpf.Platform == TPF.TPFPlatform.PC ? tex.Bytes : tex.Headerize();
            var dds = new DDS(bytes);
            return new TextureInfo(dds.dwWidth, dds.dwHeight, Math.Max(1, dds.dwMipMapCount), (int)dds.GetDXGIFormat(),
                dds.ddspf.dwFourCC == "DX10", tex.Type, true, dds.ddspf.dwFourCC);
        }
        catch
        {
            return new TextureInfo(0, 0, 0, 0, false, tex.Type, false);
        }
    }

    /// <summary>
    /// The format byte for a new or re-formatted texture: a texture in the same TPF with the same DXGI format first
    /// (the game's own choice), else the usual byte for the game.
    /// </summary>
    public static byte SuggestFormatByte(ProjectType game, TPF tpf, int dxgi, TPF.Texture exclude = null)
    {
        if (tpf != null)
        {
            foreach (var t in tpf.Textures)
            {
                if (t == exclude)
                    continue;

                var info = GetInfo(tpf, t);
                if (info.Readable && info.Dxgi == dxgi)
                    return t.Format;
            }
        }

        bool old = game is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;

        return dxgi switch
        {
            71 or 72 => 0,
            74 or 75 => 3,
            77 or 78 => 5,
            80 => (byte)(old ? 24 : 103),
            83 => (byte)(game == ProjectType.DS1R ? 36 : 104),
            95 or 96 => (byte)(game == ProjectType.DS1R ? 37 : 100),
            98 or 99 => (byte)(game == ProjectType.DS1R ? 38 : 102),
            28 or 29 => (byte)(old ? 10 : 105),
            87 or 91 => 9,
            10 => 22,
            _ => 102,
        };
    }

    /// <summary>
    /// Whether the Texture Editor can write this TPF's textures. PC only for now: console TPFs need a re-tiling
    /// writer per platform (Texture Editor plan, T8).
    /// </summary>
    public static bool IsEditable(TPF tpf) => tpf != null && tpf.Platform == TPF.TPFPlatform.PC;
}
