using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An edit of a collision's EDGE file by the EDGE Editor: the whole file before and after, so any change to it (and
/// adding or removing the file, as null) undoes the same way.
/// </summary>
public class EdgeEditAction : ViewportAction, IMapHistoryItem
{
    private readonly EdgeBank Bank;
    private readonly MapContainer Map;
    private readonly string MapId;
    private readonly string ModelName;
    private readonly EDGE Before;
    private readonly EDGE After;
    private readonly string Description;

    public string Describe() => Description;

    /// <param name="description">What the edit did, for the History tool.</param>
    public EdgeEditAction(EdgeBank bank, MapContainer map, string modelName, EDGE before, EDGE after, string description)
    {
        Bank = bank;
        Map = map;
        MapId = map.Name;
        ModelName = modelName;
        Before = before == null ? null : EdgeBank.Copy(before);
        After = after == null ? null : EdgeBank.Copy(after);
        Description = description;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(Before);
        return ActionEvent.NoEvent;
    }

    private void Apply(EDGE edge)
    {
        Bank.SetEdge(MapId, ModelName, edge);
        Map.MarkDirty(MapFileType.Edge);
        Bank.RebuildModel(MapId, ModelName);
    }
}
