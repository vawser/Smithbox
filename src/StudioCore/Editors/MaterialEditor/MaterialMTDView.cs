using Hexa.NET.ImGui;
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
/// The main window for a MTD entry.
/// </summary>
public class MaterialMTDView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialMTDView(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
    public void Draw()
    {
        if (Editor.Selection.SelectedMTD == null)
            return;

        var curMTD = Editor.Selection.SelectedMTD;

        ImGui.Text($"ShaderPath: {curMTD.ShaderPath}");
        ImGui.Text($"Description: {curMTD.Description}");

        DisplayParameterTable(curMTD);

        ImGui.Begin("Texture Types##materialTextureTypeTable", ImGuiWindowFlags.None);
        DisplayTextureList(curMTD);
        ImGui.End();
    }

    public void DisplayParameterTable(MTD curMTD)
    {
        if (ImGui.BeginTable($"ParameterTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

            for (int i = 0; i < curMTD.Params.Count; i++)
            {
                var param = curMTD.Params[i];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Name
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{param.Name}");

                ImGui.TableSetColumnIndex(1);

                // Type
                ImGui.Text($"{param.Type}");

                ImGui.TableSetColumnIndex(2);

                // Value
                ImGui.Text($"{param.Value}");
            }

            ImGui.EndTable();
        }
    }

    public void DisplayTextureList(MTD curMTD)
    {
        if (Editor.Selection.SelectedTextureIndex != -1)
        {
            if (ImGui.BeginTable($"TextureTable", 3, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

                var texture = curMTD.Textures[Editor.Selection.SelectedTextureIndex];

                // Type
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Type");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{texture.Type}");

                // Extended
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Extended");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{texture.Extended}");

                // UV Number
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"UV Number");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{texture.UVNumber}");

                // Shader Data Index
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Shader Data Index");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{texture.ShaderDataIndex}");

                // Path
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Path");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{texture.Path}");

                for (int i = 0; i < texture.UnkFloats.Count; i++)
                {
                    var curFloat = texture.UnkFloats[i];

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"Float [{i}]");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{curFloat}");
                }

                ImGui.EndTable();
            }
        }

        ImGui.BeginChild("TextureList");

        for (int i = 0; i < curMTD.Textures.Count; i++)
        {
            if(ImGui.Selectable($"Texture [{i}]", i == Editor.Selection.SelectedTextureIndex))
            {
                Editor.Selection.SelectedTextureIndex = i;
            }
        }

        ImGui.EndChild();
    }
}
