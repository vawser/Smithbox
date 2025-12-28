using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public static class PropAction_Scale
{
    public static void CopyCurrentScale(PropertyInfo prop, object obj)
    {
        CFG.Current.SavedScale = (Vector3)prop.GetValue(obj, null);
    }

    public static void PasteSavedScale(MapEditorScreen editor, ViewportSelection _selection)
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
        {
            actlist.Add(sel.ApplySavedScale());
        }

        var action = new ViewportCompoundAction(actlist);
        editor.EditorActionManager.ExecuteAction(action);
    }
}
