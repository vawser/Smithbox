using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Settings;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.EmevdEditor;

public class EmevdEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    private readonly PropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    private EventScriptInfo _selectedFileInfo;
    private EMEVD _selectedScript;
    private string _selectedScriptKey;

    private EMEVD.Event _selectedEvent;

    public EmevdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "EMEVD Editor##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";

    public void Init()
    {

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

        var dsid = ImGui.GetID("DockSpace_EventScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (false)
        {
            ImGui.Begin("Editor##InvalidEventScriptEditor");

            ImGui.Text($"This editor does not support {Project.Type}.");

            ImGui.End();
        }
        else
        {
            if (!EmevdBank.IsLoaded)
            {
                EmevdBank.LoadEventScripts();
            }

            if (EmevdBank.IsLoaded)
            {
                EventScriptFileView();
                EventScriptEventListView();
                EventScriptEventInstructionView();
                EventScriptEventParameterView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private void EventScriptFileView()
    {
        // File List
        ImGui.Begin("Files##EventScriptFileList");

        ImGui.Text($"Files");
        ImGui.Separator();

        foreach (var (info, binder) in EmevdBank.ScriptBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedScriptKey))
            {
                _selectedScriptKey = info.Name;
                _selectedFileInfo = info;
                _selectedScript = binder;
            }
        }

        ImGui.End();
    }

    private void EventScriptEventListView()
    {
        ImGui.Begin("Events##EventListView");

        if(_selectedScript != null)
        {
            foreach (var evt in _selectedScript.Events)
            {
                if (ImGui.Selectable($@" {evt.ID} - {evt.Name} - {evt.RestBehavior}", evt == _selectedEvent))
                {
                    _selectedEvent = evt;
                }
            }
        }

        ImGui.End();
    }

    private void EventScriptEventInstructionView()
    {
        ImGui.Begin("Event Instructions##EventInstructionView");

        if(_selectedEvent != null)
        {
            foreach (var ins in _selectedEvent.Instructions)
            {
                var eventStr = $"{ins.Bank}[{ins.ID}]";

                var eventArgs = "";

                for (int i = 0; i < ins.ArgData.Length; i++)
                {
                    eventArgs = eventArgs + $"{ins.ArgData[i]} ";
                }

                ImGui.Text($"{eventStr} ({eventArgs})");
            }
        }

        ImGui.End();
    }

    private void EventScriptEventParameterView()
    {
        ImGui.Begin("Event Paramters##EventParameterView");

        if (_selectedEvent != null)
        {
            foreach (var para in _selectedEvent.Parameters)
            {
                ImGui.Text($"InstructionIndex: {para.InstructionIndex}");
                ImGui.Text($"TargetStartByte: {para.TargetStartByte}");
                ImGui.Text($"SourceStartByte: {para.SourceStartByte}");
                ImGui.Text($"ByteCount: {para.ByteCount}");
                ImGui.Text($"UnkID: {para.UnkID}");
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        EmevdBank.LoadEventScripts();

        ResetActionManager();
    }

    public void Save()
    {
        EmevdBank.SaveEventScript(_selectedFileInfo, _selectedScript);
    }

    public void SaveAll()
    {
        if (EmevdBank.IsLoaded)
            EmevdBank.SaveEventScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
