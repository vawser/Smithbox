using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;

namespace StudioCore.Editors.MapEditor.Helpers;

public static class VisualizationHelper
{
    public static void ToggleRenderType(ViewportSelection selection)
    {
        selection.StoreSelection();
        var sel = selection.GetSelection();

        foreach (var entry in sel)
        {
            var ent = (Entity)entry;

            if (ent is MsbEntity)
            {
                var mEnt = (MsbEntity)ent;

                if (!mEnt.IsSwitchingRenderType && 
                    ( mEnt.Type is MsbEntityType.Region || mEnt.Type is MsbEntityType.Light))
                {
                    mEnt.SwitchRenderType();
                }
            }
        }

        selection.ResetSelection();
    }
}
