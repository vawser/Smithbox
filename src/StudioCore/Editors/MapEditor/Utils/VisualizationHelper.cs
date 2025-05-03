using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Framework.MsbEntity;

namespace StudioCore.Editors.MapEditor.Helpers;

public static class VisualizationHelper
{
    public static void ToggleRenderType(MapEditorScreen editor, ViewportSelection selection)
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

        selection.ResetSelection(editor);
    }
}
