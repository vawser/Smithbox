using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.ParamEditor;

public class PinGroups
{
    public string ParamGroupPath;
    public string RowGroupPath;
    public string FieldGroupPath;

    private ParamEditorScreen Editor;

    private ParamPinGroupDisplayState CurrentDisplayState = ParamPinGroupDisplayState.Param;

    private string _newGroupName;

    private bool RefreshGroupList = true;

    public List<string> ParamGroupFiles = new List<string>();
    public List<string> RowGroupFiles = new List<string>();
    public List<string> FieldGroupFiles = new List<string>();

    public PinGroups(ParamEditorScreen screen)
    {
        Editor = screen;
        _newGroupName = "";
    }

    public void OnProjectChanged()
    {
        ParamGroupPath = Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Params");
        RowGroupPath = Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Rows");
        FieldGroupPath = Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Fields");
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        UpdateGroupList();

        UIHelper.WrappedText("Create a pin group from your current pinned params, rows or fields, or select an existing pin group to replace your current pinned params, rows or fields.");
        UIHelper.WrappedText("");

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Configuration");
        ImGui.Separator();

        ImGui.Checkbox("Show only pinned params exclusively", ref CFG.Current.Param_PinGroups_ShowOnlyPinnedParams);
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_OnlyShowPinnedParams.HintText}\nWhen enabled, only pinned params will appear in the param list.");

        ImGui.Checkbox("Show only pinned rows exclusively", ref CFG.Current.Param_PinGroups_ShowOnlyPinnedRows);
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_OnlyShowPinnedRows.HintText}\nWhen enabled, only pinned rows will appear in the rows list.");

        ImGui.Checkbox("Show only pinned fields exclusively", ref CFG.Current.Param_PinGroups_ShowOnlyPinnedFields);
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_OnlyShowPinnedFields.HintText}\nWhen enabled, only pinned fields will appear in the param list.");

        if (ImGui.Button("Clear Param Pins", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            Editor.Project.Descriptor.PinnedParams = new();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ClearCurrentPinnedParams.HintText}\nClear current pinned params.");

        ImGui.SameLine();
        if (ImGui.Button("Clear Row Pins", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            Editor.Project.Descriptor.PinnedRows = new();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ClearCurrentPinnedRows.HintText}\nClear current pinned rows.");

        ImGui.SameLine();
        if (ImGui.Button("Clear Field Pins", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            Editor.Project.Descriptor.PinnedFields = new();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ClearCurrentPinnedFields.HintText}\nClear current pinned fields.");


        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Group Creation");
        ImGui.Separator();

        UIHelper.WrappedText("Name");
        DPI.ApplyInputWidth(windowWidth);
        ImGui.InputText("##newGroupName", ref _newGroupName, 255);

        if (ImGui.Button("Create Param Group", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CreateParamGroup();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_CreateParamPinGroup.HintText}\nCreate a new pin group from the current pinned params.");

        ImGui.SameLine();
        if (ImGui.Button("Create Row Group", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CreateRowGroup();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_CreateRowPinGroup.HintText}\nCreate a new pin group from the current pinned rows.");

        ImGui.SameLine();
        if (ImGui.Button("Create Field Group", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CreateFieldGroup();
        }
        UIHelper.Tooltip($"{KeyBindings.Current.PARAM_CreateFieldPinGroup.HintText}\nCreate a new pin group from the current pinned fields.");

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Group Lists");
        ImGui.Separator();

        if (ImGui.Button("View Param Groups", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CurrentDisplayState = ParamPinGroupDisplayState.Param;
        }
        ImGui.SameLine();
        if (ImGui.Button("View Row Groups", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CurrentDisplayState = ParamPinGroupDisplayState.Row;
        }
        ImGui.SameLine();
        if (ImGui.Button("View Field Groups", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            CurrentDisplayState = ParamPinGroupDisplayState.Field;
        }

        if (CurrentDisplayState == ParamPinGroupDisplayState.Param)
        {
            ImGui.Separator();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Param Groups:");
            ImGui.Separator();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Row)
        {
            ImGui.Separator();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Row Groups:");
            ImGui.Separator();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Field)
        {
            ImGui.Separator();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Field Groups:");
            ImGui.Separator();
        }

        ImGui.Columns(2);

        ImGui.BeginChild("##groupSelectionList");

        if (CurrentDisplayState == ParamPinGroupDisplayState.Param)
        {
            DisplayParamGroups();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Row)
        {
            DisplayRowGroups();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Field)
        {
            DisplayFieldGroups();
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        if (CurrentDisplayState == ParamPinGroupDisplayState.Param)
        {
            DisplayParamGroupContent();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Row)
        {
            DisplayRowGroupContent();
        }
        if (CurrentDisplayState == ParamPinGroupDisplayState.Field)
        {
            DisplayFieldGroupContent();
        }

        ImGui.Columns(1);
    }
    
    public void UpdateGroupList()
    {
        if (RefreshGroupList)
        {
            RefreshGroupList = false;

            ParamGroupFiles = new List<string>();
            RowGroupFiles = new List<string>();
            FieldGroupFiles = new List<string>();

            if (Directory.Exists(ParamGroupPath))
            {
                foreach (var file in Directory.EnumerateFiles(ParamGroupPath, "*.json"))
                {
                    var fileName = Path.GetFileName(file);
                    ParamGroupFiles.Add(fileName.Replace(".json", ""));
                }
            }

            if (Directory.Exists(RowGroupPath))
            {
                foreach (var file in Directory.EnumerateFiles(RowGroupPath, "*.json"))
                {
                    var fileName = Path.GetFileName(file);
                    RowGroupFiles.Add(fileName.Replace(".json", ""));
                }
            }

            if (Directory.Exists(FieldGroupPath))
            {
                foreach (var file in Directory.EnumerateFiles(FieldGroupPath, "*.json"))
                {
                    var fileName = Path.GetFileName(file);
                    FieldGroupFiles.Add(fileName.Replace(".json", ""));
                }
            }
        }
    }

    private string _selectedParamGroup;
    private string _selectedRowGroup;
    private string _selectedFieldGroup;

    public void DisplayParamGroups()
    {
        foreach (var entry in ParamGroupFiles)
        {
            if (ImGui.Selectable($"{entry}##paramPinGroup{entry}", entry == _selectedParamGroup, ImGuiSelectableFlags.AllowDoubleClick))
            {
                _selectedParamGroup = entry;
                LoadParamPinGroup(entry);
            }
            UIHelper.Tooltip("Double-click to set current param pins to this group.");

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedParamPinGroup != null)
                {
                    Editor.Project.Descriptor.PinnedParams = _selectedParamPinGroup.Pins;
                }
            }

            if (_selectedParamGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##paramPinGroupSelectionPopup{entry}"))
                {
                    if (ImGui.Selectable("Delete"))
                    {
                        DeletePinGroup(entry, ParamGroupPath);
                    }
                    UIHelper.Tooltip("Delete this group.");

                    ImGui.EndPopup();
                }
            }
        }
    }
    public void DisplayRowGroups()
    {
        foreach (var entry in RowGroupFiles)
        {
            if (ImGui.Selectable($"{entry}##rowPinGroup{entry}", entry == _selectedRowGroup, ImGuiSelectableFlags.AllowDoubleClick))
            {
                _selectedRowGroup = entry;
                LoadRowPinGroup(entry);
            }
            UIHelper.Tooltip("Double-click to set current row pins to this group.");

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedRowPinGroup != null)
                {
                    Editor.Project.Descriptor.PinnedRows = _selectedRowPinGroup.Pins;
                }
            }

            if (_selectedRowGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##rowPinGroupSelectionPopup{entry}"))
                {
                    if (ImGui.Selectable("Delete"))
                    {
                        DeletePinGroup(entry, RowGroupPath);
                    }
                    UIHelper.Tooltip("Delete this group.");

                    ImGui.EndPopup();
                }
            }
        }
    }

    public void DisplayFieldGroups()
    {
        foreach (var entry in FieldGroupFiles)
        {
            if (ImGui.Selectable($"{entry}##fieldPinGroup{entry}", entry == _selectedFieldGroup, ImGuiSelectableFlags.AllowDoubleClick))
            {
                _selectedFieldGroup = entry;
                LoadFieldPinGroup(entry);
            }
            UIHelper.Tooltip("Double-click to set current field pins to this group.");

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedFieldPinGroup != null)
                {
                    Editor.Project.Descriptor.PinnedFields = _selectedFieldPinGroup.Pins;
                }
            }

            if (_selectedFieldGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##fieldPinGroupSelectionPopup{entry}"))
                {
                    if (ImGui.Selectable("Delete"))
                    {
                        DeletePinGroup(entry, FieldGroupPath);
                    }
                    UIHelper.Tooltip("Delete this group.");

                    ImGui.EndPopup();
                }
            }
        }
    }

    public void DisplayParamGroupContent()
    {
        if(_selectedParamPinGroup != null)
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Params:");

            foreach (var entry in _selectedParamPinGroup.Pins)
            {
                UIHelper.WrappedText($"{entry}");
            }
        }
    }

    public void DisplayRowGroupContent()
    {
        if (_selectedRowPinGroup != null)
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Rows:");

            foreach (var entry in _selectedRowPinGroup.Pins)
            {
                UIHelper.WrappedText($"{entry.Key}:");
                foreach (var listEntry in entry.Value)
                {
                    UIHelper.WrappedText($" {listEntry}");
                }
            }
        }
    }

    public void DisplayFieldGroupContent()
    {
        if (_selectedFieldPinGroup != null)
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Fields:");

            foreach (var entry in _selectedFieldPinGroup.Pins)
            {
                UIHelper.WrappedText($"{entry.Key}:");
                foreach (var listEntry in entry.Value)
                {
                    UIHelper.WrappedText($" {listEntry}");
                }
            }
        }
    }

    private int autoGroupNameId = 0;

    public void SetAutoGroupName(string type)
    {
        _newGroupName = $"{type} Group {autoGroupNameId}";
        autoGroupNameId = autoGroupNameId + 1;
    }

    public void CreateParamGroup()
    {
        if(_newGroupName == "")
        {
            PlatformUtils.Instance.MessageBox("Group name cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ParamPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Editor.Project.Descriptor.PinnedParams;

        var jsonString = JsonSerializer.Serialize(newGroup, ParamPinGroupSerializationContext.Default.ParamPinGroup);
        WritePinGroup($"{_newGroupName}.json", jsonString, ParamGroupPath, "Param Pin Group");
    }
    public void CreateRowGroup()
    {
        if (_newGroupName == "")
        {
            PlatformUtils.Instance.MessageBox("Group name cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        RowPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Editor.Project.Descriptor.PinnedRows;

        var jsonString = JsonSerializer.Serialize(newGroup, RowPinGroupSerializationContext.Default.RowPinGroup);
        WritePinGroup($"{_newGroupName}.json", jsonString, RowGroupPath, "Row Pin Group");
    }
    public void CreateFieldGroup()
    {
        if (_newGroupName == "")
        {
            PlatformUtils.Instance.MessageBox("Group name cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        FieldPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Editor.Project.Descriptor.PinnedFields;

        var jsonString = JsonSerializer.Serialize(newGroup, FieldPinGroupSerializationContext.Default.FieldPinGroup);
        WritePinGroup($"{_newGroupName}.json", jsonString, FieldGroupPath, "Field Pin Group");
    }

    private ParamPinGroup _selectedParamPinGroup;
    private RowPinGroup _selectedRowPinGroup;
    private FieldPinGroup _selectedFieldPinGroup;

    public void LoadParamPinGroup(string groupName)
    {
        var readPath = Path.Join(ParamGroupPath, $"{groupName}.json");

        try
        {
            var jsonString = File.ReadAllText(readPath);
            var pinGroup = JsonSerializer.Deserialize<ParamPinGroup>(jsonString, ParamPinGroupSerializationContext.Default.ParamPinGroup);

            _selectedParamPinGroup = pinGroup;
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(readPath);
            TaskLogs.AddLog($"Failed to load param pin group: {filename} at {readPath}\n{ex}");
        }
    }
    public void LoadRowPinGroup(string groupName)
    {
        var readPath = Path.Join(RowGroupPath, $"{groupName}.json");

        try
        {
            var jsonString = File.ReadAllText(readPath);
            var pinGroup = JsonSerializer.Deserialize<RowPinGroup>(jsonString, RowPinGroupSerializationContext.Default.RowPinGroup);

            _selectedRowPinGroup = pinGroup;
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(readPath);
            TaskLogs.AddLog($"Failed to load row pin group: {filename} at {readPath}\n{ex}");
        }
    }
    public void LoadFieldPinGroup(string groupName)
    {
        var readPath = Path.Join(FieldGroupPath, $"{groupName}.json");

        try
        {
            var jsonString = File.ReadAllText(readPath);
            var pinGroup = JsonSerializer.Deserialize<FieldPinGroup>(jsonString, FieldPinGroupSerializationContext.Default.FieldPinGroup);

            _selectedFieldPinGroup = pinGroup;
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(readPath);
            TaskLogs.AddLog($"Failed to load field pin group: {filename} at {readPath}\n{ex}");
        }
    }

    public void WritePinGroup(string filename, string jsonString, string basePath, string groupName)
    {
        var writePath = Path.Combine($"{basePath}", $"{filename}");

        if (!Directory.Exists($"{basePath}"))
        {
            Directory.CreateDirectory($"{basePath}");
        }

        var proceed = true;

        if (File.Exists(writePath))
        {
            var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a {groupName}. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

            if (result is DialogResult.Cancel)
            {
                proceed = false;
            }
        }

        if (proceed)
        {
            try
            {
                var fs = new FileStream(writePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                TaskLogs.AddLog($"Pin Group: saved pin group: {filename} at {writePath}.");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Pin Group: failed to save pin group: {filename} at {writePath}\n{ex}");
            }

            RefreshGroupList = true;
        }
    }

    public void DeletePinGroup(string groupName, string basePath)
    {
        var filepath = Path.Combine(basePath, $"{groupName}.json");

        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }

        RefreshGroupList = true;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(ParamPinGroup))]
public partial class ParamPinGroupSerializationContext
    : JsonSerializerContext
{ }

public class ParamPinGroup
{
    public string Name { get; set; }
    public List<string> Pins { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(RowPinGroup))]
public partial class RowPinGroupSerializationContext
    : JsonSerializerContext
{ }

public class RowPinGroup
{
    public string Name { get; set; }
    public Dictionary<string, List<int>> Pins { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(FieldPinGroup))]
public partial class FieldPinGroupSerializationContext
    : JsonSerializerContext
{ }

public class FieldPinGroup
{
    public string Name { get; set; }
    public Dictionary<string, List<string>> Pins { get; set; }
}