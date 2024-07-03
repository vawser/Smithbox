using ImGuiNET;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Core;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Tabs;

public class ProjectEnumTab
{

    private ProjectEnumEntry _selectedEntry;
    private string _selectedEntryName;
    private ProjectEnumOption _selectedEntryOption;

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

    public ProjectEnumTab() { }

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (ImGui.BeginTabItem("Enums"))
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
            foreach (var option in _selectedEntry.Options)
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
