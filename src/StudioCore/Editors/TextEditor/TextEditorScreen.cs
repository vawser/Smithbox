﻿using ImGuiNET;
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
using StudioCore.Utilities;
using StudioCore.Locators;
using StudioCore.Core;
using StudioCore.Editors.TextEditor;
using System.IO;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.Editors.TextEditor.Actions;
using StudioCore.Editors.TextEditor.Tools;

namespace StudioCore.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    private readonly PropertyEditor _propEditor;

    public FMGEntryGroup _activeEntryGroup;
    public FMGInfo _activeFmgInfo;
    public int _activeIDCache = -1;
    private bool _arrowKeyPressed;

    private bool _clearEntryGroup;

    public List<FMG.Entry> _entryLabelCache;
    public List<FMG.Entry> _EntryLabelCacheFiltered;

    private string _searchFilter = "";
    public string _searchFilterCached = "";
    private string _fmgSearchAllString = "";
    private bool _fmgSearchAllActive = false;
    private List<FMGInfo> _filteredFmgInfo = new();
    public ActionManager EditorActionManager = new();

    public FmgEntrySelection SelectionHandler;

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;

    public ActionSubMenu ActionSubMenu;

    public TextEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        SelectionHandler = new FmgEntrySelection();

        _propEditor = new PropertyEditor(EditorActionManager);

        ToolWindow = new ToolWindow(this);
        ToolSubMenu = new ToolSubMenu(this);
        ActionSubMenu = new ActionSubMenu(this);
    }

    public string EditorName => "Text Editor";
    public string Discription => "文本编辑器 Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";

    public void Init()
    {
        ShowSaveOption = true;
    }

    public void DrawEditorMenu()
    {
        var currentFmgBank = Smithbox.BankHandler.FMGBank;

        if (ImGui.BeginMenu("编辑 Edit", currentFmgBank.IsLoaded))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"撤销 Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("撤销全部 Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("返回 Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ActionSubMenu.DisplayMenu();
        ToolSubMenu.DisplayMenu();

        if (ImGui.BeginMenu("数据 Data", Smithbox.BankHandler.FMGBank.IsLoaded))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
            if (ImGui.BeginMenu("导入文本 Import Text Entries"))
            {
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
                if (ImGui.MenuItem("合并 Merge Entries"))
                {
                    if (FmgExporter.ImportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], true))
                    {
                        ClearTextEditorCache();
                        ResetActionManager();
                    }
                }
                ImguiUtils.ShowHoverTooltip("导入保留原有的 Import: text will be merged with currently loaded text");

                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
                if (ImGui.MenuItem("替换 Replace Entries"))
                {
                    if (FmgExporter.ImportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], false))
                    {
                        ClearTextEditorCache();
                        ResetActionManager();
                    }
                }
                ImguiUtils.ShowHoverTooltip("导入并替换原有的 Import: text replaces currently loaded text entirely");

                ImGui.EndMenu();
            }

            // Export
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Database}");
            if (ImGui.BeginMenu("导出文本 Export Text Entries"))
            {
                if (ImGui.MenuItem("导出模组 Export Modded Entries"))
                {
                    FmgExporter.ExportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], true);
                }
                ImguiUtils.ShowHoverTooltip("仅导出经过修改的文本（与原版不同） Export: only modded text (different than vanilla) will be exported");

                if (ImGui.MenuItem("导出所有条目 Export All Entries"))
                {
                    FmgExporter.ExportFmgTxt(currentFmgBank.fmgLangs[currentFmgBank.LanguageFolder], false);
                }
                ImguiUtils.ShowHoverTooltip("导出：所有文本将被导出 Export: all text will be exported");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }


        if (ImGui.BeginMenu("视图 View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("类别 Text Categories"))
            {
                CFG.Current.Interface_TextEditor_TextCategories = !CFG.Current.Interface_TextEditor_TextCategories;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextCategories);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("入口 Text Entry"))
            {
                CFG.Current.Interface_TextEditor_TextEntry = !CFG.Current.Interface_TextEditor_TextEntry;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_TextEntry);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("配置 Tool Configuration"))
            {
                CFG.Current.Interface_TextEditor_ToolConfigurationWindow = !CFG.Current.Interface_TextEditor_ToolConfigurationWindow;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextEditor_ToolConfigurationWindow);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("语言 Text Language", !currentFmgBank.IsLoading))
        {
            Dictionary<string, string> folders = TextLocator.GetMsgLanguages();
            if (folders.Count == 0)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "无此语言目录 Cannot find language folders.");
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

        // Only relevant for games that use the dlc FMG system
        if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.ER)
        {
            if (ImGui.BeginMenu("文本输出 Text Output"))
            {
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
                if (ImGui.MenuItem("默认 Vanilla"))
                {
                    CurrentTargetOutputMode = TargetOutputMode.Vanilla;
                }
                ImguiUtils.ShowActiveStatus(CurrentTargetOutputMode == TargetOutputMode.Vanilla);

                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
                if (ImGui.MenuItem("DLC1"))
                {
                    CurrentTargetOutputMode = TargetOutputMode.DLC1;
                }
                ImguiUtils.ShowActiveStatus(CurrentTargetOutputMode == TargetOutputMode.DLC1);

                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
                if (ImGui.MenuItem("DLC2"))
                {
                    CurrentTargetOutputMode = TargetOutputMode.DLC2;
                }
                ImguiUtils.ShowActiveStatus(CurrentTargetOutputMode == TargetOutputMode.DLC2);

                ImGui.EndMenu();
            }
            ImguiUtils.ShowHoverTooltip("允许您在保存时切换目标 FMG 输出。默认情况下，对于 DS3 dlc2 和 ER，这个目标是 dlc02\nAllows you to switch the target FMG output on save. By default for DS3 dlc2 and ER this is dlc02.");
        }
    }

    public TargetOutputMode CurrentTargetOutputMode = TargetOutputMode.DLC2;

    public enum TargetOutputMode
    {
        Vanilla,
        DLC1,
        DLC2
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

            ImGui.Text($"不支持 This editor does not support {Smithbox.ProjectType}.");

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

                ActionSubMenu.Shortcuts();
                ToolSubMenu.Shortcuts();
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
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
        }

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
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        Smithbox.BankHandler.FMGBank.SaveFMGs();
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
            var proceed = false;

            if(!CFG.Current.FMG_NoFmgPatching && info.PatchParent == null)
            {
                proceed = true;
            }
            else if(CFG.Current.FMG_NoFmgPatching)
            {
                proceed = true;
            }

            if (proceed
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
                ImGui.Text("未加载项目 No project loaded. File -> New Project");
            }
            else if (Smithbox.BankHandler.FMGBank.IsLoading)
            {
                ImGui.Text("加载中 Loading...");
            }
            else
            {
                ImGui.Text("需要UXM解包后才能使用本程序\nThis Editor requires unpacked game files. Use UXM");
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
            if (ImGui.Button("查找 Search All FMGs##FmgSearchAll"))
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
            if (ImGui.Button("重置 Reset##FmgSearchAll"))
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
            if (ImGui.Button("清空 Clear Text"))
            {
                _searchFilter = "";
            }

            ImGui.SameLine();

            // Search
            if (InputTracker.GetKeyDown(KeyBindings.Current.TextFMG_Search))
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.InputText($"查找 Search <{KeyBindings.Current.TextFMG_Search.HintText}>", ref _searchFilter, 255);

            FMGSearchLogic(ref doFocus);

            ImGui.BeginChild("文本列 Text Entry List");
            if (_activeFmgInfo == null)
            {
                ImGui.Text("选择一个类别以查看条目 Select a category to see entries");
            }
            else if (_EntryLabelCacheFiltered == null)
            {
                ImGui.Text("没有条目匹配搜索过滤器 No entries match search filter");
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

                    if (ImGui.Selectable(label, ( _activeIDCache == r.ID || SelectionHandler.IsSelected(r.ID))))
                    {
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                        SelectionHandler.HandleSelection(r.ID);
                    }
                    else if (_activeIDCache == r.ID && _activeEntryGroup == null)
                    {
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                        _searchFilterCached = "";
                    }

                    if (_arrowKeyPressed && ImGui.IsItemFocused()
                                         && ( _activeEntryGroup?.ID != r.ID && !SelectionHandler.IsSelected(r.ID)))
                    {
                        // Up/Down arrow key selection
                        _activeEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, _activeFmgInfo);
                        SelectionHandler.HandleSelection(r.ID);
                        _arrowKeyPressed = false;
                    }

                    if (doFocus && _activeEntryGroup?.ID == r.ID)
                    {
                        ImGui.SetScrollHereY();
                        doFocus = false;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.Selectable("复刻 Duplicate Entries"))
                        {
                            ActionSubMenu.Handler.DuplicateHandler();
                        }
                        if (ImGui.Selectable("删除 Delete Entries"))
                        {
                            ActionSubMenu.Handler.DeleteHandler();
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
                ImGui.Text("等待选中任意内容编辑\nSelect an item to edit text");
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
                    _propEditor.PropEditorFMG(_activeEntryGroup.TextBody, "文本 Text", 200.0f);
                }

                if (_activeEntryGroup.Title != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Title, "标题 Title", 20.0f);
                }

                if (_activeEntryGroup.Summary != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Summary, "概述 Summary", 80.0f);
                }

                if (_activeEntryGroup.Description != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.Description, "描述 Description", 200.0f);
                }

                if (_activeEntryGroup.ExtraText != null)
                {
                    _propEditor.PropEditorFMG(_activeEntryGroup.ExtraText, "其他 Extra", 80.0f);
                }

                _propEditor.PropEditorFMGEnd();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_ModelEditor_ToolConfigurationWindow)
        {
            ToolWindow.OnGui();
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
        Smithbox.BankHandler.FMGBank.LoadFMGs();
    }
}
