using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using BodyCinfo = Havok.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Collision Editor's fall protection generation: the bodies of an f file's physics system are replaced with the
/// generated walls (with or without the walls it had). Undoing puts the old bodies back. Both mark the map's collision
/// as modified, and call back so the displayed mesh follows.
/// </summary>
public class FallProtectionAction : ViewportAction, IMapHistoryItem
{
    public string Describe() => LOC.Get("MAP_History_Fall_Protection", FileName);

    private readonly MapContainer Map;
    private readonly string FileName;
    private readonly List<BodyCinfo> Bodies;
    private readonly List<BodyCinfo> Before;
    private readonly List<BodyCinfo> After;
    private readonly Action<string> OnChanged;

    public FallProtectionAction(MapContainer map, string fileName, List<BodyCinfo> bodies, List<BodyCinfo> after,
        Action<string> onChanged)
    {
        Map = map;
        FileName = fileName;
        Bodies = bodies;
        Before = new List<BodyCinfo>(bodies);
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

    private void Set(List<BodyCinfo> bodies)
    {
        Bodies.Clear();
        Bodies.AddRange(bodies);

        Map?.MarkDirty(MapFileType.Collision);
        OnChanged?.Invoke(FileName);
    }
}
