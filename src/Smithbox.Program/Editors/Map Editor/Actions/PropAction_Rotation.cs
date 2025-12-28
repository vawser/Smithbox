using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public static class PropAction_Rotation
{
    public static void CopyCurrentRotation(PropertyInfo prop, object obj)
    {
        CFG.Current.SavedRotation = (Vector3)obj;
        //CFG.Current.SavedRotation = (Vector3)prop.GetValue(obj, null);
    }

    public static void PasteSavedRotation(MapEditorScreen editor, ViewportSelection _selection)
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
        {
            actlist.Add(sel.ApplySavedRotation());
        }

        var action = new ViewportCompoundAction(actlist);
        editor.EditorActionManager.ExecuteAction(action);
    }
}
