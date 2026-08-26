using Hexa.NET.ImGui;
using HKLib.hk2018;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tracy;

namespace StudioCore.Editors.HavokEditor;

public class HavokBinderView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string BinderFilter = "";
    public bool ExactBinderFilter = false;

    public HavokBinderView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        GUI.TitleHeader(
            LOC.Get("HAVOK_BinderView_Header"),
            LOC.Get("HAVOK_BinderView_Header_TT"));

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayBinderList(Project.Locator.HavokAnimationFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            DisplayBinderList(Project.Locator.HavokBehaviorFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayBinderList(Project.Locator.HavokCharacterFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayBinderList(Project.Locator.HavokCollisionFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayBinderList(Project.Locator.HavokAssetFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayBinderList(Project.Locator.HavokNavmeshFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayBinderList(Project.Locator.HavokCutsceneFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayBinderList(Project.Locator.HavokPartFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayBinderList(Project.Locator.HavokRumbleFiles.Entries);
        }
        else
        {
            ImGui.BeginChild("havokBinderSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_BinderView_No_Source_File_Selected"));

            ImGui.EndChild();
        }
    }

    public void DisplayBinderList(HashSet<FileDictionaryEntry> entries)
    {
        DisplayHeader();

        ImGui.BeginChild("havokBinderSection", ImGuiChildFlags.Borders);

        foreach (var entry in entries)
        {
            var selected = View.Selection.BinderFileEntry == entry;
            var displayName = entry.Filename;

            if(CFG.Current.HavokEditor_BinderList_Display_Full_Path)
            {
                displayName = entry.Path;
            }

            // Normal filter
            var isMatch = EditorFilters.IsMatch(BinderFilter, displayName, ExactBinderFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{displayName}##binderEntry_{entry.Filename}", selected))
            {
                View.Selection.ClearFileSelection();
                View.Selection.BinderFileEntry = entry;

                PopulateFileList(true);
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild($"framedList_HavokBinderList", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBinderSearch", ref BinderFilter, ref ExactBinderFilter);

        // Toggle: Full Paths
        GUI.DisplayToggleButton("fullPathToggle", Icons.FileText,
            ref CFG.Current.HavokEditor_BinderList_Display_Full_Path,
            "HAVOK_BinderView_BinderPath_Display_Short",
            "HAVOK_BinderView_BinderPath_Display_Full",
            "HAVOK_BinderView_BinderPath_Display_TT");

        ImGui.EndChild();
    }

    public void Shortcuts()
    {

    }

    public void PopulateFileList(bool clearCache = false)
    {
        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            Project.Handler.HavokData.PopulateAnimationBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            Project.Handler.HavokData.PopulateBehaviorBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            Project.Handler.HavokData.PopulateCharacterBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            Project.Handler.HavokData.PopulateMapCollisionBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            Project.Handler.HavokData.PopulateAssetCollisionBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            Project.Handler.HavokData.PopulateNavmeshBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            Project.Handler.HavokData.PopulateCutsceneBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            Project.Handler.HavokData.PopulatePartBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            Project.Handler.HavokData.PopulateRumbleBank(View.Selection.BinderFileEntry, clearCache);
        }
    }
}