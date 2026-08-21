using Hexa.NET.ImGui;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public static class HavokEditDecorators
{
    public static bool ParamRefRow(MapEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj)
    {
        if (view.Project.Handler.ParamEditor == null)
            return false;

        if (havokMeta == null)
            return false;

        var activeView = view.Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (fieldMeta.ParamRef == "")
            return false;

        List<ParamRef> refs = new()
        {
            new ParamRef(null, fieldMeta.ParamRef)
        };

        ImGui.NextColumn();

        ParamReferenceHelper.Label(activeView, refs, null);

        ImGui.NextColumn();

        if (view.Project.Handler.ParamEditor != null)
        {
            ParamReferenceHelper.Hint(activeView, refs, null, val);
            ParamReferenceHelper.Click(activeView, val, null, refs);

            if (ImGui.BeginPopupContextItem($"{prop.Name}EnumContextMenu"))
            {
                var opened = ParamReferenceHelper.ContextMenu(activeView, refs, null, val, ref newObj, null);
                ImGui.EndPopup();
                return opened;
            }
        }

        if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        return false;
    }
}
