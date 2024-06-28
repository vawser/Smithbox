using Google.Protobuf.WellKnownTypes;
using HKLib.hk2018.hk;
using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class ProjectWindow
{
    private bool MenuOpenState;

    public ProjectWindow()
    {
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Project##ProjectManagementWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##ProjectTabs");

            DisplayProjectTab();
            DisplayProjectToolsTab();
            DisplayProjectEnumsTab();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayProjectTab()
    {
        if (ImGui.BeginTabItem("General"))
        {
            if (Smithbox.ProjectHandler.CurrentProject == null)
            {
                ImGui.Text("No project loaded");
                ImguiUtils.ShowHoverTooltip("No project has been loaded yet.");
            }
            else if (TaskManager.AnyActiveTasks())
            {
                ImGui.Text("Waiting for program tasks to finish...");
                ImguiUtils.ShowHoverTooltip("Smithbox must finished all program tasks before it can load a project.");
            }
            else
            {
                ImGui.Text($"Project Name: {Smithbox.ProjectHandler.CurrentProject.Config.ProjectName}");
                ImGui.Text($"Project Type: {Smithbox.ProjectType}");
                ImGui.Text($"Project Root Directory: {Smithbox.GameRoot}");
                ImGui.Text($"Project Mod Directory: {Smithbox.ProjectRoot}");

                ImGui.Separator();

                if (ImGui.MenuItem("Open project settings file"))
                {
                    var projectPath = CFG.Current.LastProjectFile;
                    Process.Start("explorer.exe", projectPath);
                }

                ImGui.Separator();

                var useLoose = Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams;
                if (Smithbox.ProjectHandler.CurrentProject.Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
                {
                    if (ImGui.Checkbox("Use loose params", ref useLoose))
                        Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams = useLoose;
                    ImguiUtils.ShowHoverTooltip("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                }
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayProjectToolsTab()
    {

        if (ImGui.BeginTabItem("Project Tools"))
        {
            ImGui.Checkbox("Enable Recovery Folder", ref CFG.Current.System_EnableRecoveryFolder);
            ImguiUtils.ShowHoverTooltip("Enable a recovery project to be created upon an unexpected crash.");

            ImGui.Separator();

            ImGui.Checkbox("Enable Automatic Save", ref CFG.Current.System_EnableAutoSave);
            ImguiUtils.ShowHoverTooltip("All changes will be saved at the interval specificed.");

            ImGui.Text("Automatic Save Interval");
            ImguiUtils.ShowHoverTooltip("Interval in seconds between each automatic save.");

            if (ImGui.InputInt("##AutomaticSaveInterval", ref CFG.Current.System_AutoSaveIntervalSeconds))
            {
                if (CFG.Current.System_AutoSaveIntervalSeconds < 10)
                {
                    CFG.Current.System_AutoSaveIntervalSeconds = 10;
                }

                Smithbox.ProjectHandler.UpdateTimer();
            }

            ImGui.Text("Automatically Save:");
            ImguiUtils.ShowHoverTooltip("Determines which elements of Smithbox will be automatically saved, if automatic save is enabled.");

            ImGui.Indent(5.0f);

            ImGui.Checkbox("Project", ref CFG.Current.System_EnableAutoSave_Project);
            ImguiUtils.ShowHoverTooltip("The project.json will be automatically saved.");

            ImGui.Checkbox("Map Editor", ref CFG.Current.System_EnableAutoSave_MapEditor);
            ImguiUtils.ShowHoverTooltip("All loaded maps will be automatically saved.");

            ImGui.Checkbox("Model Editor", ref CFG.Current.System_EnableAutoSave_ModelEditor);
            ImguiUtils.ShowHoverTooltip("The currently loaded model will be automatically saved.");

            ImGui.Checkbox("Param Editor", ref CFG.Current.System_EnableAutoSave_ParamEditor);
            ImguiUtils.ShowHoverTooltip("All params will be automatically saved.");

            ImGui.Checkbox("Text Editor", ref CFG.Current.System_EnableAutoSave_TextEditor);
            ImguiUtils.ShowHoverTooltip("All modified text entries will be automatically saved.");

            ImGui.Checkbox("Gparam Editor", ref CFG.Current.System_EnableAutoSave_GparamEditor);
            ImguiUtils.ShowHoverTooltip("All modified gparams will be automatically saved.");

            ImGui.Unindent();

            ImGui.EndTabItem();
        }

    }

    private ProjectEnumEntry _selectedEntry;
    private string _selectedEntryName;
    private ProjectEnumOption _selectedEntryOption;

    public void DisplayProjectEnumsTab()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (ImGui.BeginTabItem("Project Enums"))
        {
            ImGui.Columns(3);

            ImGui.BeginChild($"ProjectEnumSelectionList");

            DisplayProjectEnumSelectionList();

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild($"ProjectEnumOptionSelectionList");

            DisplayProjectEnumOptionList();

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild($"ProjectEnumActions");

            DisplayProjectEnumAction();

            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    public void DisplayProjectEnumSelectionList()
    {
        ImGui.Separator();
        ImGui.Text("Enums");
        ImGui.Separator();

        // Re-select the previous entry/option after the bank is reloaded so the list updates immediately.
        if (ReselectEntry)
        {
            ReselectEntry = false;

            foreach (var entry in Smithbox.BankHandler.ProjectEnums.Enums.List)
            {
                if (_selectedEntry != null)
                {
                    if (entry.Name == _selectedEntry.Name)
                    {
                        _selectedEntry = entry;
                    }

                    foreach (var opt in _selectedEntry.Options)
                    {
                        if (_selectedEntryOption != null)
                        {
                            if (opt.ID == _selectedEntryOption.ID)
                            {
                                _selectedEntryOption = opt;
                            }
                        }
                    }
                }
            }
        }

        foreach (var entry in Smithbox.BankHandler.ProjectEnums.Enums.List)
        {
            var displayedName = $"{entry.DisplayName}";

            if (ImGui.Selectable(displayedName, entry == _selectedEntry))
            {
                _selectedEntry = entry;
                _refUpdateName = entry.Name;
                _refUpdateDisplayName = entry.DisplayName;
                _refUpdateDescription = entry.Description;
                _selectedEntryName = entry.Name;
            }
        }
    }

    public void DisplayProjectEnumOptionList()
    {
        ImGui.Separator();
        ImGui.Text("Options");
        ImGui.Separator();

        if (_selectedEntry != null)
        {
            foreach(var option in _selectedEntry.Options)
            {
                var name = $"{option.ID} - {option.Name}";

                if (ImGui.Selectable(name, option == _selectedEntryOption))
                {
                    _selectedEntryOption = option;
                    _refUpdateOptionID = option.ID;
                    _refUpdateOptionName = option.Name;
                    _refUpdateOptionDescription = option.Description;
                }
            }
        }
    }

    private string _refUpdateName = "";
    private string _refUpdateDisplayName = "";
    private string _refUpdateDescription = "";

    private string _refUpdateOptionID = "";
    private string _refUpdateOptionName = "";
    private string _refUpdateOptionDescription = "";

    private string _refNewOptionID = "";
    private string _refNewOptionName = "";
    private string _refNewOptionDescription = "";

    private bool ReselectEntry = false;

    public void DisplayProjectEnumAction()
    {
        var scale = Smithbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20 * scale);
        var buttonSize = new Vector2(width, 24 * scale);

        // Entry
        if (_selectedEntry != null)
        {
            // Selected Entry
            if (ImGui.CollapsingHeader("Selected Enum"))
            {
                ImGui.Text($"Display Name");
                ImguiUtils.ShowHoverTooltip("Defines the display name of this enum list.");
                ImGui.InputTextMultiline($"##SelectedEntry_EditDisplayName", ref _refUpdateDisplayName, 255, inputSize);
                ImGui.Text($"Description");
                ImguiUtils.ShowHoverTooltip("A description of what this enum list is for.");
                ImGui.InputTextMultiline($"##SelectedEntry_EditDescription", ref _refUpdateDescription, 255, inputSize);

                if (ImGui.Button("Update Entry", buttonSize))
                {
                    var newEntry = new ProjectEnumEntry().Clone(_selectedEntry);
                    newEntry.Name = _refUpdateName;
                    newEntry.DisplayName = _refUpdateDisplayName;
                    newEntry.Description = _refUpdateDescription;

                    Smithbox.BankHandler.ProjectEnums.UpdateEnumEntry(newEntry);
                    ReselectEntry = true;
                }

                if (ImGui.Button("Restore to Default", buttonSize))
                {
                    Smithbox.BankHandler.ProjectEnums.RestoreBaseEnumEntry(_selectedEntry);
                    ReselectEntry = true;
                }
            }

            if (_selectedEntry != null)
            {
                if (_selectedEntry.Options.Count > 0)
                {
                    if (_selectedEntryOption != null)
                    {
                        // Selected Option
                        if (ImGui.CollapsingHeader("Selected Option"))
                        {
                            ImGui.Text($"Name");
                            ImGui.InputTextMultiline($"##UpdateOptionName", ref _refUpdateOptionName, 255, inputSize);
                            ImguiUtils.ShowHoverTooltip("The display name of the enum option.");
                            //ImGui.Text($"Description");
                            //ImGui.InputTextMultiline($"##UpdateOptionDescription", ref _refUpdateOptionDescription, 255, inputSize);
                            //ImguiUtils.ShowHoverTooltip("A description of what this enum option is.");

                            if (ImGui.Button("Update Option", buttonSize))
                            {
                                var newOption = new ProjectEnumOption().Clone(_selectedEntryOption);
                                newOption.ID = _refUpdateOptionID;
                                newOption.Name = _refUpdateOptionName;
                                newOption.Description = _refUpdateOptionDescription;

                                Smithbox.BankHandler.ProjectEnums.UpdateEnumEntryOption(_selectedEntry, newOption);
                                ReselectEntry = true;
                            }
                            if (ImGui.Button("Delete Option", buttonSize))
                            {
                                Smithbox.BankHandler.ProjectEnums.RemoveEnumEntryOption(_selectedEntry, _selectedEntryOption);
                                _selectedEntryOption = null;
                                ReselectEntry = true;
                            }
                        }
                    }
                }

                // New Option
                if (ImGui.CollapsingHeader("New Option"))
                {
                    ImGui.Text($"ID");
                    ImGui.InputTextMultiline($"##NewOptionID", ref _refNewOptionID, 255, inputSize);
                    ImguiUtils.ShowHoverTooltip("The numeric ID of the enum option.");
                    ImGui.Text($"Name");
                    ImGui.InputTextMultiline($"##NewOptionName", ref _refNewOptionName, 255, inputSize);
                    ImguiUtils.ShowHoverTooltip("The display name of the enum option.");
                    //ImGui.Text($"Description");
                    //ImGui.InputTextMultiline($"##NewOptionDescription", ref _refNewOptionDescription, 255, inputSize);
                    //ImguiUtils.ShowHoverTooltip("A description of what this enum option is.");

                    if (ImGui.Button("Add Option", buttonSize))
                    {
                        if (_selectedEntry.Options.Where(e => e.ID == _refNewOptionID).Any())
                        {
                            PlatformUtils.Instance.MessageBox($"Option with {_refNewOptionID} already exists for {_selectedEntry.DisplayName}", "Error", MessageBoxButtons.OK);
                        }
                        else
                        {
                            var newOption = new ProjectEnumOption();
                            newOption.ID = _refNewOptionID;
                            newOption.Name = _refNewOptionName;
                            newOption.Description = _refNewOptionDescription;

                            Smithbox.BankHandler.ProjectEnums.UpdateEnumEntryOption(_selectedEntry, newOption);
                            ReselectEntry = true;
                        }
                    }
                }
            }
        }
    }
}
