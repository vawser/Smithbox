using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

/// <summary>
/// The main window for a MATBIN entry.
/// </summary>
public class MaterialMATBINView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialMATBINView(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        if(Editor.Selection.SelectedMATBIN != null)
        {
            DisplayHeader(Editor.Selection.SelectedMATBIN);
            DisplayParameters(Editor.Selection.SelectedMATBIN);
            DisplaySamplers(Editor.Selection.SelectedMATBIN);
        }
    }

    public void DisplayHeader(MATBIN curEntry)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.CollapsingHeader("Configuration", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginTable($"matbinHeader_table", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Primary Value", ImGuiTableColumnFlags.WidthFixed);

                // Shader Path
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Shader Path");
                UIHelper.Tooltip("Network path to the shader source file.");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                Editor.FieldInput.DisplayFieldInput($"ShaderPath", curEntry.ShaderPath, curEntry.ShaderPath);

                // Source Path
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Source Path");
                UIHelper.Tooltip("Network path to the material source file, either a matxml or an mtd.");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                Editor.FieldInput.DisplayFieldInput($"SourcePath", curEntry.SourcePath, curEntry.SourcePath);

                // Key
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Key");
                UIHelper.Tooltip("Unknown, presumed to be an identifier for documentation.");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                Editor.FieldInput.DisplayFieldInput($"Key", curEntry.Key, curEntry.Key);

                ImGui.EndTable();
            }
        }
    }

    public void DisplayParameters(MATBIN curEntry)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.CollapsingHeader("Parameters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginTable($"matbinParam_table", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Primary Value", ImGuiTableColumnFlags.WidthFixed);

                for (int i = 0; i < curEntry.Params.Count; i++)
                {
                    var curParam = curEntry.Params[i];

                    DisplayParameterEntry(i, curParam);
                }

                ImGui.EndTable();
            }
        }
    }

    public void DisplayParameterEntry(int index, MATBIN.Param curParam)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{curParam.Name}");
        UIHelper.Tooltip("");

        ImGui.TableSetColumnIndex(1);

        ImGui.AlignTextToFramePadding();
        Editor.FieldInput.DisplayParamInput($"{curParam.Name}{index}", curParam.Value, curParam.Type);
    }

    public void DisplaySamplers(MATBIN curEntry)
    {
        for (int i = 0; i < curEntry.Samplers.Count; i++)
        {
            if (ImGui.CollapsingHeader($"Sampler {i}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var curSampler = curEntry.Samplers[i];

                DisplaySamplerEntry(i, curSampler);
            }
        }
    }

    public void DisplaySamplerEntry(int index, MATBIN.Sampler curSampler)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"matbinSampler_table{index}", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Primary Value", ImGuiTableColumnFlags.WidthFixed);

            // Type
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Type");
            UIHelper.Tooltip("The type of the sampler.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            Editor.FieldInput.DisplayFieldInput($"Type", curSampler.Type, curSampler.Type);

            // Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Path");
            UIHelper.Tooltip("An optional network path to the texture, if not specified in the FLVER.");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            Editor.FieldInput.DisplayFieldInput($"Path", curSampler.Path, curSampler.Path);

            // Unk14
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Unk14");
            UIHelper.Tooltip("");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            Editor.FieldInput.DisplayFieldInput($"Unk14", curSampler.Unk14, curSampler.Unk14);

            ImGui.EndTable();
        }
    }
}
