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
    public static class PropAction_Rotation
    {
        public static void CopyCurrentRotation(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedRotation = (Vector3)obj;
            //CFG.Current.SavedRotation = (Vector3)prop.GetValue(obj, null);
        }

        public static void PasteSavedRotation(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedRotation());
            }

            CompoundAction action = new(actlist);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(action);
        }
    }
}
