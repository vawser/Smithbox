using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexContainerList
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    public TexContainerList(TexEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Containers", "");
        Parent.Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("ContainerList", new Vector2(width, height));

        ImGui.BeginTabBar("sourceTabs");

        //DisplayDebugSection();
        DisplayFileCategories_DES();
        DisplayFileCategories_DS1();
        DisplayFileCategories_DS2();
        DisplayFileCategories_DS3();
        DisplayFileCategories_BB();
        DisplayFileCategories_SDT();
        DisplayFileCategories_ER();
        DisplayFileCategories_AC6();
        DisplayFileCategories_NR();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public Dictionary<string, string> AliasCache = new();

    /// <summary>
    /// The UI for each container category type
    /// </summary>
    private void DisplayFileSection(string title, TextureViewCategory displayCategory, List<string> pathFilter, Dictionary<FileDictionaryEntry, BinderContents> dict)
    {
        if (ImGui.BeginTabItem($"{title}"))
        {
            ImGui.BeginChild($"texSourceList_{title}");

            var filteredEntries = new Dictionary<FileDictionaryEntry, BinderContents>();

            // Helper to get alias with caching
            string GetAlias(string rawName)
            {
                string cacheKey = $"{displayCategory}:{rawName}";
                if (!AliasCache.TryGetValue(cacheKey, out var alias))
                {
                    alias = displayCategory switch
                    {
                        TextureViewCategory.Characters => AliasHelper.GetCharacterAlias(Project, rawName),
                        TextureViewCategory.Assets => AliasHelper.GetAssetAlias(Project, rawName),
                        TextureViewCategory.Parts => AliasHelper.GetPartAlias(Project, rawName),
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

                    if (Parent.Editor.Filters.IsFileFilterMatch(entry.Key.Path, aliasName))
                    {
                        if (!CFG.Current.TextureViewer_File_List_Display_Low_Detail_Entries)
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
                    if (Parent.Selection.SelectedFileEntry != null)
                    {
                        if (Parent.Selection.SelectedFileEntry.Path == entry.Key.Path)
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

                        Parent.Editor.ViewHandler.ActiveView = Parent;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectFile)
                    {
                        Parent.Selection.SelectFile = false;
                        TargetDict = dict;
                        LoadTextureBinder = true;
                        TargetTextureBinderEntry = entry.Key;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectFile = true;
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        var alias = AliasHelper.GetTextureContainerAliasName(Project, entry.Key.Filename, displayCategory);
                        UIHelper.DisplayAlias(alias);
                    }

                    ImGui.EndGroup();
                }
            }

            clipper.End();

            ImGui.EndChild();
            ImGui.EndTabItem();
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
            Parent.Selection.ResetSelection();

            if (TargetTextureBinderEntry.Extension == "tpfbhd")
            {
                Task<bool> loadTask = Project.Handler.TextureData.PrimaryBank.LoadPackedTextureBinder(TargetTextureBinderEntry);
                Task.WaitAll(loadTask);
            }
            else
            {
                Task<bool> loadTask = Project.Handler.TextureData.PrimaryBank.LoadTextureBinder(TargetTextureBinderEntry);
                Task.WaitAll(loadTask);
            }

            var targetBinder = TargetDict.FirstOrDefault(e => e.Key.Path == TargetTextureBinderEntry.Path);

            if (targetBinder.Key != null)
            {
                Parent.Selection.SelectTextureFile(targetBinder.Key, targetBinder.Value);
            }

            LoadTextureBinder = false;
        }
    }

    /// <summary>
    /// List of all the files catelogued
    /// </summary>
    public void DisplayDebugSection()
    {
        if (ImGui.CollapsingHeader("Texture Files"))
        {
            foreach (var entry in Project.Locator.TextureFiles.Entries)
            {
                ImGui.Text($"{entry.Path}");
            }
        }

        if (ImGui.CollapsingHeader("Packed Texture Files"))
        {
            foreach (var entry in Project.Locator.TexturePackedFiles.Entries)
            {
                ImGui.Text($"{entry.Path}");
            }
        }
    }

    public void DisplayFileCategories_DES()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DES)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Facegen
            DisplayFileSection(
                "Facegen",
                TextureViewCategory.Particles,
                new List<string>() { "/facegen" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Item
            DisplayFileSection(
                "Items",
                TextureViewCategory.Particles,
                new List<string>() { "/item" },
                Project.Handler.TextureData.PrimaryBank.Entries);
        }
    }

    public void DisplayFileCategories_DS1()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_DS2()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/model/chr", "/model_lq/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object (model/object)
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Objects,
                new List<string>() { "/model/object", "/model_lq/object" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts (model/parts)
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/model/parts", "/model_lq/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu (menu)
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Map,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX (sfx)
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Map,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map (model/map)
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/model/map", "/model_lq/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_DS3()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS3)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Adhoc
            DisplayFileSection(
                "Adhoc",
                TextureViewCategory.Adhoc,
                new List<string>() { "/adhoc" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_BB()
    {
        if (Project.Descriptor.ProjectType is ProjectType.BB)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_SDT()
    {
        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_ER()
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // High Definition Icons
            DisplayFileSection(
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // Map Tiles
            DisplayFileSection(
                "Map Tiles",
                TextureViewCategory.MapTiles,
                new List<string>() { "maptile" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_AC6()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            /// Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            /// SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map Textures
            DisplayFileSection(
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // High Definition Icons
            DisplayFileSection(
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // Terms of Service
            DisplayFileSection(
                "Terms of Service",
                TextureViewCategory.TOS,
                new List<string>() { "_tos_" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }

    public void DisplayFileCategories_NR()
    {
        if (Project.Descriptor.ProjectType is ProjectType.NR)
        {
            // Chr
            DisplayFileSection(
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // High Definition Icons
            DisplayFileSection(
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }
}
