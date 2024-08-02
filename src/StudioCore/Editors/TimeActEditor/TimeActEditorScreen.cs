using DotNext;
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

    public ActionManager EditorActionManager = new();

    public TimeActSelectionHandler SelectionHandler;

    public TimeActEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        SelectionHandler = new TimeActSelectionHandler(EditorActionManager, this);
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

        if (Smithbox.ProjectType is not ProjectType.ER)
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
                }));
            }

            if (!TaskManager.AnyActiveTasks() && AnimationBank.IsLoaded)
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

    public void TimeActContainerFileView()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");

        ImGui.InputText($"Search##fileContainerFilter", ref TimeActFilters._fileContainerFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("ContainerList");
        for (int i = 0; i < AnimationBank.FileBank.Count; i++)
        {
            var info = AnimationBank.FileBank.ElementAt(i).Key;
            var binder = AnimationBank.FileBank.ElementAt(i).Value;

            if (TimeActFilters.FileContainerFilter(info))
            {
                var isSelected = false;
                if(i == SelectionHandler.ContainerIndex)
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@" {info.Name}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.FileContainerChange(info, binder, i);
                }
                TimeActUtils.DisplayTimeActFileAlias(info.Name);

                SelectionHandler.ContainerContextMenu.ContainerMenu(isSelected);
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }

    public void TimeActInternalFileView()
    {
        ImGui.Begin("Time Acts##TimeActList");

        if(!SelectionHandler.HasSelectedFileContainer())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActFilter", ref TimeActFilters._timeActFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("TimeActList");

        for (int i = 0; i < SelectionHandler.ContainerInfo.TimeActFiles.Count; i++)
        {
            TAE entry = SelectionHandler.ContainerInfo.TimeActFiles[i];

            if (TimeActFilters.TimeActFilter(SelectionHandler.ContainerInfo, entry))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActKey || 
                    SelectionHandler.TimeActMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@"{TimeActUtils.GetTimeActName(entry.ID)}##TimeAct{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActChange(entry, i);
                }
                TimeActUtils.DisplayTimeActAlias(SelectionHandler.ContainerInfo, entry.ID);

                SelectionHandler.TimeActContextMenu.TimeActMenu(isSelected);
            }

        }
        ImGui.EndChild();

        ImGui.End();
    }

    
    public void TimeActAnimationView()
    {
        ImGui.Begin("Animations##TimeActAnimationList");

        if (!SelectionHandler.HasSelectedTimeAct())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActAnimationFilter", ref TimeActFilters._timeActAnimationFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("AnimationList");

        for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            TAE.Animation entry = SelectionHandler.CurrentTimeAct.Animations[i];

            if (TimeActFilters.TimeActAnimationFilter(SelectionHandler.ContainerInfo, entry))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActAnimationIndex ||
                    SelectionHandler.TimeActAnimationMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@" {entry.ID}##taeAnim{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActAnimationChange(entry, i);
                }
                TimeActUtils.DisplayAnimationAlias(SelectionHandler, entry.ID);

                SelectionHandler.TimeActAnimationContextMenu.TimeActAnimationMenu(isSelected);
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }

    public void TimeActAnimEventGraphView()
    {
        ImGui.Begin("Events##TimeActAnimEventList");

        if (!SelectionHandler.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventFilter", ref TimeActFilters._timeActEventFilterString, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventList");

        for (int i = 0; i < SelectionHandler.CurrentTimeActAnimation.Events.Count; i++)
        {
            TAE.Event evt = SelectionHandler.CurrentTimeActAnimation.Events[i];

            if (TimeActFilters.TimeActEventFilter(SelectionHandler.ContainerInfo, evt))
            {
                var isSelected = false;
                if (i == SelectionHandler.CurrentTimeActEventIndex ||
                    SelectionHandler.TimeActEventMultiselect.IsMultiselected(i))
                {
                    isSelected = true;
                }

                if (ImGui.Selectable($@" {evt.TypeName}##taeEvent{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectionHandler.TimeActEventChange(evt, i);
                }

                SelectionHandler.TimeActEventContextMenu.TimeActEventMenu(isSelected);
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        if (CFG.Current.AutoLoadBank_TimeAct)
        {
            AnimationBank.LoadTimeActs();
        }

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeAct(SelectionHandler.ContainerInfo, SelectionHandler.ContainerBinder);
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
