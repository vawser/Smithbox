using DotNext.Collections.Generic;
using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.DRB;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    private readonly PropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    private AnimationFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private TAE _selectedTimeAct;
    private int _selectedTimeActKey;

    public TimeActEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "TimeAct Editor##TimeActEditor";
    public string CommandEndpoint => "timeact";
    public string SaveType => "TAE";

    public void Init()
    {
        ShowSaveOption = false;
    }

    public void DrawEditorMenu()
    {
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

        var dsid = ImGui.GetID("DockSpace_TimeActEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S or ProjectType.DS2)
        {
            ImGui.Begin("Editor##InvalidTaeEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!AnimationBank.IsLoaded)
            {
                TaskManager.Run(new TaskManager.LiveTask($"Setup Time Act Editor", TaskManager.RequeueType.None, false,
                () =>
                {
                    AnimationBank.LoadTimeActs();
                    HavokFileBank.LoadAllHavokFiles();
                }));
            }

            if (!TaskManager.AnyActiveTasks() && AnimationBank.IsLoaded && HavokFileBank.IsLoaded)
            {
                TimeActContainerFileView();
                TimeActInternalFileView();
                TimeActAnimationView();
                TimeActAnimEventGraphView();
            }
            else
            {
                ImGui.Begin("Editor##LoadingTaeEditor");

                ImGui.Text($"This editor is still loading.");

                ImGui.End();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private HavokContainerInfo LoadedHavokContainer;

    public void TimeActContainerFileView()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in AnimationBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedTimeActKey = -1; // Clear tae key if file is changed
                _selectedTimeActAnimationKey = -1;
                _selectedTimeActEventKey = -1;
                _selectedTimeAct = null;

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;

                foreach (var entry in HavokFileBank.BehaviorContainerBank)
                {
                    if(entry.Filename == info.Name)
                    {
                        entry.LoadBinder();

                        foreach (var file in entry.InternalFileList)
                        {
                            var name = file.Split("export")[1];
                            if (name.Contains("behaviors"))
                            {
                                LoadedHavokContainer = entry;
                                LoadedHavokContainer.LoadFile(file.ToLower());
                                LoadedHavokContainer.ReadHavokObjects(file.ToLower());
                            }
                        }
                    }
                }
            }
            DisplaySelectableAlias(info.Name, Smithbox.AliasCacheHandler.AliasCache.Characters);
        }

        ImGui.End();
    }

    public void TimeActInternalFileView()
    {
        ImGui.Begin("Time Acts##TimeActList");

        if (_selectedFileInfo != null)
        {
            ImGui.Text($"TimeActs");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.TimeActFiles.Count; i++)
            {
                TAE entry = _selectedFileInfo.TimeActFiles[i];

                if (ImGui.Selectable($@" a{(entry.ID - 2000)}", i == _selectedTimeActKey))
                {
                    _selectedTimeActAnimationKey = -1;
                    _selectedTimeActEventKey = -1;
                    _selectedTimeActKey = i;
                    _selectedTimeAct = entry;
                    ApplyTemplate();
                }
            }
        }

        ImGui.End();
    }

    public void ApplyTemplate()
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS1:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.DS1"]);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.SOTFS"]);
                break;
            case ProjectType.DS3:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.DS3"]);
                break;
            case ProjectType.BB:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.BB"]);
                break;
            case ProjectType.SDT:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.SDT"]);
                break;
            case ProjectType.ER:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.ER"]);
                break;
            case ProjectType.AC6:
                _selectedTimeAct.ApplyTemplate(AnimationBank.TAETemplates["TAE.Template.AC6"]);
                break;
        }
    }

    private long _selectedTimeActAnimationKey = -1;

    public void TimeActAnimationView()
    {
        ImGui.Begin("Animations##TimeActAnimationList");

        if (_selectedTimeAct != null)
        {
            foreach(var anim in _selectedTimeAct.Animations)
            {
                if (ImGui.Selectable($@" {anim.ID}", anim.ID == _selectedTimeActAnimationKey))
                {
                    _selectedTimeActAnimationKey = anim.ID;
                    _selectedTimeActEventKey = -1;
                }
                var list = GetAnimationAliasList(anim.ID);
                if (list.Count > 0)
                {
                    AliasUtils.DisplayAlias(list[0]);
                    AliasUtils.AliasTooltip(list, "Generators that use this animation:");
                }
            }
        }

        ImGui.End();
    }

    private Dictionary<long, List<string>> AnimationAliasCache = new();

    private List<string> GetAnimationAliasList(long id)
    {
        List<string> aliasList = new();

        if(AnimationAliasCache.ContainsKey(id))
        {
            aliasList = AnimationAliasCache[id];
        }
        else
        {
            // TODO: this is a bit slow with c0000, leading to a one-time delay of 15 seconds
            // Would be better to pre-cache this with a Task, and just not display aliases until the Task finishes
            List<CustomManualSelectorGenerator> cmsgs = LoadedHavokContainer.LoadedObjects
            .Where(x => x is CustomManualSelectorGenerator cmsg && cmsg.m_generators
            .All(y => y is hkbClipGenerator))
            .Cast<CustomManualSelectorGenerator>().ToList();

            Dictionary<hkbClipGenerator, List<CustomManualSelectorGenerator>> clipParents = LoadedHavokContainer.LoadedObjects
                .Where(x => x is hkbClipGenerator)
                .Cast<hkbClipGenerator>()
                .Distinct()
                .ToDictionary(x => x, _ => new List<CustomManualSelectorGenerator>());

            var _cmsgsByAnimId = cmsgs.GroupBy(x => x.m_animId).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var entry in _cmsgsByAnimId)
            {
                if(entry.Key == id)
                {
                    foreach(var val in entry.Value)
                    {
                        aliasList.Add($"{val.m_name}");
                    }
                }
            }

            AnimationAliasCache.Add(id, aliasList);
        }

        return aliasList;
    }

    private void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
    {
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            if (CFG.Current.MapEditor_AssetBrowser_ShowAliases)
            {
                var aliasName = referenceDict[lowerName].name;

                AliasUtils.DisplayAlias(aliasName);
            }

            // Tags
            if (CFG.Current.MapEditor_AssetBrowser_ShowTags)
            {
                var tagString = string.Join(" ", referenceDict[lowerName].tags);
                AliasUtils.DisplayTagAlias(tagString);
            }
        }
    }

    private long _selectedTimeActEventKey = -1;

    public void TimeActAnimEventGraphView()
    {
        ImGui.Begin("Event Graph##TimeActAnimEventGraph");

        if (_selectedTimeActAnimationKey != -1)
        {
            var selectedAnim = _selectedTimeAct.Animations.Where(e => e.ID == _selectedTimeActAnimationKey).FirstOrDefault();

            for(int i = 0; i < selectedAnim.Events.Count; i++)
            {
                var evt = selectedAnim.Events[i];

                if (ImGui.Selectable($@" {evt.TypeName}", i == _selectedTimeActEventKey))
                {
                    _selectedTimeActEventKey = i;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        if (CFG.Current.AutoLoadBank_TimeAct)
            AnimationBank.LoadTimeActs();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeAct(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeActs();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
