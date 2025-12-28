using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.ModelEditor;

public class ModelMaskToggler
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    private bool SelectEntry = false;
    private int SelectedID = -1;

    public ModelMaskToggler(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnToolWindow()
    {
        // TODO: re-enable once we've sorted the sub-mesh thing
        //var windowWidth = ImGui.GetWindowWidth();

        //if (ImGui.CollapsingHeader("Model Mask Toggler"))
        //{
        //    UIHelper.WrappedText("Quickly toggle between model mask combinations by selecting a NPC Param entry.");
        //    UIHelper.WrappedText("");

        //    ImGui.Separator();

        //    Display();
        //}
    }

    public void Display()
    {
        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            UIHelper.WrappedText("This project type is not supported by this tool.");
            return;
        }

        if (Editor.Project.ParamEditor == null)
        {
            UIHelper.WrappedText("The Param Editor must be enabled for this tool to work.");

            return;
        }

        if(Editor.Selection.SelectedModelWrapper == null)
        {
            UIHelper.WrappedText("A model must be loaded first for this tool to work.");

            return;
        }

        var filename = Editor.Selection.SelectedModelWrapper.Name;
        var npcParamKey = "NpcParam";

        if (!Editor.Project.ParamData.PrimaryBank.Params.ContainsKey(npcParamKey))
        {
            UIHelper.WrappedText("Failed to find associated NpcParam entry.");

            return;
        }

        var npcParam = Editor.Project.ParamData.PrimaryBank.Params[npcParamKey];

        if (npcParam == null)
        {
            UIHelper.WrappedText("Failed to find associated NpcParam entry.");

            return;
        }

        foreach (var entry in npcParam.Rows)
        {
            if (IsAssociatedParam($"{entry.ID}", filename))
            {
                if (ImGui.Selectable($"[{entry.ID}]##row{entry.ID}", entry.ID == SelectedID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ToggleMeshes(entry);
                    SelectedID = entry.ID;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEntry)
                {
                    SelectEntry = false;
                    SelectedID = entry.ID;
                    ToggleMeshes(entry);
                }

                if (ImGui.IsItemFocused() &&
                    (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                {
                    SelectEntry = true;
                }

                UIHelper.DisplayAlias($"{entry.Name}");
            }
        }
    }

    public bool IsAssociatedParam(string rowID, string filename)
    {
        if (filename.Length >= 4 && rowID.Length >= 4)
        {
            var model = filename.Substring(1, 4); // Remove the 'c'
            var row = rowID.Substring(0, 4);

            if (row == model)
            {
                return true;
            }
        }

        return false;
    }

    public void ToggleMeshes(Param.Row row)
    {
        List<bool> maskList = new List<bool>();

        foreach (var cell in row.Cells)
        {
            var internalName = GetInternalName();

            // Works on the assumption that we iterate top to bottom
            // So the natural add order corresponds to the mask I
            // e.g. first entry is mask 0, at index 0, etc
            if (cell.Def.InternalName.Contains(internalName))
            {
                if ($"{cell.Value}" == "0")
                {
                    maskList.Add(false);
                }
                else
                {
                    maskList.Add(true);
                }
            }
        }

        var container = Editor.Selection.SelectedModelWrapper.Container;
        var flver = Editor.Selection.SelectedModelWrapper.FLVER;

        Dictionary<int, FLVER2.Material> materialDict = new();

        for (int i = 0; i < flver.Materials.Count; i++)
        {
            var material = flver.Materials[i];

            materialDict.Add(i, material);
        }

        foreach (var entry in container.Meshes)
        {
            entry.EditorVisible = false;

            FLVER2.Mesh mesh = (FLVER2.Mesh)entry.WrappedObject;

            if (materialDict.ContainsKey(mesh.MaterialIndex))
            {
                var material = materialDict[mesh.MaterialIndex];

                var regex = @"\#[0-9]*\#";
                var maskIdStr = Regex.Match(material.Name, regex).Value;
                maskIdStr = maskIdStr.Replace("#", ""); // Remove the #s

                // If is a mask entry, default to false.
                if (maskIdStr != "")
                {
                    try
                    {
                        int maskId = int.Parse(maskIdStr);
                        entry.EditorVisible = maskList[maskId];
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to parse Mask ID: {e.Message}", LogLevel.Warning);
                    }
                }
                else
                {
                    entry.EditorVisible = true;
                }
            }
        }
    }

    private string GetInternalName()
    {
        return "modelDispMask";
    }
}
