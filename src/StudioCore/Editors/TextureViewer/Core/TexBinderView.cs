using Hexa.NET.ImGui;
using HKLib.hk2018.hk;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.TextureViewer.Data;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace StudioCore.Editors.TextureViewer;

public class TexBinderView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexBinderView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Files##TextureContainerList");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.BinderList);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("TextureFileCategories");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.BinderList);

        /// --- Assets ---
        if (Editor.Project.ProjectType is ProjectType.AC6 or ProjectType.ER)
        {
            DisplayFileSection(
                "Asset", 
                TextureViewCategory.Asset, 
                new List<string>() { "/asset" },
                Editor.Project.TextureData.PrimaryBank.Entries);
        }
        else
        {
            DisplayFileSection(
                "Object", 
                TextureViewCategory.Object, 
                new List<string>() { "/obj" },
                Editor.Project.TextureData.PrimaryBank.Entries);
        }

        /// --- Characters ---
        DisplayFileSection(
            "Characters", 
            TextureViewCategory.Character, 
            new List<string>() { "/chr" },
            Editor.Project.TextureData.PrimaryBank.Entries);

        /// --- Parts ---
        DisplayFileSection(
            "Parts", 
            TextureViewCategory.Part, 
            new List<string>() { "/parts" },
            Editor.Project.TextureData.PrimaryBank.Entries);

        /// --- SFX ---
        DisplayFileSection(
            "Particles", 
            TextureViewCategory.Particle, 
            new List<string>() { "/sfx" },
            Editor.Project.TextureData.PrimaryBank.Entries);

        /// --- Menu ---
        DisplayFileSection(
            "Menu", 
            TextureViewCategory.Menu, 
            new List<string>() { "/menu" },
            Editor.Project.TextureData.PrimaryBank.Entries);

        /// --- Other ---
        if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
        {
            DisplayFileSection(
                "Other", 
                TextureViewCategory.Other, 
                new List<string>() { "/other" },
                Editor.Project.TextureData.PrimaryBank.Entries);
        }

        // ER
        if (Editor.Project.ProjectType == ProjectType.ER)
        {
            DisplayFileSection(
                "HD Icons", 
                TextureViewCategory.HDIcons, 
                new List<string>() { "solo" },
                Editor.Project.TextureData.PrimaryBank.PackedEntries);

            DisplayFileSection(
                "Map Tiles", 
                TextureViewCategory.MapTiles, 
                new List<string>() { "maptile" },
                Editor.Project.TextureData.PrimaryBank.PackedEntries);
        }

        ImGui.EndChild();

        ImGui.End();
    }

    public Dictionary<string, string> AliasCache = new();

    /// <summary>
    /// The UI for each container category type
    /// </summary>
    private void DisplayFileSection(string title, TextureViewCategory displayCategory, List<string> pathFilter, Dictionary<FileDictionaryEntry, BinderContents> dict)
    {
        if (ImGui.CollapsingHeader($"{title}"))
        {
            var filteredEntries = new Dictionary<FileDictionaryEntry, BinderContents>();

            // Helper to get alias with caching
            string GetAlias(string rawName)
            {
                string cacheKey = $"{displayCategory}:{rawName}";
                if (!AliasCache.TryGetValue(cacheKey, out var alias))
                {
                    alias = displayCategory switch
                    {
                        TextureViewCategory.Character => AliasUtils.GetCharacterAlias(Editor.Project, rawName),
                        TextureViewCategory.Asset => AliasUtils.GetAssetAlias(Editor.Project, rawName),
                        TextureViewCategory.Part => AliasUtils.GetPartAlias(Editor.Project, rawName),
                        _ => ""
                    };
                    AliasCache[cacheKey] = alias;
                }
                return alias;
            }

            // Build pre-filtered list
            foreach (var entry in dict)
            {
                var isValidCategory = false;
                var addEntry = false;

                foreach (var fileType in pathFilter)
                {
                    if (entry.Key.Path.Contains(fileType))
                    {
                        if (!filteredEntries.ContainsKey(entry.Key))
                        {
                            isValidCategory = true;
                        }
                    }
                }

                if (isValidCategory)
                {
                    var rawName = entry.Key.Filename.ToLower();
                    var aliasName = GetAlias(rawName);

                    if (Editor.Filters.IsFileFilterMatch(entry.Key.Path, aliasName))
                    {
                        if (!CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries)
                        {
                            if (entry.Key.Filename.Contains("_l"))
                            {
                                continue;
                            }
                        }

                        addEntry = true;
                    }
                }

                if (addEntry)
                {
                    filteredEntries.Add(entry.Key, entry.Value);
                }
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var entry = filteredEntries.ElementAt(i);
                    var rawName = entry.Key.Filename.ToLower();
                    var aliasName = GetAlias(rawName);

                    ImGui.BeginGroup();

                    var displayName = entry.Key.Path;

                    var isSelected = false;
                    if (Editor.Selection.SelectedFileEntry != null)
                    {
                        if (Editor.Selection.SelectedFileEntry.Path == entry.Key.Path)
                        {
                            isSelected = true;
                        }
                    }

                    // File row
                    if (ImGui.Selectable($@"{displayName}##{entry.Key.Path}{i}", isSelected))
                    {
                        LoadTextureBinder = true;
                        TargetDict = dict;
                        TargetTextureBinderEntry = entry.Key;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectFile)
                    {
                        Editor.Selection.SelectFile = false;
                        TargetDict = dict;
                        LoadTextureBinder = true;
                        TargetTextureBinderEntry = entry.Key;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectFile = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        var alias = AliasUtils.GetTextureContainerAliasName(Editor.Project, entry.Key.Filename, displayCategory);
                        UIHelper.DisplayAlias(alias);
                    }

                    ImGui.EndGroup();
                }
            }

            clipper.End();
        }
    }

    private FileDictionaryEntry TargetTextureBinderEntry;
    private Dictionary<FileDictionaryEntry, BinderContents> TargetDict;
    private bool LoadTextureBinder = false;

    public void Update()
    {
        if (TargetTextureBinderEntry == null)
            return;

        if (TargetDict == null)
            return;

        if (LoadTextureBinder)
        {
            Editor.Selection.ResetSelection();

            if (TargetTextureBinderEntry.Extension == "tpfbhd")
            {
                Task<bool> loadTask = Project.TextureData.PrimaryBank.LoadPackedTextureBinder(TargetTextureBinderEntry);
                Task.WaitAll(loadTask);
            }
            else
            {
                Task<bool> loadTask = Project.TextureData.PrimaryBank.LoadTextureBinder(TargetTextureBinderEntry);
                Task.WaitAll(loadTask);
            }

            var targetBinder = TargetDict.FirstOrDefault(e => e.Key.Path == TargetTextureBinderEntry.Path);

            if (targetBinder.Key != null)
            {
                Editor.Selection.SelectTextureFile(targetBinder.Key, targetBinder.Value);
            }

            LoadTextureBinder = false;
        }
    }
}
