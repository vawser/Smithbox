using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.PropertyEditor
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

            CompoundAction action = new(actlist);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(action);
        }
    }
}
