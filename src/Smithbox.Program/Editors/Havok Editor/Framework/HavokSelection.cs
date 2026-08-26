using HKLib.hk2018;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokSelection
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public bool DoFocus = false;

    // Category List
    public HavokCategoryMode CategoryMode = HavokCategoryMode.None;

    // Binder List
    public FileDictionaryEntry BinderFileEntry;

    // File List
    public string FilePath;

    // Properties
    public HavokPropertyViewType PropertyViewType = HavokPropertyViewType.Structured;

    // File-specific State
    public bool AppliedHavokTreeSearch = false;

    public HavokSelection(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void ClearSelection()
    {
        View.Selection.BinderFileEntry = null;
        View.Selection.FilePath = null;
        View.FileView.SoftSelectEntries.Clear();

        AppliedHavokTreeSearch = false;

        View.PropertyView.BehaviorView.ResetSelection();
    }

    public void ClearFileSelection()
    {
        View.Selection.FilePath = null;
        View.FileView.SoftSelectEntries.Clear();

        AppliedHavokTreeSearch = false;

        View.PropertyView.BehaviorView.ResetSelection();
    }

    public void ApplyFileSpecificTreeSearches(object sourceObject)
    {
        if (AppliedHavokTreeSearch)
            return;

        View.PropertyView.BehaviorView.SetupBehaviorView(sourceObject);

        AppliedHavokTreeSearch = true;
    }

    public Dictionary<string, string> BinderAliasCache = new();
    public Dictionary<string, string> FileAliasCache = new();

    public void RebuildBinderAliasCache()
    {
        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            BuildBinderAliasCache(Project.Locator.HavokAnimationFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            BuildBinderAliasCache(Project.Locator.HavokBehaviorFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            BuildBinderAliasCache(Project.Locator.HavokCharacterFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            BuildBinderAliasCache(Project.Locator.HavokCollisionFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            BuildBinderAliasCache(Project.Locator.HavokAssetFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            BuildBinderAliasCache(Project.Locator.HavokNavmeshFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            BuildBinderAliasCache(Project.Locator.HavokCutsceneFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            BuildBinderAliasCache(Project.Locator.HavokPartFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            BuildBinderAliasCache(Project.Locator.HavokRumbleFiles.Entries);
        }
    }

    public void BuildBinderAliasCache(HashSet<FileDictionaryEntry> entries)
    {
        BinderAliasCache.Clear();

        if (!CFG.Current.HavokEditor_BinderList_Display_Aliases)
            return;

        foreach (var entry in entries)
        {
            var alias = "";
            var rawName = entry.Filename;

            // Alias
            switch (View.Selection.CategoryMode)
            {
                case HavokCategoryMode.Map_Collision:
                case HavokCategoryMode.Navmesh:
                    var mapID = $"m{rawName.Substring(1)}";
                    alias = AliasHelper.GetMapNameAlias(Project, mapID);
                    break;
                case HavokCategoryMode.Animation:
                    alias = AliasHelper.GetAnimationAlias(Project, rawName);
                    break;
                case HavokCategoryMode.Behavior:
                case HavokCategoryMode.Character:
                    alias = AliasHelper.GetCharacterAlias(Project, rawName);
                    break;
                case HavokCategoryMode.Part_Collidable:
                    var partID = rawName.Replace("_l", "");
                    alias = AliasHelper.GetPartAlias(Project, partID);
                    break;
                case HavokCategoryMode.Asset_Collision:
                    var assetID = rawName.Replace("_l", "").Replace("_h", "");
                    alias = AliasHelper.GetAssetAlias(Project, assetID);
                    break;
                case HavokCategoryMode.Cutscene:
                    var cutMapId = $"m{rawName.Substring(1, 5)}_00_00";
                    alias = AliasHelper.GetMapNameAlias(Project, cutMapId);
                    break;
            }

            if(alias != "")
            {
                if (!BinderAliasCache.ContainsKey(entry.Filename))
                {
                    BinderAliasCache.Add(entry.Filename, alias);
                }
            }
        }
    }

    public void ClearFileAliasCache()
    {
        FileAliasCache.Clear();
    }

    public void AddToFileAliasCache(string filename)
    {
        if (!CFG.Current.HavokEditor_FileList_Display_Aliases)
            return;

        var alias = "";
        var rawName = Path.GetFileNameWithoutExtension(filename);

        if(filename.Contains(".dcx"))
        {
            rawName = Path.GetFileNameWithoutExtension(rawName);
        }

        // Alias
        switch (View.Selection.CategoryMode)
        {
            case HavokCategoryMode.Animation:
            case HavokCategoryMode.Navmesh:
                alias = AliasHelper.GetAnimationAlias(Project, rawName);
                break;
        }

        if (alias != "")
        {
            if (!FileAliasCache.ContainsKey(rawName))
            {
                FileAliasCache.Add(rawName, alias);
            }
        }
    }
}
