using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using StudioCore.Interface;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Utilities;
using StudioCore.Locators;
using StudioCore.Core;
using StudioCore.Editors.TextEditor;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using System.IO;

namespace StudioCore.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    private readonly PropertyEditor _propEditor;

    public FMGEntryGroup _activeEntryGroup;
    public FMGInfo _activeFmgInfo;
    public static int _activeIDCache = -1;
    private bool _arrowKeyPressed;

    private bool _clearEntryGroup;

    private List<FMG.Entry> _entryLabelCache;
    private List<FMG.Entry> _EntryLabelCacheFiltered;

    private string _searchFilter = "";
    public static string _searchFilterCached = "";
    private string _fmgSearchAllString = "";
    private bool _fmgSearchAllActive = false;
    private List<FMGInfo> _filteredFmgInfo = new();
    public static ActionManager EditorActionManager = new();

    private TextToolbar _textToolbar;
    private TextToolbar_ActionList _textToolbar_ActionList;
    private TextToolbar_Configuration _textToolbar_Configuration;

    public TextEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);

        _textToolbar = new TextToolbar(EditorActionManager);
        _textToolbar_ActionList = new TextToolbar_ActionList();
        _textToolbar_Configuration = new TextToolbar_Configuration();
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";

    public void Init()
    {
        ShowSaveOption = true;
    }

    public void DrawEditorMenu()
    {
        var currentFmgBank = Smithbox.BankHandler.FMGBank;

        if (ImGui.BeginMenu("Edit", currentFmgBank.IsLoaded))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Scissors}");
            if (ImGui.MenuItem("Remove", KeyBindings.Current.Core_Delete.HintText, false,
                    _activeEntryGroup != null))
            {
                TextAction_Delete.DeleteSelectedEntry();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false,
                    _activeEntryGroup != null))
            {
                TextAction_Duplicate.DuplicateSelectedEntry();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Text Categories"))
            {
                CFG.Current.Interface_TextEditor_TextCategories = !CFG.Current.Interface_TextEditor_TextCategories;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextCategories);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Text Entry"))
            {
                CFG.Current.Interface_TextEditor_TextEntry = !CFG.Current.Interface_TextEditor_TextEntry;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextEntry);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Toolbar"))
            {
                CFG.Current.Interface_TextEditor_Toolbar = !CFG.Current.Interface_TextEditor_Toolbar;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_Toolbar);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Data", Smithbox.BankHandler.FMGBank.IsLoaded))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
            if (ImGui.BeginMenu("Merge"))
            {
                ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
                    "Import: text will be merged with currently loaded text");

                if (ImGui.MenuItem("Import text files and merge"))
                {
                    if (FmgExporter.ImportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], true))
                    {
                        ClearTextEditorCache();
                        ResetActionManager();
                    }
                }

                ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
                    "Export: only modded text (different than vanilla) will be exported");
                if (ImGui.MenuItem("Export modded text to text files"))
                {
                    FmgExporter.ExportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], true);
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
            if (ImGui.BeginMenu("All"))
            {
                ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
                    "Import: text replaces currently loaded text entirely");

                if (ImGui.MenuItem("Import text files and replace"))
                {
                    if (FmgExporter.ImportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], false))
                    {
                        ClearTextEditorCache();
                        ResetActionManager();
                    }
                }

                ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
                    "Export: all text will be exported");
                if (ImGui.MenuItem("Export all text to text files"))
                {
                    FmgExporter.ExportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], false);
                }

                if (ImGui.BeginMenu("Legacy"))
                {
                    ImGui.TextColored(new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
                        "Old version of text import/export system.\n" +
                        "Import: text replaces currently loaded text entirely.");
                    if (ImGui.MenuItem("Import json"))
                    {
                        if (FmgExporter.ImportFmgJson(false))
                        {
                            ClearTextEditorCache();
                            ResetActionManager();
                        }
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Text Language", !currentFmgBank.IsLoading))
        {
            Dictionary<string, string> folders = ResourceTextLocator.GetMsgLanguages();
            if (folders.Count == 0)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Cannot find language folders.");
            }
            else
            {
                foreach (KeyValuePair<string, string> path in folders)
                {
                    string disp = path.Key;

                    if (FMGDictionaries.Languages.ContainsKey(path.Key))
                        disp = FMGDictionaries.Languages[path.Key];

                    if (currentFmgBank.fmgLangs.ContainsKey(path.Key))
                    {
                        disp += "*";
                    }

                    if (ImGui.MenuItem(disp, "", currentFmgBank.LanguageFolder == path.Key))
                    {
                        ChangeLanguage(path.Key);
                    }
                }
            }

            ImGui.EndMenu();
        }

        if (DisplayFmgUpdate && IsSupportedProjectType_FmgUpdate())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Warning_Text_Color);
            if (ImGui.Button("Update FMGs"))
            {
                UpdateFmgs();
            }
            ImGui.PopStyleColor();
            ImguiUtils.ShowHoverTooltip("Your mod has unique FMG entries that are not in the latest FMG binder. Use this action to move them into it.");
        }
    }

    private bool DisplayFmgUpdate = false;
    private Dictionary<FmgIDType, List<FMG.Entry>> itemEntriesToUpdate;
    private Dictionary<FmgIDType, List<FMG.Entry>> menuEntriesToUpdate;

    private bool IsSupportedProjectType_FmgUpdate()
    {
        if(Smithbox.ProjectType is ProjectType.ER)
            return true;

        return false;
    }

    private void SetupFmgUpdate()
    {
        // Only support ER for now, although technically DS3 should be covered
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            var langFolder = Smithbox.BankHandler.FMGBank.LanguageFolder;

            Dictionary<FmgFileCategory, FMGFileSet> Project_Item_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Project_DLC_Item_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Base_Item_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Base_DLC_Item_VanillaFmgInfoBanks = new();

            Dictionary<FmgFileCategory, FMGFileSet> Project_Menu_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Project_DLC_Menu_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Base_Menu_VanillaFmgInfoBanks = new();
            Dictionary<FmgFileCategory, FMGFileSet> Base_DLC_Menu_VanillaFmgInfoBanks = new();

            ResourceDescriptor projectItemMsgPath = ResourceTextLocator.GetMsgbnd_Project_Upgrader("item", "", langFolder);
            ResourceDescriptor projectDlcItemMsgPath = ResourceTextLocator.GetMsgbnd_Project_Upgrader("item", "_dlc02", langFolder);
            ResourceDescriptor baseItemMsgPath = ResourceTextLocator.GetMsgbnd_Vanilla_Upgrader("item", "", langFolder);
            ResourceDescriptor baseDlcItemMsgPath = ResourceTextLocator.GetMsgbnd_Vanilla_Upgrader("item", "_dlc02", langFolder);

            ResourceDescriptor projectMenuMsgPath = ResourceTextLocator.GetMsgbnd_Project_Upgrader("menu", "", langFolder);
            ResourceDescriptor projectDlcMenuMsgPath = ResourceTextLocator.GetMsgbnd_Project_Upgrader("menu", "_dlc02", langFolder);
            ResourceDescriptor baseMenuMsgPath = ResourceTextLocator.GetMsgbnd_Vanilla_Upgrader("menu", "", langFolder);
            ResourceDescriptor baseDlcMenuMsgPath = ResourceTextLocator.GetMsgbnd_Vanilla_Upgrader("menu", "_dlc02", langFolder);

            // If the asset paths do not exist, return early to stop a failed msgbnd load
            if (!File.Exists(projectItemMsgPath.AssetPath) ||
                !File.Exists(projectDlcItemMsgPath.AssetPath) ||
                !File.Exists(baseItemMsgPath.AssetPath) ||
                !File.Exists(baseDlcItemMsgPath.AssetPath) ||
                !File.Exists(projectMenuMsgPath.AssetPath) ||
                !File.Exists(projectDlcMenuMsgPath.AssetPath) ||
                !File.Exists(baseMenuMsgPath.AssetPath) ||
                !File.Exists(baseDlcMenuMsgPath.AssetPath))
                return;

            FMGFileSet projectItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
            FMGFileSet projectDlcItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
            FMGFileSet baseItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
            FMGFileSet baseDlcItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
            FMGFileSet projectMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
            FMGFileSet projectDlcMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
            FMGFileSet baseMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
            FMGFileSet baseDlcMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);

            if (projectItemMsgBnd.LoadMsgBnd(projectItemMsgPath.AssetPath, "item.msgbnd"))
                Project_Item_VanillaFmgInfoBanks.Add(projectItemMsgBnd.FileCategory, projectItemMsgBnd);

            if (projectDlcItemMsgBnd.LoadMsgBnd(projectDlcItemMsgPath.AssetPath, "item.msgbnd"))
                Project_DLC_Item_VanillaFmgInfoBanks.Add(projectDlcItemMsgBnd.FileCategory, projectDlcItemMsgBnd);

            if (baseItemMsgBnd.LoadMsgBnd(baseItemMsgPath.AssetPath, "item.msgbnd"))
                Base_Item_VanillaFmgInfoBanks.Add(baseItemMsgBnd.FileCategory, baseItemMsgBnd);

            if (baseDlcItemMsgBnd.LoadMsgBnd(baseDlcItemMsgPath.AssetPath, "item.msgbnd"))
                Base_DLC_Item_VanillaFmgInfoBanks.Add(baseDlcItemMsgBnd.FileCategory, baseDlcItemMsgBnd);

            if (projectMenuMsgBnd.LoadMsgBnd(projectMenuMsgPath.AssetPath, "menu.msgbnd"))
                Project_Menu_VanillaFmgInfoBanks.Add(projectMenuMsgBnd.FileCategory, projectMenuMsgBnd);

            if (projectDlcMenuMsgBnd.LoadMsgBnd(projectDlcMenuMsgPath.AssetPath, "menu.msgbnd"))
                Project_DLC_Menu_VanillaFmgInfoBanks.Add(projectDlcMenuMsgBnd.FileCategory, projectDlcMenuMsgBnd);

            if (baseMenuMsgBnd.LoadMsgBnd(baseMenuMsgPath.AssetPath, "menu.msgbnd"))
                Base_Menu_VanillaFmgInfoBanks.Add(baseMenuMsgBnd.FileCategory, baseMenuMsgBnd);

            if (baseDlcMenuMsgBnd.LoadMsgBnd(baseDlcMenuMsgPath.AssetPath, "menu.msgbnd"))
                Base_DLC_Menu_VanillaFmgInfoBanks.Add(baseDlcMenuMsgBnd.FileCategory, baseDlcMenuMsgBnd);

            itemEntriesToUpdate = new Dictionary<FmgIDType, List<FMG.Entry>>();
            menuEntriesToUpdate = new Dictionary<FmgIDType, List<FMG.Entry>>();

            itemEntriesToUpdate = GetEntriesToUpdate(Project_Item_VanillaFmgInfoBanks, Project_DLC_Item_VanillaFmgInfoBanks, Base_Item_VanillaFmgInfoBanks, Base_DLC_Item_VanillaFmgInfoBanks);
            menuEntriesToUpdate = GetEntriesToUpdate(Project_Menu_VanillaFmgInfoBanks, Project_DLC_Menu_VanillaFmgInfoBanks, Base_Menu_VanillaFmgInfoBanks, Base_DLC_Menu_VanillaFmgInfoBanks);

            foreach (var entry in itemEntriesToUpdate)
            {
                foreach (var fmgEntry in entry.Value)
                {
                    DisplayFmgUpdate = true;
                    //TaskLogs.AddLog($"New Item Entry: {entry.Key} - {fmgEntry.ID} {fmgEntry.Text}");
                }
            }
            foreach (var entry in menuEntriesToUpdate)
            {
                foreach (var fmgEntry in entry.Value)
                {
                    DisplayFmgUpdate = true;
                    //TaskLogs.AddLog($"New Menu Entry: {entry.Key} - {fmgEntry.ID} {fmgEntry.Text}");
                }
            }
        }
    }

    private void UpdateFmgs()
    {
        ApplyFmgUpdate(itemEntriesToUpdate);
        ApplyFmgUpdate(menuEntriesToUpdate);

        Smithbox.BankHandler.FMGBank.SaveFMGs();

        DisplayFmgUpdate = false;

        SetupFmgUpdate(); // Re-run this to check FMG update state and set to false if update was successful
    }

    private void ApplyFmgUpdate(Dictionary<FmgIDType, List<FMG.Entry>> entries)
    {
        var fmgBank = Smithbox.BankHandler.FMGBank;

        foreach(var updateEntry in entries)
        {
            foreach (var entry in fmgBank.FmgInfoBank)
            {
                if(updateEntry.Key == entry.FmgID)
                {
                    foreach (var fmgEntry in updateEntry.Value)
                    {
                        entry.Fmg.Entries.Add(fmgEntry);
                    }
                }
            }
        }
    }

    private Dictionary<FmgIDType, List<FMG.Entry>> GetEntriesToUpdate(Dictionary<FmgFileCategory, FMGFileSet> projectInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> projectDlcInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> baseInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> baseDlcInfoBanks)
    {
        var entries = new Dictionary<FmgIDType, List<FMG.Entry>>();

        // Populate entries with the project vanilla entries
        foreach(var bank in projectInfoBanks)
        {
            foreach(var fmgInfo in bank.Value.FmgInfos)
            {
                foreach(var entry in fmgInfo.Fmg.Entries)
                {
                    if(!entries.ContainsKey(fmgInfo.FmgID))
                    {
                        entries.Add(fmgInfo.FmgID, new List<FMG.Entry>());
                    }

                    entries[fmgInfo.FmgID].Add(entry);
                }
            }
        }

        // Remove any matches from the entries if they are present in the vanilla FMGs
        foreach (var bank in baseInfoBanks)
        {
            foreach (var pEntry in entries)
            {
                foreach (var fmgInfo in bank.Value.FmgInfos)
                {
                    if (pEntry.Key == fmgInfo.FmgID)
                    {
                        foreach (var entry in fmgInfo.Fmg.Entries)
                        {
                            foreach (var pFmgEntry in pEntry.Value)
                            {
                                if (entry.ID == pFmgEntry.ID)
                                {
                                    entries[pEntry.Key].Remove(pFmgEntry);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Remove any matches from the entries if they are present in the vanilla DLC2 FMGs
        foreach (var bank in baseDlcInfoBanks)
        {
            foreach (var pEntry in entries)
            {
                foreach (var fmgInfo in bank.Value.FmgInfos)
                {
                    if (pEntry.Key == fmgInfo.FmgID)
                    {
                        foreach (var entry in fmgInfo.Fmg.Entries)
                        {
                            foreach (var pFmgEntry in pEntry.Value)
                            {
                                if (entry.ID == pFmgEntry.ID)
                                {
                                    entries[pEntry.Key].Remove(pFmgEntry);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Remove any matches from the entries if they are present in the project DLC2 FMGs
        foreach (var bank in projectDlcInfoBanks)
        {
            foreach (var pEntry in entries)
            {
                foreach (var fmgInfo in bank.Value.FmgInfos)
                {
                    if (pEntry.Key == fmgInfo.FmgID)
                    {
                        foreach (var entry in fmgInfo.Fmg.Entries)
                        {
                            foreach (var pFmgEntry in pEntry.Value)
                            {
                                if (entry.ID == pFmgEntry.ID)
                                {
                                    entries[pEntry.Key].Remove(pFmgEntry);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


        return entries;
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TextEntries");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            ImGui.Begin("Editor##InvalidTextEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            var currentFmgBank = Smithbox.BankHandler.FMGBank;

            if (!ImGui.IsAnyItemActive() && currentFmgBank.IsLoaded)
            {
                // Only allow key shortcuts when an item [text box] is not currently activated
                if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
                {
                    EditorActionManager.UndoAction();
                }

                if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
                {
                    EditorActionManager.RedoAction();
                }

                if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete) && _activeEntryGroup != null)
                {
                    TextAction_Delete.DeleteSelectedEntry();
                }

                if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate) && _activeEntryGroup != null)
                {
                    TextAction_Duplicate.DuplicateSelectedEntry();
                }
            }

            var doFocus = false;
            // Parse select commands
            if (initcmd != null && initcmd[0] == "select")
            {
                if (initcmd.Length > 1)
                {
                    // Select FMG
                    doFocus = true;
                    // Use three possible keys: entry category is for param references,
                    // binder id and FMG name are for soapstone references.
                    // This can be revisited as more high-level categories get added.
                    int? searchId = null;
                    FmgEntryCategory? searchCategory = null;
                    string searchName = null;
                    if (int.TryParse(initcmd[1], out var intId) && intId >= 0)
                    {
                        searchId = intId;
                    }
                    // Enum.TryParse allows arbitrary ints (thanks C#), so checking definition is required
                    else if (Enum.TryParse(initcmd[1], out FmgEntryCategory cat)
                             && Enum.IsDefined(typeof(FmgEntryCategory), cat))
                    {
                        searchCategory = cat;
                    }
                    else
                    {
                        searchName = initcmd[1];
                    }

                    foreach (FMGInfo info in currentFmgBank.FmgInfoBank)
                    {
                        var match = false;
                        // This matches top-level item FMGs
                        if (info.EntryCategory.Equals(searchCategory) && info.PatchParent == null
                                                                      && info.EntryType is FmgEntryTextType.Title
                                                                          or FmgEntryTextType.TextBody)
                        {
                            match = true;
                        }
                        else if (searchId is int binderId && binderId == (int)info.FmgID)
                        {
                            match = true;
                        }
                        else if (info.Name == searchName)
                        {
                            match = true;
                        }

                        if (match)
                        {
                            _activeFmgInfo = info;
                            break;
                        }
                    }

                    if (initcmd.Length > 2 && _activeFmgInfo != null)
                    {
                        // Select Entry
                        var parsed = int.TryParse(initcmd[2], out var id);
                        if (parsed)
                        {
                            _activeEntryGroup = currentFmgBank.GenerateEntryGroup(id, _activeFmgInfo);
                        }
                    }
                }
            }

            EditorGUI(doFocus);
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        SetupFmgUpdate();
        _fmgSearchAllString = "";
        _filteredFmgInfo.Clear();
        ClearTextEditorCache();
        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        Smithbox.BankHandler.FMGBank.SaveFMGs();

        SetupFmgUpdate(); // Re-run this to check FMG update state
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        Smithbox.BankHandler.FMGBank.SaveFMGs();

        SetupFmgUpdate(); // Re-run this to check FMG update state
    }

    private void ClearTextEditorCache()
    {
        UICache.ClearCaches();
        _entryLabelCache = null;
        _EntryLabelCacheFiltered = null;
        _activeFmgInfo = null;
        _activeEntryGroup = null;
        _activeIDCache = -1;
        _searchFilter = "";
        _searchFilterCached = "";
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    /// <summary>
    ///     Deletes all Entries within active EntryGroup from their FMGs
    /// </summary>
    private void DeleteFMGEntries(FMGEntryGroup entry)
    {
        var action = new DeleteFMGEntryAction(entry);
        EditorActionManager.ExecuteAction(action);
        _activeEntryGroup = null;
        _activeIDCache = -1;

        // Lazy method to refresh search filter
        _searchFilterCached = "";
    }

    private void FMGSearchLogic(ref bool doFocus)
    {
        if (_entryLabelCache != null)
        {
            if (_searchFilter != _searchFilterCached)
            {
                List<FMG.Entry> matches = new();
                _EntryLabelCacheFiltered = _entryLabelCache;

                List<FMG.Entry> mainEntries;
                if (_searchFilter.Length > _searchFilterCached.Length)
                {
                    mainEntries = _EntryLabelCacheFiltered;
                }
                else
                {
                    mainEntries = _entryLabelCache;
                }

                // Title/Textbody
                foreach (FMG.Entry entry in mainEntries)
                {
                    if (entry.ID.ToString().Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // ID search
                        matches.Add(entry);
                    }
                    else if (entry.Text != null)
                    {
                        // Text search
                        if (entry.Text.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase))
                        {
                            matches.Add(entry);
                        }
                    }
                }

                // Descriptions
                foreach (FMG.Entry entry in Smithbox.BankHandler.FMGBank.GetFmgEntriesByCategoryAndTextType(_activeFmgInfo.EntryCategory,
                             FmgEntryTextType.Description, false))
                {
                    if (entry.Text != null)
                    {
                        if (entry.Text.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase))
                        {
                            FMG.Entry search = _entryLabelCache.Find(e => e.ID == entry.ID && !matches.Contains(e));
                            if (search != null)
                            {
                                matches.Add(search);
                            }
                        }
                    }
                }

                // Summaries
                foreach (FMG.Entry entry in Smithbox.BankHandler.FMGBank.GetFmgEntriesByCategoryAndTextType(_activeFmgInfo.EntryCategory,
                             FmgEntryTextType.Summary, false))
                {
                    if (entry.Text != null)
                    {
                        if (entry.Text.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase))
                        {
                            FMG.Entry search = _entryLabelCache.Find(e => e.ID == entry.ID && !matches.Contains(e));
                            if (search != null)
                            {
                                matches.Add(search);
                            }
                        }
                    }
                }

                // Extra Text
                foreach (FMG.Entry entry in Smithbox.BankHandler.FMGBank.GetFmgEntriesByCategoryAndTextType(_activeFmgInfo.EntryCategory,
                             FmgEntryTextType.ExtraText, false))
                {
                    if (entry.Text != null)
                    {
                        if (entry.Text.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase))
                        {
                            FMG.Entry search = _entryLabelCache.Find(e => e.ID == entry.ID && !matches.Contains(e));
                            if (search != null)
                            {
                                matches.Add(search);
                            }
                        }
                    }
                }

                matches = matches.OrderBy(e => e.ID).ToList();

                _EntryLabelCacheFiltered = matches;
                _searchFilterCached = _searchFilter;
                doFocus = true;
            }
            else if (_entryLabelCache != _EntryLabelCacheFiltered && _searchFilter == "")
            {
                _EntryLabelCacheFiltered = _entryLabelCache;
            }
        }
    }

    private void CategoryListUI(FmgFileCategory uiType, bool doFocus)
    {
        List<FMGInfo> infos;
        if (_fmgSearchAllActive)
            infos = _filteredFmgInfo;
        else
            infos = Smithbox.BankHandler.FMGBank.FmgInfoBank.ToList();

        foreach (var info in infos)
        {
            if (info.PatchParent == null
                && info.FileCategory == uiType
                && info.EntryType is FmgEntryTextType.Title or FmgEntryTextType.TextBody)
            {
                string displayName;
                if (CFG.Current.FMG_ShowOriginalNames)
                {
                    displayName = info.FileName;
                }
                else
                {
                    if (!CFG.Current.FMG_NoGroupedFmgEntries)
                    {
                        displayName = info.Name.Replace("Title", "");
                    }
                    else
                    {
                        displayName = info.Name;
                    }

                    displayName = displayName.Replace("Modern_", "");
                }

                if (ImGui.Selectable($@" {displayName}", info == _activeFmgInfo))
                {
                    ClearTextEditorCache();
                    _activeFmgInfo = info;
                    if (_fmgSearchAllActive)
                    {
                        _searchFilter = _fmgSearchAllString;
                        _searchFilterCached = "";
                    }
                }

                if (doFocus && info == _activeFmgInfo)
                {
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    private void EditorGUI(bool doFocus)
    {
        var scale = Smithbox.GetUIScale();

        if (!Smithbox.BankHandler.FMGBank.IsLoaded)
        {
            if (Smithbox.ProjectHandler.CurrentProject.Config == null)
            {
                ImGui.Text("No project loaded. File -> New Project");
            }
            else if (Smithbox.BankHandler.FMGBank.IsLoading)
            {
                ImGui.Text("Loading...");
            }
            else
            {
                ImGui.Text("This Editor requires unpacked game files. Use UXM");
                // Ascii fatcat (IMPORTANT)
                ImGui.TextUnformatted(
                    "\nPPPPP5555PGGGGGGGBBBBBBGPYJ?????????JJYYYYYY5PPPGGPPPGPPPPPPPPPPPPGGGGGGGGGPPPPPPPPPPPPPPPPPPPPPPPPP\r\nPPPPP5555PGGGGGBBBBBBGGGPYJJ??????JJYY55555Y555PPPPGGGGGGGGGGGGGGGGGPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP\r\nPPPPP555PPGGGGGBBBBBBGGGP5JJ?????JJY55555555YYY555PPPPGGGGGGGPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP\r\nPPPPP555PPGGGGGBBBBBBGBGPYJ?????JJY555555555555555555PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPGPPP\r\nPP55555PPGGGGGGGBBBBBBGGPYJ????JJY555555555555555555555555PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP\r\n555Y55PPGGGGGGGGGGGBBGGG5Y?????JYY5555555P5555P555555555555555PPPPPPPPPPPPPPPPPPGGGGGGGGGPPPPPPPPPPP\r\n555555PPPPPPPPGGGGGBBBBGPJ???JJYY55555555PPPP5555555555555555555PPPPPPPPPPPPPPPPPPPPPPPPPPPPPGGGGGGG\r\nPPPPPPPPPPPGGGGGGGGGBBBBG5YJJJYYYY555555PPP55555555555555555555555PPPPPPPPPGGPPPPPGGGGPPPPPPPPPPPPPP\r\nPPPPPPPGGGGGGGGGGGGBBBBBGP555YYYYYYYYYY55555555555YYYYYYYYY55555555555PPPPPPPPPPPPPPPPPPPPPPPGPPPPPP\r\nPPPPPGGGGGPPPPPPPPGGGGGGPPPP5555YYYYYYYYYY555555YYYYYYYYYY55555YYYYYY5555555555555555555555555555PPP\r\nPPPPPPPPPPPP555555PPPPPPPPPP5555555555YYYYYYYYYYYYYYYYYYYY55YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY\r\n55PPPPPP555555555555555PP55555555555555555YYYYYYYYYYYYYYYYYYYYYYYJJJJJ??JJJJJJ?JJJJJJJJJJJJJJJJJJJJJ\r\nPPPPPPP55555555PP5555555P55555555555555555YYYYYYYYYYYYYYYYJJJJJJJJJ??????????77?????????????????????\r\nPPPPPP5PPPPPPPPPP555555555555555555555555YYYYYYYYYYYYYYYJJ??????????7777777777777?????????7?????????\r\nPPPPPPPPPPPPPPPP55555555555555555555YYYYYYYYYYYYYYYYJJJ???7777777777??7777777777777777????77777?????\r\nGGGGBBBBBBGGGGGGGGGPPPPPPPPP5555555YYYYYYYJJJJJJJJ??????77777777?77??????777777777777777?777777?????\r\nGGGGGGGBBBBBBBBBBBBBBGGGGGPPPPPP5555YYYYJJJJJJ????????7777777?????????????????????7777777777777?????\r\nPPPPPPPPPGGGGGGGGGGBBBBBBBGGPPPPPP555YYYYJJJJJ?????????77777????JJYYYJJJJJ?????????????????777777777\r\nPPP5PPPPPPPPPPPPPPPPPPPGGBBGGGPPPPPP55YYYYYYJJJ??7777????777??JJYY5555YYYYYYYYYYYYYJJJJYJJJJ????????\r\nPPPPPPPPPPPPP5PPPPPP555PPGGGGGPPPPPPP5555YYYJJ???7777?77777???JY555PPPPPPPPPGGGGGGGGGPPPPPPPP555YYYY\r\nPPPPPPPPPPPPP55PPP5555555PPPPPPPPPPPPP555YYJJ????????????777??J5PGGGGGGGGGGGGGGGGBBBBBBBBBBBBBBBBBBB\r\nPPPPPPPPPPPP55555555555PPPPPPPPPPPPPP55YYYYJJJ?????????????77?Y5P555YJJ??JJJJJ??JJJJJJJYYY5555555555\r\nPPP55PPPP5555555555555PPPPPPPPPPPGGPP5YYYYYJJJ????????????777??JJJJ???777777777777777777????????????\r\n555555P55555P55555555PPPPPPPPPPPGGGP55YYYYYJJJJJJJ??JJJ?????????????77777777777777777777777777777777\r\n5555555555PPPP55555PPPPPPPPPPPPPGGPP5YYYYYYJJJJJJJJJJJJJ????????????77777777777777777777777777777777\r\nP5555555555P5555555PPPPPPPPGGGGGGGPP5YYYYYYYYJJJJJYYYYYJJJ??????????77777777777777777777777777777777\r\n555P5555555555555555PPPPPPPGGGGPPPP55YYYYYYYYYYYYYY555YYJJJ????????777777777777777777777777777777777\r\n55555555555555PPP555PPPPPPGGGGGGPPP55YYYYYYYYYYYYYYY55YYJJJ???????7777777777777777777777777777777777\r\nPPPP55PP5555PPPPPPPPPPPPPPPGGGGGGPPP555YYYYYYYYYYY55PP5YJJ???????????7777777777777777777777777777777\r\nPPP555555PPPP55PPPPPPPPPPPGGGGBGGPPPPP555555555PPPGGPP5YJJJJ??????????777777777777777777777777777777\r\nPPPPPPPPPPPPPPPPPPPPPPPPPPGGGGGGGPPPPPPPPPPPPPPPGGGGP55YYYYJJ?????????777777777777777777777777777777\r\nPPPPPPPGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGPPGPPP55YYYYYJJJJ?????????77777777777777777777777777\r\nPPPPPPPPGGGBBBBBBBBBGGGBBBBBBBBBBBBBBBBBBBBGGBBGGGGGGPPP5555YYYYYYYJJJJJJ??????777777777777777777777\r\nPPPPPPPPPPPPPGGBBBGGPPGGBBBBBBBBBBBBBBBBBBBBBBBBBBBGGGGGGGPPPPPPPPP55555YYYYJJJ????77777777777777777\r\nPPPPPPPPPPPPPPPPPPPPPPPPGBBBBBBBBBBBBBBBBBBBBBB##BGGPPGGBBBBGGGGGGGGPPPPPPPP5555YYJJ??????7777777777\r\nPPPPPPPPPPPPPPPPPPP55555PPGGGBBBBBBBBBBBBBBBBBBBGGPPPPGGGGGPPPPPPPPPPP5555555555555YYJJJ????????????\r\nPPPPPPPPPPPPPPPP5PPPPP555PPPPPPPPPPPPPPPPGPPPPPPP555555555555555YYYJJJJJJJJJJJJJJYYYYJJJJJJJJJ??????\r\nPPPPPPPPPPPPPPP555PPPP555555555555555555555555YYYYYYYYYYYJJJJJJJ???777777777777???JJJJJJJJJJJJJJ????\r\nPPP55PPPPP555PP5555555555555555YYYYYYYYYYYYYYJJJJJJJJJJ??????777777777777777777???JJJJJJJJJJJJJJJ???\r\nPPPPPPPP5555555555555555YYYYYYYJJJJJJ????????????????????????????777777777777???JJJJJJJJJJJJJJJJJ???\r\nPPPP5P555555555555555555555YYYYYJJJJJJJJ??????????????????????????????7?????JJJJJJJJJJJJJJJJJJJJJJJJ\r\nPPP55PP55555555555555555555YYYYYYYJJJJJJJJJJJJJJJJ?????????JJJJJJJJ???JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ\r\n555555555555555555555555YYYYYYYYYYYYJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ\r\n55555555555555555555555555555555YYYYYYYYYYYYYYYYYYYYYYYYYYYYYJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ\r\n5555555555555555555555555555555555555555555555555555555YYYYYYYJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ?\r\n55555555555555555555555555555555555555555555555555555555YYYYYYYYYYYYJJJJJJJJJJJJJJJJJJJJJJJJJJJ?????\r\n555555555555555555555555555555555555555555555555555555YYYYYYYYYYYYYYYJJJJJJJJJJJJJJJJJJJJJ??????????");
            }

            return;
        }

        if (CFG.Current.Interface_TextEditor_TextCategories)
        {
            ImGui.Begin("Text Categories");
            ImGui.Indent();
            ImGui.InputText("##SearchAllFmgsInput", ref _fmgSearchAllString, 255);
            ImGui.Unindent();
            ImGui.SameLine();

            if (_fmgSearchAllString == "")
            {
                _fmgSearchAllActive = false;
                ImGui.BeginDisabled();
            }
            if (ImGui.Button("Search All FMGs##FmgSearchAll"))
            {
                _fmgSearchAllActive = true;
                _filteredFmgInfo.Clear();
                foreach (var info in Smithbox.BankHandler.FMGBank.SortedFmgInfoBank)
                {
                    if (info.PatchParent == null)
                    {
                        foreach (var entry in info.GetPatchedEntries(false))
                        {
                            if ((entry.Text != null && entry.Text.Contains(_fmgSearchAllString, StringComparison.CurrentCultureIgnoreCase))
                                || entry.ID.ToString().Contains(_fmgSearchAllString))
                            {
                                if (info.EntryType is not FmgEntryTextType.Title and not FmgEntryTextType.TextBody)
                                {
                                    try
                                    {
                                        _filteredFmgInfo.Add(info.GetTitleFmgInfo());
                                    }
                                    catch (Exception e)
                                    {
                                        //This fmginfo lacks a title
                                        _filteredFmgInfo.Add(info);
                                    }
                                }
                                else
                                {
                                    _filteredFmgInfo.Add(info);
                                }
                                break;
                            }
                        }
                    }
                }
                _filteredFmgInfo = _filteredFmgInfo.Distinct().ToList();
            }
            if (_fmgSearchAllString == "")
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Reset##FmgSearchAll"))
            {
                _fmgSearchAllActive = false;
                _fmgSearchAllString = "";
                _filteredFmgInfo.Clear();
            }
            ImGui.Separator();

            foreach (FmgFileCategory v in Smithbox.BankHandler.FMGBank.currentFmgInfoBanks)
            {
                ImGui.Separator();
                ImGui.Text($"  {v} Text");
                ImGui.Separator();
                // Categories
                CategoryListUI(v, doFocus);
                ImGui.Spacing();
            }

            if (_activeFmgInfo != null)
            {
                _entryLabelCache = UICache.GetCached(this, "FMGEntryCache", () =>
                {
                    return _activeFmgInfo.GetPatchedEntries();
                });
            }

            // Needed to ensure EntryGroup is still valid after undo/redo actions while also maintaining highlight-duped-row functionality.
            // It's a bit dumb and probably overthinking things.
            _clearEntryGroup = UICache.GetCached(this, "FMGClearEntryGroup", () =>
            {
                if (_clearEntryGroup)
                {
                    return false;
                }

                return true;
            });
            if (_clearEntryGroup)
            {
                if (!doFocus)
                {
                    _activeEntryGroup = null;
                }

                UICache.RemoveCache(this, "FMGClearEntryGroup");
            }

            ImGui.End();

            ImGui.Begin("Text Entries");
            if (ImGui.Button("Clear Text"))
            {
                _searchFilter = "";
            }

            ImGui.SameLine();

            // Search
            if (InputTracker.GetKeyDown(KeyBindings.Current.TextFMG_Search))
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.InputText($"Search <{KeyBindings.Current.TextFMG_Search.HintText}>", ref _searchFilter, 255);

            FMGSearchLogic(ref doFocus);

            ImGui.BeginChild("Text Entry List");
            if (_activeFmgInfo == null)
            {
                ImGui.Text("Select a category to see entries");
            }
            else if (_EntryLabelCacheFiltered == null)
            {
                ImGui.Text("No entries match search filter");
            }
            else
            {
                if (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down))
                {
                    _arrowKeyPressed = true;
                }

                for (var i = 0; i < _EntryLabelCacheFiltered.Count; i++)
                {
                    FMG.Entry r = _EntryLabelCacheFiltered[i];
                    // Entries
                    var text = r.Text == null
                        ? "%null%"
                        : r.Text.Replace("\n", "\n".PadRight(r.ID.ToString().Length + 2));
                    var label = $@"{r.ID} {text}";
                    label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth());
                    if (ImGui.Selectable(label, _activeIDCache == r.ID))
                    {
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                    }
                    else if (_activeIDCache == r.ID && _activeEntryGroup == null)
                    {
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                        _searchFilterCached = "";
                    }

                    if (_arrowKeyPressed && ImGui.IsItemFocused()
                                         && _activeEntryGroup?.ID != r.ID)
                    {
                        // Up/Down arrow key selection
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                        _arrowKeyPressed = false;
                    }

                    if (doFocus && _activeEntryGroup?.ID == r.ID)
                    {
                        ImGui.SetScrollHereY();
                        doFocus = false;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.Selectable("Delete Entry"))
                        {
                            _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                            TextAction_Delete.DeleteSelectedEntry();
                        }

                        ImGui.Separator();

                        if (ImGui.Selectable("Duplicate Entry"))
                        {
                            _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                            TextAction_Duplicate.DuplicateSelectedEntry();
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();
        }

        if(CFG.Current.Interface_TextEditor_TextEntry)
        {
            ImGui.Begin("Text");
            if (_activeEntryGroup == null)
            {
                ImGui.Text("Select an item to edit text");
            }
            else
            {
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, 100 * scale);
                ImGui.Text("ID");
                ImGui.NextColumn();

                _propEditor.PropIDFMG(_activeEntryGroup, _entryLabelCache);
                _activeIDCache = _activeEntryGroup.ID;

                ImGui.NextColumn();

                _propEditor.PropEditorFMGBegin();
                if (_activeEntryGroup.TextBody != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.TextBody, "Text", 200.0f);
                }

                if (_activeEntryGroup.Title != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Title, "Title", 20.0f);
                }

                if (_activeEntryGroup.Summary != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Summary, "Summary", 80.0f);
                }

                if (_activeEntryGroup.Description != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Description, "Description", 200.0f);
                }

                if (_activeEntryGroup.ExtraText != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.ExtraText, "Extra", 80.0f);
                }

                _propEditor.PropEditorFMGEnd();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_TextEditor_Toolbar)
        {
            _textToolbar_ActionList.OnGui();
            _textToolbar_Configuration.OnGui();
        }
    }

    private void ChangeLanguage(string path)
    {
        Smithbox.ProjectHandler.CurrentProject.Config.LastFmgLanguageUsed = path;
        Smithbox.ProjectHandler.SaveCurrentProject();

        _fmgSearchAllString = "";
        _filteredFmgInfo.Clear();
        ClearTextEditorCache();
        ResetActionManager();

        Smithbox.BankHandler.FMGBank.LanguageFolder = path;
        Smithbox.BankHandler.FMGBank.LoadFMGs(path);
    }
}
