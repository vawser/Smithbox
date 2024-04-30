using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor.Toolbar;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Editors.ParticleEditor.Toolbar;
using StudioCore.Editors.TalkEditor;
using StudioCore.Interface;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.ParticleEditor;

public class ParticleEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public PropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    // Files
    private ParticleFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    // Particles
    private FxrInfo _selectedParticleInfo;
    private int _selectedParticleKey;

    private string _particleSearchInput = "";
    private string _particleSearchInputCache = "";

    private string _particleDataSearchInput = "";
    private string _particleDataSearchInputCache = "";

    private bool[] displayTruth;

    public ParticleToolbar _particleToolbar;
    public ParticleToolbar_ActionList _particleToolbar_ActionList;
    public ParticleToolbar_Configuration _particleToolbar_Configuration;

    public ParticleEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);

        _particleToolbar = new ParticleToolbar(EditorActionManager);
        _particleToolbar_ActionList = new ParticleToolbar_ActionList();
        _particleToolbar_Configuration = new ParticleToolbar_Configuration();
    }

    public string EditorName => "Particle Editor##ParticleEditor";
    public string CommandEndpoint => "particle";
    public string SaveType => "Particle";

    public void Init()
    {

    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo", KeyBindings.Current.Core_Undo.HintText, false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false, EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_ParticleEditor_Files = !CFG.Current.Interface_ParticleEditor_Files;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ParticleEditor_Files);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Particles"))
            {
                CFG.Current.Interface_ParticleEditor_Particles = !CFG.Current.Interface_ParticleEditor_Particles;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ParticleEditor_Particles);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Data"))
            {
                CFG.Current.Interface_ParticleEditor_Data = !CFG.Current.Interface_ParticleEditor_Data;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ParticleEditor_Data);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Toolbar"))
            {
                CFG.Current.Interface_ParticleEditor_Toolbar = !CFG.Current.Interface_ParticleEditor_Toolbar;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ParticleEditor_Toolbar);

            ImGui.EndMenu();
        }
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

        var dsid = ImGui.GetID("DockSpace_ParticleEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Begin("Editor##InvalidParticleEditor");

            ImGui.Text($"This editor does not support {Project.Type}.");

            ImGui.End();
        }
        else
        {

            if (!ParticleBank.IsLoaded)
            {
                if (!CFG.Current.AutoLoadBank_Particle)
                {
                    if (ImGui.Button("Load Particle Editor"))
                    {
                        ParticleBank.LoadParticles();
                    }
                }
            }

            ParticleShortcuts();

            if (ParticleBank.IsLoaded)
            {
                if (CFG.Current.Interface_ParticleEditor_Files)
                {
                    ParticleFileView();
                }
                if (CFG.Current.Interface_ParticleEditor_Particles)
                {
                    ParticleListView();
                }
                if (CFG.Current.Interface_ParticleEditor_Data)
                {
                    ParticleDataView();
                }
            }

            if(CFG.Current.Interface_ParticleEditor_Toolbar)
            {
                _particleToolbar_ActionList.OnGui();
                _particleToolbar_Configuration.OnGui();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    //*****************************
    // Shortcuts
    //*****************************
    public void ParticleShortcuts()
    {
        // Keyboard shortcuts
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
        {
            EditorActionManager.RedoAction();
        }
    }

    //*****************************
    // Windows
    //*****************************
    // Files
    public void ParticleFileView()
    {
        // File List
        ImGui.Begin("Files##ParticleFileList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fileSearchInput != _fileSearchInputCache)
        {
            _fileSearchInputCache = _fileSearchInput;
        }

        foreach (var info in ParticleBank.FileBank)
        {
            if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
            {
                // Ignore the resource ffxbnd files
                if (info.FxrFiles.Count > 0)
                {
                    ImGui.BeginGroup();
                    if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
                    {
                        _selectedParticleKey = -1; // Clear particle key if file is changed

                        _selectedBinderKey = info.Name;
                        _selectedFileInfo = info;
                        _selectedBinder = info.Binder;
                    }
                    ImGui.EndGroup();
                }
            }

            ParticleFileContextMenu(info);
        }

        ImGui.End();
    }

    // Particles
    public void ParticleListView()
    {
        ImGui.Begin("Particles##ParticleList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _particleSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_particleSearchInput != _particleSearchInputCache)
        {
            _particleSearchInputCache = _particleSearchInput;
        }

        if (_selectedFileInfo != null && _selectedFileInfo.FxrFiles != null)
        {
            ImGui.Text($"Particles");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.FxrFiles.Count; i++)
            {
                var name = Path.GetFileNameWithoutExtension(_selectedFileInfo.FxrFiles[i]);

                if (SearchFilters.IsEditorSearchMatch(_particleSearchInput, name, " "))
                {
                    if (ImGui.Selectable($@" {name}", i == _selectedParticleKey))
                    {
                        _selectedParticleKey = i;

                        ParticleBank.LoadParticle(name, _selectedFileInfo);

                        if (LoadedFXR.ContainsKey(name))
                        {
                            _selectedParticleInfo = LoadedFXR[name];
                        }
                        else
                        {
                            TaskLogs.AddLog($"LoadedFXR does not contain FxrInfo for {name}", LogLevel.Warning);
                        }
                    }

                    DisplayAlias(name);
                }

                ParticleListContextMenu(i);
            }
        }

        ImGui.End();
    }

    // Data
    public void ParticleDataView()
    {
        ImGui.Begin("Particle Data##ParticleDataView");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _particleDataSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_particleDataSearchInput != _particleDataSearchInputCache)
        {
            _particleDataSearchInputCache = _particleDataSearchInput;
        }

        if(_selectedParticleInfo != null && _selectedParticleKey != -1)
        {
            FXR3 particle = _selectedParticleInfo.Content;

            _propEditor.OnGui(particle);
        }

        ImGui.End();
    }

    //*****************************
    // Context Menus
    //*****************************
    public void ParticleFileContextMenu(ParticleFileInfo info)
    {
        if (info.Name == _selectedBinderKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##FileListContext"))
            {

                ImGui.EndPopup();
            }
        }
    }

    public void ParticleListContextMenu(int index)
    {
        if (index == _selectedParticleKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##ParticleListContext"))
            {
                
                ImGui.EndPopup();
            }
        }
    }

    //*****************************
    // Editor
    //*****************************
    public void OnProjectChanged()
    {
        // Only support FXR3 for now
        if(Project.Type is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            if (CFG.Current.AutoLoadBank_Particle)
                ParticleBank.LoadParticles();
        }

        ResetActionManager();
    }

    public void Save()
    {
        if (ParticleBank.IsLoaded)
            ParticleBank.SaveParticle(_selectedParticleInfo);
    }

    public void SaveAll()
    {
        if (ParticleBank.IsLoaded)
            ParticleBank.SaveParticles();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }

    //*****************************
    // Utility
    //*****************************
    private void DisplayAlias(string name)
    {
        if (CFG.Current.Interface_Display_Alias_for_Particles)
        {
            if (ParticleAliasBank.Bank.AliasNames != null)
            {
                var prettyName = "";

                var entries = ParticleAliasBank.Bank.AliasNames.GetEntries("Particles");
                foreach (var entry in entries)
                {
                    if (name == entry.id)
                    {
                        prettyName = entry.name;
                        break;
                    }
                }

                if (prettyName != "")
                {
                    ImGui.SameLine();
                    ImGui.PushTextWrapPos();

                    ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"<{prettyName}>");

                    ImGui.PopTextWrapPos();
                }
            }
        }
    }
}
