using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class HavokPropertyDecorators
{
    public static bool ParamRefRow(IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj)
    {
        ParamEditorView activeView = null;

        if(view is MapEditorView mapEditorView)
        {
            if (mapEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = mapEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

        if (activeView == null)
            return false;

        if (havokMeta == null)
            return false;

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

        if (activeView.Project.Handler.ParamEditor != null)
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
