using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexContainerList
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

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
        UIHelper.SimpleHeader(
            LOC.Get("TEXVIEW_ContainerList_Header_Containers"),
            LOC.Get("TEXVIEW_ContainerList_Header_Containers_TT"));

        EditorFilters.DisplayFramedListFilter("textureViewer_ContainerList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.BeginChild("ContainerList", new Vector2(0, 0), ImGuiChildFlags.Borders);

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
    private void DisplayFileSection(string title, string imguiKey, TextureViewCategory displayCategory, List<string> pathFilter, Dictionary<FileDictionaryEntry, BinderContents> dict)
    {
        if (ImGui.BeginTabItem($"{title}###tab_{imguiKey}"))
        {
            ImGui.BeginChild($"texSourceList_{imguiKey}#");

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

                    var isMatch = EditorFilters.IsMatch(
                        FileListFilter, entry.Key.Path, ExactFileListFilter, aliasName, true, true);

                    if (isMatch)
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

                    ContextMenu(entry, i, displayCategory);
                }
            }

            clipper.End();

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }

    private void ContextMenu(KeyValuePair<FileDictionaryEntry, BinderContents> entry, int index, TextureViewCategory displayCategory)
    {
        if (ImGui.BeginPopupContextItem($"context_{entry.Key.Path}{index}"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_Copy_to_Project")}##copyToProjectAction"))
            {
                var outputPath = Path.Join(Project.Descriptor.ProjectPath, entry.Key.Path);
                entry.Value.WriteBinder(outputPath);
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_ContainerList_Context_Copy_to_Project_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("TEXVIEW_ContainerList_Context_Header_Export")}##exportMenuHeader"))
            {
                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_All_TPFs")}##allTpfsAction"))
                {
                    _ = Parent.Editor.ToolView.TextureExport.ExportTPFsFromContainerAsync(entry.Value);
                }
                UIHelper.Tooltip(
                    LOC.Get("TEXVIEW_ContainerList_Context_All_TPFs_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_All_Textures")}##allTexturesAction"))
                {
                    _ = Parent.Editor.ToolView.TextureExport.ExportTexturesFromContainerAsync(entry.Value);
                }
                UIHelper.Tooltip(
                    LOC.Get("TEXVIEW_ContainerList_Context_All_Textures_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_Copy_Path")}##copyPath"))
            {
                ImGui.SetClipboardText(entry.Key.Path);
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_ContainerList_Context_Copy_Path_TT"));

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_Copy_Filename")}##copyFilename"))
            {
                ImGui.SetClipboardText(entry.Key.Filename);
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_ContainerList_Context_Copy_Filename_TT"));

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ContainerList_Context_Copy_Alias")}##copyAlias"))
            {
                var alias = AliasHelper.GetTextureContainerAliasName(Project, entry.Key.Filename, displayCategory);
                ImGui.SetClipboardText(alias);
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_ContainerList_Context_Copy_Alias_TT"));

            ImGui.EndPopup();
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
            Parent.Selection.AutoSelectTpf = true;
        }
    }

    public void DisplayFileCategories_DES()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DES)
        {
            // Chr
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Fonts"),
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Facegen
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Facegen"),
                "Facegen",
                TextureViewCategory.Particles,
                new List<string>() { "/facegen" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Item
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Items"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Fonts"),
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/model/chr", "/model_lq/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object (model/object)
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Objects,
                new List<string>() { "/model/object", "/model_lq/object" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts (model/parts)
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/model/parts", "/model_lq/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu (menu)
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Map,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX (sfx)
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Map,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map (model/map)
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Fonts"),
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Adhoc
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Adhoc"),
                "Adhoc",
                TextureViewCategory.Adhoc,
                new List<string>() { "/adhoc" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Object
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Objects"),
                "Objects",
                TextureViewCategory.Assets,
                new List<string>() { "/obj" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Font
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Fonts"),
                "Fonts",
                TextureViewCategory.Particles,
                new List<string>() { "/font" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Packed: Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Assets"),
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // High Definition Icons
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_HD_Icons"),
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // Map Tiles
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Tiles"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Assets"),
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            /// Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            /// SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map Textures
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map_Textures"),
                "Map Textures",
                TextureViewCategory.MapTextures,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // High Definition Icons
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_HD_Icons"),
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);

            // Terms of Service
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_TOS"),
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
                LOC.Get("TEXVIEW_Category_Characters"),
                "Characters",
                TextureViewCategory.Characters,
                new List<string>() { "/chr" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Assets
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Assets"),
                "Asset",
                TextureViewCategory.Assets,
                new List<string>() { "/asset" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Parts
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Parts"),
                "Parts",
                TextureViewCategory.Parts,
                new List<string>() { "/parts" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Map
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Map"),
                "Map",
                TextureViewCategory.Map,
                new List<string>() { "/map" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Menu
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Menu"),
                "Menu",
                TextureViewCategory.Menu,
                new List<string>() { "/menu" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // Other
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Other"),
                "Other",
                TextureViewCategory.Other,
                new List<string>() { "/other" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // SFX
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_Particles"),
                "Particles",
                TextureViewCategory.Particles,
                new List<string>() { "/sfx" },
                Project.Handler.TextureData.PrimaryBank.Entries);

            // High Definition Icons
            DisplayFileSection(
                LOC.Get("TEXVIEW_Category_HD_Icons"),
                "HD Icons",
                TextureViewCategory.HighDefinitionIcons,
                new List<string>() { "solo" },
                Project.Handler.TextureData.PrimaryBank.PackedEntries);
        }
    }
}
