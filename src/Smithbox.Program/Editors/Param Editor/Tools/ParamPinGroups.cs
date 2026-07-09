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

public class ParamPinGroups
{
    public string ParamGroupPath = "";
    public string RowGroupPath = "";
    public string FieldGroupPath = "";

    private ParamEditorView View;
    private ProjectEntry Project;

    private ParamPinGroupDisplayState CurrentDisplayState = ParamPinGroupDisplayState.Param;

    private string _newGroupName = "";

    private bool RefreshGroupList = true;

    public List<string> ParamGroupFiles = new List<string>();
    public List<string> RowGroupFiles = new List<string>();
    public List<string> FieldGroupFiles = new List<string>();

    public ParamPinGroups(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        ParamGroupPath = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Params");
        RowGroupPath = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Rows");
        FieldGroupPath = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Pin Groups", "Fields");
    }
    
    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Pin Groups
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_PinGroups_Header")}##pinGroupsHeader"))
        {
            ImGui.BeginChild("PinGroupToolSection", ImGuiChildFlags.Borders);

            UpdateGroupList();

            GUI.WrappedText(LOC.Get("PARAM_PinGroups_Hint"));

            // Options
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_PinGroups_Header_Options"),
                LOC.Get("PARAM_PinGroups_Header_Options_TT"));

            ImGui.Checkbox($"{LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Params_Only")}##togglePinnedParams", 
                ref CFG.Current.Param_PinGroups_ShowOnlyPinnedParams);
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Params_Only_TT"));

            ImGui.Checkbox($"{LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Rows_Only")}##togglePinnedRows",
                ref CFG.Current.Param_PinGroups_ShowOnlyPinnedRows);
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Rows_Only_TT"));

            ImGui.Checkbox($"{LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Fields_Only")}##togglePinnedFields", 
                ref CFG.Current.Param_PinGroups_ShowOnlyPinnedFields);
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_Checkbox_Pinned_Fields_Only_TT"));

            // Display
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_PinGroups_Header_Display"),
                LOC.Get("PARAM_PinGroups_Header_Display_TT"));

            GUI.MultiButtonInput("pinGroupActions",
                "clearParamPins", 
                LOC.Get("PARAM_PinGroups_Action_Clear_Param_Pins"),
                LOC.Get("PARAM_PinGroups_Action_Clear_Param_Pins_TT"),
                ClearParamPins,

                "clearRowPins",
                LOC.Get("PARAM_PinGroups_Action_Clear_Row_Pins"),
                LOC.Get("PARAM_PinGroups_Action_Clear_Row_Pins_TT"), 
                ClearRowPins,

                "clearFieldPins",
                LOC.Get("PARAM_PinGroups_Action_Clear_Field_Pins"),
                LOC.Get("PARAM_PinGroups_Action_Clear_Field_Pins_TT"), 
                ClearFieldPins);

            // Creation
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_PinGroups_Header_Creation"),
                LOC.Get("PARAM_PinGroups_Header_Creation_TT"));

            GUI.SinglelineTextInputWithHint($"newGroupName", ref _newGroupName, LOC.Get("PARAM_PinGroups_Input_Name_Hint"));

            GUI.MultiButtonInput("pinGroupActions",
                "createParamPinGroup",
                LOC.Get("PARAM_PinGroups_Action_Create_Param_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Create_Param_Pin_Group_TT"),
                CreateParamGroup,

                "createRowPinGroup",
                LOC.Get("PARAM_PinGroups_Action_Create_Row_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Create_Row_Pin_Group_TT"), 
                CreateRowGroup,

                "createFieldPinGroup",
                LOC.Get("PARAM_PinGroups_Action_Create_Field_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Create_Field_Pin_Group_TT"), 
                CreateFieldGroup);

            // Lists
            GUI.Spacer();
            GUI.SimpleHeader("Lists", "");

            GUI.MultiButtonInput("pinGroupDisplayActions",
                "displayParamPinGroups", 
                LOC.Get("PARAM_PinGroups_Action_Display_Param_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Display_Param_Pin_Group_TT"),
                DisplayParamPinGroups,

                "displayRowPinGroups",
                LOC.Get("PARAM_PinGroups_Action_Display_Row_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Display_Row_Pin_Group_TT"), 
                DisplayRowPinGroups,

                "displayFieldPinGroups",
                LOC.Get("PARAM_PinGroups_Action_Display_Field_Pin_Group"),
                LOC.Get("PARAM_PinGroups_Action_Display_Field_Pin_Group_TT"), 
                DisplayFieldPinGroups);

            GUI.Spacer();

            // Table
            if (CurrentDisplayState == ParamPinGroupDisplayState.Param)
            {
                GUI.SimpleHeader(
                    LOC.Get("PARAM_PinGroups_Header_Param_Groups"),
                    LOC.Get("PARAM_PinGroups_Header_Param_Groups_TT"));
            }
            if (CurrentDisplayState == ParamPinGroupDisplayState.Row)
            {
                GUI.SimpleHeader(
                    LOC.Get("PARAM_PinGroups_Header_Row_Groups"),
                    LOC.Get("PARAM_PinGroups_Header_Row_Groups_TT"));
            }
            if (CurrentDisplayState == ParamPinGroupDisplayState.Field)
            {
                GUI.SimpleHeader(
                    LOC.Get("PARAM_PinGroups_Header_Field_Groups"),
                    LOC.Get("PARAM_PinGroups_Header_Field_Groups_TT"));
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

            ImGui.EndChild();
        }
    }

    public void ClearParamPins()
    {
        Project.Descriptor.PinnedParams = new();
    }

    public void ClearRowPins()
    {
        Project.Descriptor.PinnedRows = new();
    }

    public void ClearFieldPins()
    {
        Project.Descriptor.PinnedFields = new();
    }

    public void DisplayParamPinGroups()
    {
        CurrentDisplayState = ParamPinGroupDisplayState.Param;
    }
    public void DisplayRowPinGroups()
    {
        CurrentDisplayState = ParamPinGroupDisplayState.Row;
    }
    public void DisplayFieldPinGroups()
    {
        CurrentDisplayState = ParamPinGroupDisplayState.Field;
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
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_ParamGroup_Set_TT"));

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedParamPinGroup != null)
                {
                    Project.Descriptor.PinnedParams = _selectedParamPinGroup.Pins;
                }
            }

            if (_selectedParamGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##paramPinGroupSelectionPopup{entry}"))
                {
                    // Delete
                    if (ImGui.Selectable($"{LOC.Get("PARAM_PinGroups_Action_Delete")}##paramGroupDeleteAction"))
                    {
                        DeletePinGroup(entry, ParamGroupPath);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_PinGroups_Action_Delete_TT"));

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
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_RowGroup_Set_TT"));

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedRowPinGroup != null)
                {
                    Project.Descriptor.PinnedRows = _selectedRowPinGroup.Pins;
                }
            }

            if (_selectedRowGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##rowPinGroupSelectionPopup{entry}"))
                {
                    // Delete
                    if (ImGui.Selectable($"{LOC.Get("PARAM_PinGroups_Action_Delete")}##rowGroupDeleteAction"))
                    {
                        DeletePinGroup(entry, RowGroupPath);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_PinGroups_Action_Delete_TT"));

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
            GUI.Tooltip(LOC.Get("PARAM_PinGroups_FieldGroup_Set_TT"));

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (_selectedFieldPinGroup != null)
                {
                    Project.Descriptor.PinnedFields = _selectedFieldPinGroup.Pins;
                }
            }

            if (_selectedFieldGroup == entry)
            {
                if (ImGui.BeginPopupContextItem($"##fieldPinGroupSelectionPopup{entry}"))
                {
                    // Delete
                    if (ImGui.Selectable($"{LOC.Get("PARAM_PinGroups_Action_Delete")}##fieldGroupDeleteAction"))
                    {
                        DeletePinGroup(entry, FieldGroupPath);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_PinGroups_Action_Delete_TT"));

                    ImGui.EndPopup();
                }
            }
        }
    }

    public void DisplayParamGroupContent()
    {
        if(_selectedParamPinGroup != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_PinGroups_Alias_Params"));

            foreach (var entry in _selectedParamPinGroup.Pins)
            {
                GUI.WrappedText($"{entry}");
            }
        }
    }

    public void DisplayRowGroupContent()
    {
        if (_selectedRowPinGroup != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_PinGroups_Alias_Rows"));

            foreach (var entry in _selectedRowPinGroup.Pins)
            {
                GUI.WrappedText($"{entry.Key}:");
                foreach (var listEntry in entry.Value)
                {
                    GUI.WrappedText($" {listEntry}");
                }
            }
        }
    }

    public void DisplayFieldGroupContent()
    {
        if (_selectedFieldPinGroup != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_PinGroups_Alias_Fields"));

            foreach (var entry in _selectedFieldPinGroup.Pins)
            {
                GUI.WrappedText($"{entry.Key}:");
                foreach (var listEntry in entry.Value)
                {
                    GUI.WrappedText($" {listEntry}");
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
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Empty_Group_Name"));
            return;
        }

        ParamPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Project.Descriptor.PinnedParams;

        var jsonString = JsonSerializer.Serialize(newGroup, ParamPinGroupSerializationContext.Default.ParamPinGroup);
        WritePinGroup($"{_newGroupName}.json", jsonString, ParamGroupPath, "Param Pin Group");
    }

    public void CreateRowGroup()
    {
        if (_newGroupName == "")
        {
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Empty_Group_Name"));
            return;
        }

        RowPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Project.Descriptor.PinnedRows;

        var jsonString = JsonSerializer.Serialize(newGroup, RowPinGroupSerializationContext.Default.RowPinGroup);
        WritePinGroup($"{_newGroupName}.json", jsonString, RowGroupPath, "Row Pin Group");
    }

    public void CreateFieldGroup()
    {
        if (_newGroupName == "")
        {
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Empty_Group_Name"));
            return;
        }

        FieldPinGroup newGroup = new();
        newGroup.Name = _newGroupName;
        newGroup.Pins = Project.Descriptor.PinnedFields;

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
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Load_Pin_Group_FAIL", filename, readPath), ex);
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
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Load_Pin_Group_FAIL", filename, readPath), ex);
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
            Smithbox.LogError<ParamPinGroup>(LOC.Get("PARAM_PinGroups_Load_Pin_Group_FAIL", filename, readPath), ex);
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
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("PARAM_PinGroups_Dialog_Overwrite_Existing_Group", filename, groupName),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.OKCancel);

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

                Smithbox.Log<ParamPinGroup>(
                    LOC.Get("PARAM_PinGroups_Save_Pin_Group_PASS", filename, writePath));
            }
            catch (Exception ex)
            {
                Smithbox.LogError<ParamPinGroup>(
                    LOC.Get("PARAM_PinGroups_Save_Pin_Group_FAIL", filename, writePath), ex);
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