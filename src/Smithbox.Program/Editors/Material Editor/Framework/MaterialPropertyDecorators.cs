using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

public static class MaterialPropertyDecorators
{
    public static bool ParamRefRow(IEditorView view, MaterialClass classMeta, PropertyInfo prop, object val, ref object newObj)
    {
        // The Param Editor of the material's project, whatever kind of view asks
        ProjectEntry project = view switch
        {
            MaterialEditorView materialView => materialView.Project,
            MapEditorView mapEditorView => mapEditorView.Project,
            _ => null
        };

        var activeView = project?.Handler.ParamEditor?.ViewHandler.ActiveView;

        if (activeView == null)
            return false;

        if (classMeta == null)
            return false;

        var fieldMeta = classMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
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

        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        return false;
    }
}
