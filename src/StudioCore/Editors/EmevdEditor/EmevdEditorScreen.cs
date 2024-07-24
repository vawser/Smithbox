using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Localization;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.EmevdEditor;

public class EmevdEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    private EventScriptInfo _selectedFileInfo;
    private EMEVD _selectedScript;
    private string _selectedScriptKey;

    private EMEVD.Event _selectedEvent;

    public EmevdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
    }

    public string EditorName => $"{LOC.Get("EDITOR__EMEVD_EDITOR")}##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => $"{LOC.Get("EDITOR__EMEVD_EDITOR_SAVE_TYPE")}";

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

        var dsid = ImGui.GetID("DockSpace_EventScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!EmevdBank.IsLoaded)
        {
            EmevdBank.LoadEventScripts();
            EmevdBank.LoadEMEDF();
        }

        if (EmevdBank.IsLoaded)
        {
            EventScriptFileView();
            EventScriptEventListView();
            EventScriptEventInstructionView();
            EventScriptEventParameterView();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private void EventScriptFileView()
    {
        // File List
        ImGui.Begin($"{LOC.Get("EMEVD_EDITOR__MENU__FILES")}" + "##EventScriptFileList");

        foreach (var (info, binder) in EmevdBank.ScriptBank)
        {
            var displayName = $"{info.Name}";

            if (ImGui.Selectable(displayName, info.Name == _selectedScriptKey))
            {
                _selectedScriptKey = info.Name;
                _selectedFileInfo = info;
                _selectedScript = binder;
            }
            var aliasName = AliasUtils.GetMapNameAlias(info.Name);
            AliasUtils.DisplayAlias(aliasName);
        }

        ImGui.End();
    }

    private void EventScriptEventListView()
    {
        ImGui.Begin($"{LOC.Get("EMEVD_EDITOR__MENU__EVENTS")}" + "##EventListView");

        if (_selectedScript != null)
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

    Vector4 insColor = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);

    private void EventScriptEventInstructionView()
    {
        ImGui.Begin($"{LOC.Get("EMEVD_EDITOR__MENU__EVENT_INSTRUCTIONS")}" + "##EventInstructionView");

        if (_selectedEvent != null)
        {
            ImGui.Columns(2);
            foreach (var ins in _selectedEvent.Instructions)
            {
                ShowRawDisplay(ins);
            }

            ImGui.NextColumn();

            foreach (var ins in _selectedEvent.Instructions)
            {
                ShowSimpleDisplay(ins);
            }

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    Dictionary<long, string> InstructionTypes = new Dictionary<long, string>()
    {
        [0] = "byte",
        [1] = "ushort",
        [2] = "uint",
        [3] = "sbyte",
        [4] = "short",
        [5] = "int",
        [6] = "float"
    };

    private void ShowRawDisplay(EMEVD.Instruction ins)
    {
        var eventStr = $"{ins.Bank}[{ins.ID}]";
        var eventArgs = "";
        int argIndex = 0;

        foreach (var classEntry in EmevdBank.InfoBank.Classes)
        {
            if (ins.Bank == classEntry.Index)
            {
                foreach (var insEntry in classEntry.Instructions)
                {
                    if (ins.ID == insEntry.Index)
                    {
                        // This formats the args byte array into the right primitive types for display
                        for(int i = 0; i < insEntry.Arguments.Length; i++)
                        {
                            var argEntry = insEntry.Arguments[i];
                            string separator = ", ";

                            if(i == insEntry.Arguments.Length - 1)
                            {
                                separator = "";
                            }

                            // TODO: peek and check we don't exceed the length of the arguments
                            if (InstructionTypes.ContainsKey(argEntry.Type))
                            {
                                switch (argEntry.Type)
                                {
                                    // byte
                                    case 0:
                                        if (argIndex + 1 <= ins.ArgData.Length)
                                        {
                                            var byteVal = ins.ArgData[argIndex];
                                            eventArgs = eventArgs + $"{byteVal}{separator}";
                                            argIndex += 1;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // ushort
                                    case 1:
                                        if (argIndex + 2 <= ins.ArgData.Length)
                                        {
                                            var ushortVal = BitConverter.ToUInt16(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{ushortVal}{separator}";
                                            argIndex += 2;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // uint
                                    case 2:
                                        if (argIndex + 4 <= ins.ArgData.Length)
                                        {
                                            var uintVal = BitConverter.ToUInt32(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{uintVal}{separator}";
                                            argIndex += 4;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // sbyte
                                    case 3:
                                        if (argIndex + 1 <= ins.ArgData.Length)
                                        {
                                            var sbyteVal = (sbyte)ins.ArgData[argIndex];
                                            eventArgs = eventArgs + $"{sbyteVal}{separator}";
                                            argIndex += 1;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // short
                                    case 4:
                                        if (argIndex + 2 <= ins.ArgData.Length)
                                        {
                                            var shortVal = BitConverter.ToInt16(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{shortVal}{separator}";
                                            argIndex += 2;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // int
                                    case 5:
                                        if (argIndex + 4 <= ins.ArgData.Length)
                                        {
                                            var intVal = BitConverter.ToInt32(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{intVal}{separator}";
                                            argIndex += 4;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // float
                                    case 6:
                                        if (argIndex + 4 <= ins.ArgData.Length)
                                        {
                                            var floatVal = BitConverter.ToSingle(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{floatVal}{separator}";
                                            argIndex += 4;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // uint
                                    case 8:
                                        if (argIndex + 4 <= ins.ArgData.Length)
                                        {
                                            var uintVal2 = BitConverter.ToUInt32(ins.ArgData, argIndex);
                                            eventArgs = eventArgs + $"{uintVal2}{separator}";
                                            argIndex += 4;
                                        }
                                        else
                                        {
                                            //TaskLogs.AddLog($"{eventStr}: Stored Length of {argIndex} exceeded actual length");
                                        }
                                        break;
                                    // unk
                                    default: break;
                                }
                            }
                        }
                    }
                }

            }
        }

        ImguiUtils.WrappedText($"{eventStr}");
        ImGui.SameLine();
        ImguiUtils.WrappedTextColored(insColor, $"({eventArgs})");
    }

    private void ShowSimpleDisplay(EMEVD.Instruction ins)
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

        ImguiUtils.WrappedText($"{classStr} [{insStr}]");
        ImGui.SameLine();
        ImguiUtils.WrappedTextColored(insColor, $"({argsStr})");
    }

    private void EventScriptEventParameterView()
    {
        ImGui.Begin("Event Parameters##EventParameterView");

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
