using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Veldrid;

namespace StudioCore.Editors.ModelEditor.Utils;

public static class ModelMaskToggler
{
    public static bool IsSupportedProjectType(ProjectEntry project)
    {
        if (project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            return false;
        }

        return true;
    }

    private static bool SelectEntry = false;
    private static int SelectedID = -1;

    // NPC Param List
    public static void Display(ModelEditorScreen editor)
    {
        var filename = editor.Selection._selectedFileName;
        var npcParamKey = "NpcParam";

        if (editor.Project.ParamEditor == null)
            return;

        if (!editor.Project.ParamData.PrimaryBank.Params.ContainsKey(npcParamKey))
            return;

        var npcParam = editor.Project.ParamData.PrimaryBank.Params[npcParamKey];

        if(npcParam == null)
            return;

        foreach (var entry in npcParam.Rows)
        {
            if(IsAssociatedParam($"{entry.ID}", filename))
            {
                if(ImGui.Selectable($"[{entry.ID}]##row{entry.ID}", entry.ID == SelectedID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ToggleMeshes(editor, entry);
                    SelectedID = entry.ID;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEntry)
                {
                    SelectEntry = false;
                    SelectedID = entry.ID;
                    ToggleMeshes(editor, entry);
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

    public static bool IsAssociatedParam(string rowID, string filename)
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

    public static void ToggleMeshes(ModelEditorScreen editor, Param.Row row)
    {
        List<bool> maskList = new List<bool>();

        foreach(var cell in row.Cells)
        {
            var internalName = GetInternalName();

            // Works on the assumption that we iterate top to bottom
            // So the natural add order corresponds to the mask I
            // e.g. first entry is mask 0, at index 0, etc
            if(cell.Def.InternalName.Contains(internalName))
            {
                if($"{cell.Value}" == "0")
                {
                    maskList.Add(false);
                }
                else
                {
                    maskList.Add(true);
                }
            }
        }

        var flver = editor.ResManager.GetCurrentFLVER();

        Dictionary<int, FLVER2.Material> materialDict = new();

        for(int i = 0; i < flver.Materials.Count; i++)
        {
            var material = flver.Materials[i];

            materialDict.Add(i, material);
        }

        var container = editor._universe.LoadedModelContainer;

        foreach(var entry in container.Mesh_RootNode.Children)
        {
            entry.EditorVisible = false;

            FLVER2.Mesh mesh = (FLVER2.Mesh)entry.WrappedObject;

            if(materialDict.ContainsKey(mesh.MaterialIndex))
            {
                var material = materialDict[mesh.MaterialIndex];

                var regex = @"\#[0-9]*\#";
                var maskIdStr = Regex.Match(material.Name, regex).Value;
                maskIdStr = maskIdStr.Replace("#", ""); // Remove the #s

                // If is a mask entry, default to false.
                if(maskIdStr != "")
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

    private static string GetInternalName()
    {
        return "modelDispMask";
    }
}
