using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.EmevdEditor;

public class EmevdEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    public EventScriptInfo _selectedFileInfo;
    public EMEVD _selectedScript;
    public string _selectedScriptKey;

    public EMEVD.Event _selectedEvent;
    public EMEVD.Instruction _selectedInstruction;

    public EventParameterEditor EventParameterEditor;
    public InstructionParameterEditor InstructionParameterEditor;

    public EmevdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        EventParameterEditor = new EventParameterEditor(this);
        InstructionParameterEditor = new InstructionParameterEditor(this);
    }

    public string EditorName => "EMEVD Editor##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";

    public void Init()
    {
        ShowSaveOption = true;
    }
    public void DrawEditorMenu()
    {
        ImGui.Separator();

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

        if (!(Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            ImGui.Begin("Editor##InvalidEmevdEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!EmevdBank.IsLoaded)
            {
                EmevdBank.LoadEventScripts();
                EmevdBank.LoadEMEDF();
            }

            if (EmevdBank.IsLoaded && EmevdBank.IsSupported)
            {
                EventScriptFileView();
                EventScriptEventListView();
                EventScriptEventInstructionView();
                EventScriptEventParameterView();
                EventScriptInstructionParameterView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private bool SelectScript = false;

    private void EventScriptFileView()
    {
        // File List
        ImGui.Begin("Files##EventScriptFileList");

        ImGui.Text($"Files");
        ImGui.Separator();

        foreach (var (info, binder) in EmevdBank.ScriptBank)
        {
            var displayName = $"{info.Name}";

            // Script row
            if (ImGui.Selectable(displayName, info.Name == _selectedScriptKey))
            {
                _selectedScriptKey = info.Name;
                _selectedFileInfo = info;
                _selectedScript = binder;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && SelectScript)
            {
                SelectScript = false;
                _selectedScriptKey = info.Name;
                _selectedFileInfo = info;
                _selectedScript = binder;
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                SelectScript = true;
            }

            var aliasName = AliasUtils.GetMapNameAlias(info.Name);
            AliasUtils.DisplayAlias(aliasName);
        }

        ImGui.End();
    }

    private bool SelectEvent = false;

    private void EventScriptEventListView()
    {
        ImGui.Begin("Events##EventListView");

        if(_selectedScript != null)
        {
            foreach (var evt in _selectedScript.Events)
            {
                // Event row
                if (ImGui.Selectable($@" {evt.ID} - {evt.Name} - {evt.RestBehavior}", evt == _selectedEvent))
                {
                    _selectedEvent = evt;
                }
                
                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEvent)
                {
                    SelectEvent = false;
                    _selectedEvent = evt;
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    SelectEvent = true;
                }
            }
        }

        ImGui.End();
    }

    private bool SelectEventInstruction = false;

    private void EventScriptEventInstructionView()
    {
        ImGui.Begin("Event Instructions##EventInstructionView");

        if(_selectedEvent != null)
        {
            foreach (var ins in _selectedEvent.Instructions)
            {
                if (ImGui.Selectable($@" {ins.Bank}[{ins.ID}]", ins == _selectedInstruction))
                {
                    _selectedInstruction = ins;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectEventInstruction)
                {
                    SelectEventInstruction = false;
                    _selectedInstruction = ins;
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    SelectEventInstruction = true;
                }

                DisplayInstructionAlias(ins);
            }

        }

        ImGui.End();
    }

    private void DisplayInstructionAlias(EMEVD.Instruction ins)
    {
        var classStr = "Unknown";
        var insStr = "Unknown";
        var argsStr = "";

        foreach (var classEntry in EmevdBank.InfoBank.Classes)
        {
            if (ins.Bank == classEntry.Index)
            {
                classStr = classEntry.Name;

                foreach (var insEntry in classEntry.Instructions)
                {
                    if(ins.ID == insEntry.Index)
                    {
                        insStr = insEntry.Name;

                        for (int i = 0; i < insEntry.Arguments.Length; i++)
                        {
                            var argEntry = insEntry.Arguments[i];
                            string separator = ", ";

                            if (i == insEntry.Arguments.Length - 1)
                            {
                                separator = "";
                            }

                            argsStr = $"{argsStr}{argEntry.Name}{separator}";
                        }
                    }
                }

            }
        }

        if (argsStr == "")
            argsStr = "Unknown";


        AliasUtils.DisplayAlias($"{classStr} [{insStr}]");
        AliasUtils.DisplayColoredAlias($"({argsStr})", CFG.Current.ImGui_Benefit_Text_Color);
    }

    private void EventScriptEventParameterView()
    {
        ImGui.Begin("Event Parameters##EventParameterView");

        EventParameterEditor.Display();

        ImGui.End();
    }

    private void EventScriptInstructionParameterView()
    {
        ImGui.Begin("Instruction Parameters##InstructionParameterView");

        InstructionParameterEditor.Display();

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        EventParameterEditor.OnProjectChanged();
        InstructionParameterEditor.OnProjectChanged();

        EmevdBank.LoadEventScripts();
        EmevdBank.LoadEMEDF();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        EmevdBank.SaveEventScript(_selectedFileInfo, _selectedScript);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EmevdBank.IsLoaded)
            EmevdBank.SaveEventScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
