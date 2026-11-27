using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Shows a small-base map (m30–m49) where a map pattern puts it: its root moved to the world transform of a
/// SmallBaseAttach region in an m60 tile, so the base is seen in place in the field. Display only: the root transform
/// isn't part of the MSB (a map root has no file), so nothing is saved and Reset puts the map back at its origin.
/// The region's transform is the attach row's posX/Y/Z and rotY (MSB Research.md §5.6); MapDefaultInfoParam's
/// mapPosOffsetY for the base (row ID = the map ID's digits, e.g. 32100000) is added to the height, as the game
/// adds it to small-base blocks. Analysis doc M4.
/// </summary>
public class NrSmallBasePlacement
{
    public sealed record Placement(string AttachMapID, string RegionName, float OffsetY);

    private readonly MapEditorView View;
    private readonly Dictionary<string, Placement> Placed = new(StringComparer.OrdinalIgnoreCase);

    public NrSmallBasePlacement(MapEditorView view)
    {
        View = view;
    }

    /// <summary>The bases currently shown at an attach point, by base map ID</summary>
    public IReadOnlyDictionary<string, Placement> Placements => Placed;

    /// <summary>
    /// Moves the loaded base map to the loaded attach map's region. Returns an error text, or null when placed.
    /// </summary>
    public string Place(string baseMapId, string attachMapId, string regionName)
    {
        var baseMap = View.Selection.GetMapContainerFromMapID(baseMapId);
        var attachMap = View.Selection.GetMapContainerFromMapID(attachMapId);

        if (baseMap == null)
            return LOC.Get("MAP_SmallBase_Not_Loaded", baseMapId);

        if (attachMap == null)
            return LOC.Get("MAP_SmallBase_Not_Loaded", attachMapId);

        var region = attachMap.GetObjectsByName(regionName).FirstOrDefault(e => e is MsbEntity { Type: MsbEntityType.Region });

        if (region == null)
            return LOC.Get("MAP_SmallBase_No_Region", regionName, attachMapId);

        // The region's world transform: its own position and rotation, through the attach map's root
        if (!Matrix4x4.Decompose(region.GetWorldMatrix(), out _, out var rotation, out var position))
            return LOC.Get("MAP_SmallBase_Bad_Transform", regionName);

        var offsetY = GetOffsetY(View.Project, baseMapId);
        position.Y += offsetY;

        SetRoot(baseMap, new Transform(position, rotation, Vector3.One));
        Placed[baseMapId] = new Placement(attachMapId, regionName, offsetY);

        return null;
    }

    /// <summary>Puts a base back at its own origin</summary>
    public void Reset(string baseMapId)
    {
        Placed.Remove(baseMapId);

        var baseMap = View.Selection.GetMapContainerFromMapID(baseMapId);

        if (baseMap != null)
            SetRoot(baseMap, Transform.Default);
    }

    public void OnMapUnloaded(MapContainer map)
    {
        if (map?.Name == null)
            return;

        // An unloaded base takes its placement with it; an unloaded attach map leaves the base where it is
        Placed.Remove(map.Name);
    }

    private void SetRoot(MapContainer map, Transform transform)
    {
        // As the tile offsets are applied on load: not an undo step, and the root has no file to mark as modified
        map.RootObject.GetUpdateTransformAction(transform).Execute();
        map.RootObject.UpdateRenderModel();
        View.DelayPicking();
    }

    /// <summary>MapDefaultInfoParam mapPosOffsetY for a map (row ID AABBCCDD), or 0</summary>
    public static float GetOffsetY(ProjectEntry project, string mapId)
    {
        if (mapId == null || mapId.Length != 12)
            return 0;

        var digits = mapId.Replace("m", "").Replace("_", "");

        if (!int.TryParse(digits, out var rowId))
            return 0;

        var row = MapPatternLinks.FindRow(project, "MapDefaultInfoParam", rowId);

        return row != null && CharacterComposition.HasField(row, "mapPosOffsetY")
            ? CharacterComposition.GetFloat(row, "mapPosOffsetY")
            : 0;
    }
}
