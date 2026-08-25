using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class ModelPropertyDecorators
{
    // Param Reference
    public static bool ParamRefRow(IEditorView view, ModelClass classMeta, PropertyInfo prop, object val, ref object newObj)
    {
        ParamEditorView activeView = null;

        if (view is MapEditorView mapEditorView)
        {
            if (mapEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = mapEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

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

        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
        {
            ImGui.NextColumn();
        }

        return false;
    }

    // Dummy Reference
    public static void DummyRefRow(IEditorView view, ModelClass classMeta, PropertyInfo prop, object val, ref object newObj)
    {
        if (view is ModelEditorView modelEditorView)
        {
            if (classMeta == null)
                return;

            var fieldMeta = classMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
            if (fieldMeta == null)
            {
                return;
            }

            if (!fieldMeta.DummyRef)
                return;

            var container = modelEditorView.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(val.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Dummies.Count; i++)
            {
                var curDummy = container.Dummies[i];

                if (i == value)
                {
                    var dummy = (FLVER.Dummy)curDummy.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##dummySelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/dummy/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text(LOC.Get("MODEL_Properties_Dummy_Ref_Hint", i, dummy.ReferenceID));
                }
            }

            if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
    }

    // Node Reference
    public static void NodeRefRow(IEditorView view, ModelClass classMeta, PropertyInfo prop, object val, ref object newObj)
    {
        if (view is ModelEditorView modelEditorView)
        {
            if (classMeta == null)
                return;

            var fieldMeta = classMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
            if (fieldMeta == null)
            {
                return;
            }

            if (!fieldMeta.NodeRef)
                return;

            var container = modelEditorView.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(val.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Nodes.Count; i++)
            {
                var curNode = container.Nodes[i];

                if (i == value)
                {
                    var node = (FLVER.Node)curNode.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##nodeSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/node/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{node.Name}");
                }
            }

            if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
    }

    // Material Reference
    public static void MaterialRefRow(IEditorView view, ModelClass classMeta, PropertyInfo prop, object val, ref object newObj)
    {
        if (view is ModelEditorView modelEditorView)
        {
            if (classMeta == null)
                return;

            var fieldMeta = classMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
            if (fieldMeta == null)
            {
                return;
            }

            if (!fieldMeta.MaterialRef)
                return;

            var container = modelEditorView.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(val.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Materials.Count; i++)
            {
                var curMaterial = container.Materials[i];

                if (i == value)
                {
                    var material = (FLVER2.Material)curMaterial.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##matSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/material/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{material.Name}");
                }
            }

            if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
    }
}
