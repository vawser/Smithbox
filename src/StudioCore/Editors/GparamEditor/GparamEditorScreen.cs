using ImGuiNET;
using Silk.NET.OpenGL;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    private ProjectSettings _projectSettings;

    private ActionManager EditorActionManager = new();

    private GparamParamBank.GparamInfo _selectedGparamInfo;
    private GPARAM _selectedGparam;
    private string _selectedGparamKey;

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    private GPARAM.Param _selectedParamGroup;
    private int _selectedParamGroupKey;

    private string _paramGroupSearchInput = "";
    private string _paramGroupSearchInputCache = "";

    private GPARAM.IField _selectedParamField;
    private int _selectedParamFieldKey;

    private string _paramFieldSearchInput = "";
    private string _paramFieldSearchInputCache = "";

    private GPARAM.IFieldValue _selectedFieldValue = null;
    private int _selectedFieldValueKey;

    private string _fieldIdSearchInput = "";
    private string _fieldIdSearchInputCache = "";

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!GparamParamBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Gparam)
            {
                if (ImGui.Button("Load Gparam Editor"))
                {
                    GparamParamBank.LoadGraphicsParams();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        GparamShortcuts();

        if (GparamParamBank.IsLoaded)
        {
            GparamListView();
            GparamGroupList();
            GparamFieldList();
            GparamValueProperties();
        }

        ImGui.PopStyleVar();
    }

    public void GparamShortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
        {
            
        }
    }

    private void GparamListView()
    {
        ImGui.Begin("Files##GparamFileList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fileSearchInput != _fileSearchInputCache)
        {
            _fileSearchInputCache = _fileSearchInput;
        }

        foreach (var (name, info) in GparamParamBank.ParamBank)
        {
            if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
            {
                var prettyName = "";
                if (CFG.Current.Gparam_DisplayMapNames)
                {
                    prettyName = MapAliasBank.GetMapNameFromFilename(info.Name);
                }

                ImGui.BeginGroup();
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGparamKey))
                {
                    ResetGroupSelection();
                    ResetFieldSelection();
                    ResetValueSelection();

                    _selectedGparamKey = info.Name;
                    _selectedGparamInfo = info;
                    _selectedGparam = info.Gparam;
                }
                if (prettyName != "")
                {
                    ImGui.SameLine();
                    ImGui.PushTextWrapPos();
                
                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), @$"<{prettyName}>");

                    ImGui.PopTextWrapPos();
                }

                ImGui.EndGroup();
            }

            // Context Menu: File
            if (info.Name == _selectedGparamKey)
            {
                if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
                {
                    // Only allow these actions on mod-local files
                    if (info.IsModFile)
                    {
                        
                    }

                    if (ImGui.Button("Duplicate"))
                    {
                        DuplicateGparamFile();

                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.End();
    }

    public void GparamGroupList()
    {
        ImGui.Begin("Groups##GparamGroups");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramGroupSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramGroupSearchInput != _paramGroupSearchInputCache)
        {
            _paramGroupSearchInputCache = _paramGroupSearchInput;
        }

        if (_selectedGparam != null && _selectedGparamKey != "")
        {
            GPARAM data = _selectedGparam;

            ImGui.Text($"Group");
            ImGui.Separator();

            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                if (SearchFilters.IsEditorSearchMatch(_paramGroupSearchInput, entry.Name, " "))
                {
                    if (ImGui.Selectable($@" {name}##{entry.Key}", i == _selectedParamGroupKey))
                    {
                        ResetFieldSelection();
                        ResetValueSelection();

                        _selectedParamGroup = entry;
                        _selectedParamGroupKey = i;
                    }
                }
            }
        }

        ImGui.End();
    }

    public void GparamFieldList()
    {
        ImGui.Begin("Fields##GparamFields");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramFieldSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramFieldSearchInput != _paramFieldSearchInputCache)
        {
            _paramFieldSearchInputCache = _paramFieldSearchInput;
        }

        if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
        {
            GPARAM.Param data = _selectedParamGroup;

            ImGui.Text($"Field");
            ImGui.Separator();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                if (SearchFilters.IsEditorSearchMatch(_paramFieldSearchInput, entry.Name, " "))
                {
                    if (ImGui.Selectable($@" {name}##{entry.Key}", i == _selectedParamFieldKey))
                    {
                        ResetValueSelection();

                        _selectedParamField = entry;
                        _selectedParamFieldKey = i;
                    }
                }
            }
        }

        ImGui.End();
    }
    private void GparamValueProperties()
    {
        ImGui.Begin("Values##GparamValues");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fieldIdSearchInput, 255);
        ImGui.SameLine();
        ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fieldIdSearchInput != _fieldIdSearchInputCache)
        {
            _fieldIdSearchInputCache = _fieldIdSearchInput;
        }

        if (_selectedParamField != null && _selectedParamFieldKey != -1)
        {
            GPARAM.IField field = _selectedParamField;

            bool[] displayTruth = new bool[field.Values.Count];

            for (int i = 0; i < field.Values.Count; i++)
            {
                displayTruth[i] = true;
            }

            ImGui.Columns(3);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            ImGui.Text($"ID");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = SearchFilters.IsIdSearchMatch(_fieldIdSearchInput, entry.Id.ToString());

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            ImGui.Text($"Value");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_Value(i, field, entry);
                }
            }

            ImGui.EndChild();

            // Information
            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("InfoList##GparamPropertyInfo");
            ImGui.Text($"Information");
            ImGui.Separator();

            // Only show once
            GparamProperty_Info(field);

            ImGui.EndChild();
        }

        ImGui.End();
    }

    public void GparamProperty_ID(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();

        string name = value.Id.ToString();

        if(CFG.Current.Gparam_DisplayUnk04)
        {
            name = $"{name} - Time of Day: {value.Unk04}";
        }


        if (ImGui.Selectable($"{name}##{index}", index == _selectedFieldValueKey))
        {
            _selectedFieldValue = value;
            _selectedFieldValueKey = index;
        }
    }

    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        GparamEditor.PropertyField(index, field, value);
    }

    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = GparamFormatBank.Bank.GetReferenceDescription(_selectedParamField.Key);

        ImGui.Text($"{desc}");
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_Gparam)
            GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(_selectedGparamInfo);
    }

    public void SaveAll()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }


    private void ResetAllSelection()
    {
        ResetFileSelection();
        ResetGroupSelection();
        ResetFieldSelection();
        ResetValueSelection();
    }

    private void ResetFileSelection()
    {
        _selectedGparam = null;
        _selectedGparamKey = "";
    }

    private void ResetGroupSelection()
    {
        _selectedParamGroup = null;
        _selectedParamGroupKey = -1;
    }

    private void ResetFieldSelection()
    {
        _selectedParamField = null;
        _selectedParamFieldKey = -1;
    }

    private void ResetValueSelection()
    {
        _selectedFieldValue = null;
        _selectedFieldValueKey = -1;
    }

    public void DuplicateGparamFile()
    {
        bool isValidFile = false;
        string filePath = _selectedGparamInfo.Path;
        string baseFileName = _selectedGparamInfo.Name;
        string tryFileName = _selectedGparamInfo.Name;

        do
        {
            string currentfileName = CreateDuplicateFileName(tryFileName);
            string newFilePath = filePath.Replace(baseFileName, currentfileName);

            // If the original is in the root dir, change the path to mod
            newFilePath = newFilePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

            if (!File.Exists(newFilePath))
            {
                File.Copy(filePath, newFilePath);
                isValidFile = true;
            }
            else
            {
                TaskLogs.AddLog($"{newFilePath} already exists!");
                tryFileName = currentfileName;
            }
        }
        while (!isValidFile);

        GparamParamBank.LoadGraphicsParams();
    }

    public string CreateDuplicateFileName(string fileName)
    {
        Match mapMatch = Regex.Match(fileName, @"[0-9]{4}");

        if(mapMatch.Success)
        {
            var res = mapMatch.Groups[0].Value;

            int slot = 0;
            string slotStr = "";

            try
            {
                int number;
                int.TryParse(res, out number);

                slot = number + 1;
            }
            catch { }

            if(slot >= 100 && slot < 999)
            {
                slotStr = "0";
            }
            if (slot >= 10 && slot < 99)
            {
                slotStr = "00";
            }
            if (slot >= 0 && slot < 9)
            {
                slotStr = "000";
            }

            var finalSlotStr = $"{slotStr}{slot}";
            var final = fileName.Replace(res, finalSlotStr);

            return final;
        }
        else
        {
            Match dupeMatch = Regex.Match(fileName, @"__[0-9]{1}");

            if (dupeMatch.Success)
            {
                var res = dupeMatch.Groups[0].Value;

                Match numMatch = Regex.Match(res, @"[0-9]{1}");

                var num = numMatch.Groups[0].Value;
                try
                {
                    int number;
                    int.TryParse(res, out number);

                    number = number + 1;

                    return $"{fileName}__{number}";
                }
                catch 
                {
                    return $"{fileName}__1";
                }
            }
            else
            {
                return $"{fileName}__1";
            }
        }
    }
}
