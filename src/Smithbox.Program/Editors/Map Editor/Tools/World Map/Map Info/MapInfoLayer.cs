namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The four index layers of an ER mapinfotexbnd. Each pixel of a layer is the ID of a row in its param,
/// the _m61 variant for the m61 tiles; 255 is the "none" row.
/// See Claude/Documentation/MapInfoTex Editor.md.
/// </summary>
public enum MapInfoLayer
{
    MapPiece,
    MapName,
    Weather,
    Mimicry
}

public static class MapInfoLayers
{
    public static readonly MapInfoLayer[] All = { MapInfoLayer.MapPiece, MapInfoLayer.MapName, MapInfoLayer.Weather, MapInfoLayer.Mimicry };

    /// <summary>The value no row is painted with</summary>
    public const byte None = 255;

    /// <summary>The BMP that holds the layer: 0 = mAA_XX_YY_02.bmp, 1 = mAA_XX_YY_02_01.bmp</summary>
    public static int Slot(MapInfoLayer layer) => layer == MapInfoLayer.Mimicry ? 1 : 0;

    /// <summary>The byte of a BMP pixel (stored B, G, R) that holds the layer</summary>
    public static int ByteOffset(MapInfoLayer layer) => layer switch
    {
        MapInfoLayer.MapPiece => 2, // R
        MapInfoLayer.MapName => 1, // G
        MapInfoLayer.Weather => 0, // B
        MapInfoLayer.Mimicry => 2, // R of the second BMP
        _ => 0
    };

    public static string ParamName(MapInfoLayer layer, bool dlc)
    {
        var name = layer switch
        {
            MapInfoLayer.MapPiece => "MapPieceTexParam",
            MapInfoLayer.MapName => "MapNameTexParam",
            MapInfoLayer.Weather => "WeatherLotTexParam",
            MapInfoLayer.Mimicry => "MimicryEstablishmentTexParam",
            _ => ""
        };

        return dlc ? $"{name}_m61" : name;
    }

    public static string NameKey(MapInfoLayer layer) => $"MAP_MapInfo_Layer_{layer}";
}
