using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections;
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

    // MTD / MATBIN Preview
    public static void MaterialPreview(ModelEditorView view, string materialPath)
    {
        var bank = view.Project.Handler.MaterialData?.PrimaryBank;

        if (bank == null || string.IsNullOrEmpty(materialPath))
            return;

        ImGui.Separator();

        if (!ImGui.CollapsingHeader($"{LOC.Get("MODEL_Properties_Material_Preview_Header")}##materialPreview", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var reference = bank.FindReference(materialPath);

        if (reference == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Properties_Material_Preview_Not_Found", materialPath));
            return;
        }

        // The Material Editor is optional, so only offer the jump if it exists
        if (view.Project.Handler.MaterialEditor != null)
        {
            if (ImGui.Button($"{Icons.Binoculars}##materialJump"))
            {
                EditorCommandQueue.AddCommand(new[] { "material", "select", materialPath });
            }
            GUI.Tooltip(LOC.Get("MODEL_Properties_Material_Preview_Jump_TT"));

            ImGui.SameLine();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{Path.GetFileName(reference.FileKey)}");

        if (reference.File is MTDFileWrapper mtdFile)
        {
            var mtd = mtdFile.MTD;

            GUI.WrappedText(LOC.Get("MODEL_Properties_Material_Preview_Shader", mtd.ShaderPath));
            GUI.WrappedText(LOC.Get("MODEL_Properties_Material_Preview_Description", mtd.Description));

            MaterialPreviewParams(mtd.Params.Select(e => (e.Name, e.Value)));
            MaterialPreviewTextures(view, mtd.Textures.Select(e => (e.Type, e.Path)));
        }

        if (reference.File is MATBINFileWrapper matbinFile)
        {
            var matbin = matbinFile.MATBIN;

            GUI.WrappedText(LOC.Get("MODEL_Properties_Material_Preview_Shader", matbin.ShaderPath));
            GUI.WrappedText(LOC.Get("MODEL_Properties_Material_Preview_Source", matbin.SourcePath));

            MaterialPreviewTextures(view, matbin.Samplers.Select(e => (e.Type, e.Path)));
            MaterialPreviewParams(matbin.Params.Select(e => (e.Name, e.Value)));
        }
    }

    private static void MaterialPreviewParams(IEnumerable<(string Name, object Value)> entries)
    {
        ImGui.Text(LOC.Get("MODEL_Properties_Material_Preview_Params"));

        if (ImGui.BeginTable("##materialPreviewParams", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("MODEL_Properties_Material_Preview_Col_Name"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Properties_Material_Preview_Col_Value"));
            ImGui.TableHeadersRow();

            foreach (var entry in entries)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{entry.Name}");

                ImGui.TableNextColumn();
                ImGui.Text(GetMaterialParamText(entry.Value));
            }

            ImGui.EndTable();
        }
    }

    private static void MaterialPreviewTextures(ModelEditorView view, IEnumerable<(string Type, string Path)> entries)
    {
        ImGui.Text(LOC.Get("MODEL_Properties_Material_Preview_Textures"));

        if (ImGui.BeginTable("##materialPreviewTextures", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("MODEL_Properties_Material_Preview_Col_Type"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Properties_Material_Preview_Col_Path"));
            ImGui.TableSetupColumn("##export", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var index = 0;

            foreach (var entry in entries)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{entry.Type}");

                ImGui.TableNextColumn();
                ImGui.Text($"{entry.Path}");

                ImGui.TableNextColumn();

                if (!string.IsNullOrEmpty(entry.Path))
                {
                    if (ImGui.Button($"{Icons.Download}##textureExport{index}"))
                    {
                        ExportMaterialTexture(view, entry.Path);
                    }
                    GUI.Tooltip(LOC.Get("MODEL_Properties_Material_Preview_Export_Texture_TT"));
                }

                index++;
            }

            ImGui.EndTable();
        }
    }

    // Exports to the project's output folder, using the export type set in the Texture Viewer
    private static void ExportMaterialTexture(ModelEditorView view, string texturePath)
    {
        // The model's textures are held in the pool by name, once they have been uploaded
        ResourceManager.DefaultPool.TryFind<TextureResource>(texturePath, out var handle);

        var texture = handle?.Get()?.GPUTexture?.TpfTexture;

        if (texture == null)
        {
            Smithbox.LogError(typeof(ModelPropertyDecorators), LOC.Get("MODEL_Properties_Material_Preview_Log_Texture_Not_Loaded", texturePath));
            return;
        }

        var exportType = CFG.Current.TextureViewerToolbar_ExportTextureType;

        var exportDirectory = Path.Join(view.Project.Descriptor.ProjectPath, "output");
        var exportFilePath = Path.Join(exportDirectory, texture.Name);
        var fullFilePath = $"{exportFilePath}{TexUtils.GetExportExtension(exportType)}";

        if (File.Exists(fullFilePath))
        {
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("TEXVIEW_TexExport_Dialog_Overwrite_File", fullFilePath),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;
        }

        try
        {
            Directory.CreateDirectory(exportDirectory);

            // Console textures are stored without a DDS header
            TexUtils.ExportImage(exportFilePath, texture.Headerize(), exportType);

            Smithbox.Log(typeof(ModelPropertyDecorators), LOC.Get("MODEL_Properties_Material_Preview_Log_Texture_Exported", fullFilePath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ModelPropertyDecorators), LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), e);
        }
    }

    // Param values are boxed scalars or arrays of them
    private static string GetMaterialParamText(object value)
    {
        if (value is IEnumerable values && value is not string)
        {
            return string.Join(", ", values.Cast<object>());
        }

        return $"{value}";
    }
}
