using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor.Actions
{
    public static class PropAction_Position
    {
        public static void CopyCurrentPosition(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedPosition = (Vector3)obj;
            //CFG.Current.SavedPosition = (Vector3)prop.GetValue(obj, null);
        }

        public static void PasteSavedPosition(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedPosition());
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(action);
        }
    }
}
