using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor.Actions
{
    public static class PropAction_Scale
    {
        public static void CopyCurrentScale(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedScale = (Vector3)prop.GetValue(obj, null);
        }

        public static void PasteSavedScale(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedScale());
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(action);
        }
    }
}
