using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Collision Editor's fall protection generation: the bodies of an f file's physics system are replaced with the
/// generated walls. Undoing puts the old bodies back. Both mark the map's collision as modified, and call back so the
/// displayed mesh follows. The bodies are of whichever Havok version the game's files are (hk2018 for ER, NR and
/// DB_NT, hk2019 for AC6), so the system's body list is taken as it is.
/// </summary>
public class FallProtectionAction : ViewportAction, IMapHistoryItem
{
    public string Describe() => LOC.Get("MAP_History_Fall_Protection", FileName);

    private readonly MapContainer Map;
    private readonly string FileName;
    private readonly IList Bodies;
    private readonly List<object> Before;
    private readonly List<object> After;
    private readonly Action<string> OnChanged;

    public FallProtectionAction(MapContainer map, string fileName, IList bodies, List<object> after,
        Action<string> onChanged)
    {
        Map = map;
        FileName = fileName;
        Bodies = bodies;
        Before = bodies.Cast<object>().ToList();
        After = after;
        OnChanged = onChanged;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Set(After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Set(Before);
        return ActionEvent.NoEvent;
    }

    private void Set(List<object> bodies)
    {
        Bodies.Clear();

        foreach (var body in bodies)
            Bodies.Add(body);

        Map?.MarkDirty(MapFileType.Collision);
        OnChanged?.Invoke(FileName);
    }
}
