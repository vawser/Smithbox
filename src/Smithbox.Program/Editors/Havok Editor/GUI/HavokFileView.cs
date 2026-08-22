using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokFileView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string FileFilter = "";
    public bool ExactFileFilter = false;

    public HavokFileView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        GUI.SimpleHeader(
            LOC.Get("HAVOK_FileView_Header"),
            LOC.Get("HAVOK_FileView_Header_TT"));

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayFileList(data.AnimationBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            DisplayFileList(data.BehaviorBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayFileList(data.CharacterBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayFileList(data.MapCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayFileList(data.AssetCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayFileList(data.NavmeshBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayFileList(data.CutsceneBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayFileList(data.PartBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayFileList(data.RumbleBank);
        }
        else
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText("No source file has been selected yet.");

            ImGui.EndChild();
        }
    }

    public void DisplayFileList(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict)
    {
        if(View.Selection.BinderFileEntry == null)
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText("No source file has been selected yet.");

            ImGui.EndChild();
            return;
        }

        if (!bankDict.ContainsKey(View.Selection.BinderFileEntry))
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"Bank does not contain a source file with this path:\n{View.Selection.BinderFileEntry.Path}");

            ImGui.EndChild();

            return;
        }

        var curBinder = bankDict[View.Selection.BinderFileEntry];

        DisplayHeader();

        ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

        foreach (var entry in curBinder)
        {
            var filepath = entry.Key;
            var selected = View.Selection.FilePath == filepath;
            var displayName = Path.GetFileNameWithoutExtension(entry.Key);

            if(CFG.Current.HavokEditor_FileList_Display_Full_Path)
            {
                displayName = entry.Key;
            }

            // Normal filter
            var isMatch = EditorFilters.IsMatch(FileFilter, displayName, ExactFileFilter);

            if (!isMatch)
                continue;

            // Only display .hkx files
            if (filepath.EndsWith(".hkx") || filepath.EndsWith(".hkx.dcx"))
            {
                if (ImGui.Selectable($"{displayName}##fileEntry_{filepath}", selected))
                {
                    View.Selection.ClearFileSelection();

                    View.Selection.FilePath = filepath;
                    LoadHavokFile();
                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_HavokFileList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokFileSearch", ref FileFilter, ref ExactFileFilter);

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##toggleFullPathName"))
        {
            CFG.Current.HavokEditor_FileList_Display_Full_Path = !CFG.Current.HavokEditor_FileList_Display_Full_Path;
        }

        var fullPathVis = "Show Short Name";
        if (CFG.Current.HavokEditor_FileList_Display_Full_Path)
            fullPathVis = "Show Full Name";

        GUI.Tooltip($"Toggle the display name used in the file list.\nCurrent Mode: {fullPathVis}");

        ImGui.EndChild();
    }

    public void LoadHavokFile()
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            data.LoadAnimationFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            data.LoadBehaviorFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            data.LoadCharacterFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.LoadMapCollisionFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            data.LoadAssetCollisionFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            data.LoadNavmeshFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            data.LoadCutsceneFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            data.LoadPartFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            data.LoadRumbleFile(View.Selection.BinderFileEntry, View.Selection.FilePath);
        }
    }
}