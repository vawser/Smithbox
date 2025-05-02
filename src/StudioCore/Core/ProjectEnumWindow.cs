using Hexa.NET.ImGui;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Editor;
using StudioCore.Interface;

using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Core;

public static class ProjectEnumWindow
{
    public static bool DisplayProjectEnumWindow = false;

    private static ProjectEnumEntry _selectedEntry;
    private static string _selectedEntryName;
    private static ProjectEnumOption _selectedEntryOption;

    private static string _refUpdateName = "";
    private static string _refUpdateDisplayName = "";
    private static string _refUpdateDescription = "";

    private static string _refUpdateOptionID = "";
    private static string _refUpdateOptionName = "";
    private static string _refUpdateOptionDescription = "";

    private static string _refNewOptionID = "";
    private static string _refNewOptionName = "";
    private static string _refNewOptionDescription = "";

    private static bool ReselectEntry = false;

    public static void ToggleWindow()
    {
        DisplayProjectEnumWindow = !DisplayProjectEnumWindow;
    }

    public static void Display()
    {
        if (!DisplayProjectEnumWindow)
            return;

        var scale = DPI.GetUIScale();

        ImGui.SetNextWindowSize(new Vector2(1200.0f, 1000.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin($"Project Enums", ref DisplayProjectEnumWindow, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(3);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBgSecondary);
            ImGui.BeginChild($"ProjectEnumSelectionList");

            DisplayProjectEnumSelectionList();

            ImGui.EndChild();
            ImGui.PopStyleColor(1);

            ImGui.NextColumn();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBgSecondary);
            ImGui.BeginChild($"ProjectEnumOptionSelectionList");

            DisplayProjectEnumOptionList();

            ImGui.EndChild();
            ImGui.PopStyleColor(1);

            ImGui.NextColumn();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBgSecondary);
            ImGui.BeginChild($"ProjectEnumActions");

            DisplayProjectEnumAction();

            ImGui.EndChild();

            ImGui.Columns(1);

            ImGui.PopStyleColor(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public static void DisplayProjectEnumSelectionList()
    {
        ImGui.Separator();
        ImGui.Text("Enums");
        ImGui.Separator();

        // Re-select the previous entry/option after the bank is reloaded so the list updates immediately.
        if (ReselectEntry)
        {
            ReselectEntry = false;

            foreach (var entry in BaseEditor.BankHandler.ProjectEnums.Enums.List)
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

        foreach (var entry in BaseEditor.BankHandler.ProjectEnums.Enums.List)
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

    public static void DisplayProjectEnumOptionList()
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

    public static void DisplayProjectEnumAction()
    {
        var scale = DPI.GetUIScale();
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
                UIHelper.Tooltip("Defines the display name of this enum list.");
                ImGui.InputTextMultiline($"##SelectedEntry_EditDisplayName", ref _refUpdateDisplayName, 255, inputSize);
                ImGui.Text($"Description");
                UIHelper.Tooltip("A description of what this enum list is for.");
                ImGui.InputTextMultiline($"##SelectedEntry_EditDescription", ref _refUpdateDescription, 255, inputSize);

                if (ImGui.Button("Update Entry", buttonSize))
                {
                    var newEntry = new ProjectEnumEntry().Clone(_selectedEntry);
                    newEntry.Name = _refUpdateName;
                    newEntry.DisplayName = _refUpdateDisplayName;
                    newEntry.Description = _refUpdateDescription;

                    BaseEditor.BankHandler.ProjectEnums.UpdateEnumEntry(newEntry);
                    ReselectEntry = true;
                }

                if (ImGui.Button("Restore to Default", buttonSize))
                {
                    BaseEditor.BankHandler.ProjectEnums.RestoreBaseEnumEntry(_selectedEntry);
                    ReselectEntry = true;
                }

                if (CFG.Current.EnableWikiTools)
                {
                    if (ImGui.Button("Copy Selected Alias Name", buttonSize))
                    {
                        Clipboard.SetText(_selectedEntry.Name);
                    }
                    UIHelper.Tooltip("Copy the currently selected alias name directly to your clipboard.");

                    if (ImGui.Button("Copy Alias List", buttonSize))
                    {
                        var aliasList = "";

                        var entry = BaseEditor.BankHandler.ProjectEnums.Enums.List.Where(e => e.Name == _selectedEntry.Name).FirstOrDefault();

                        if (entry != null)
                        {
                            foreach (var opt in entry.Options)
                            {
                                var line = $"{opt.ID} {opt.Name}\n";

                                if (CFG.Current.EnableWikiTools)
                                {
                                    line = $"| {opt.ID} | {opt.Name} |\n";
                                }

                                aliasList = aliasList + line;
                            }

                            if (CFG.Current.EnableWikiTools)
                            {
                                aliasList = $"^ ID ^ Description ^\n{aliasList}";
                            }

                            Clipboard.SetText(aliasList);
                        }
                    }
                    UIHelper.Tooltip("Copy the aliases into a list: <ID> <Name>, saving to your clipboard.");
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
                                UIHelper.Tooltip("The display name of the enum option.");
                                //ImGui.Text($"Description");
                                //ImGui.InputTextMultiline($"##UpdateOptionDescription", ref _refUpdateOptionDescription, 255, inputSize);
                                //ImguiUtils.ShowHoverTooltip("A description of what this enum option is.");

                                if (ImGui.Button("Update Option", buttonSize))
                                {
                                    var newOption = new ProjectEnumOption().Clone(_selectedEntryOption);
                                    newOption.ID = _refUpdateOptionID;
                                    newOption.Name = _refUpdateOptionName;
                                    newOption.Description = _refUpdateOptionDescription;

                                    BaseEditor.BankHandler.ProjectEnums.UpdateEnumEntryOption(_selectedEntry, newOption);
                                    ReselectEntry = true;
                                }
                                if (ImGui.Button("Delete Option", buttonSize))
                                {
                                    BaseEditor.BankHandler.ProjectEnums.RemoveEnumEntryOption(_selectedEntry, _selectedEntryOption);
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
                        UIHelper.Tooltip("The numeric ID of the enum option.");
                        ImGui.Text($"Name");
                        ImGui.InputTextMultiline($"##NewOptionName", ref _refNewOptionName, 255, inputSize);
                        UIHelper.Tooltip("The display name of the enum option.");
                        //ImGui.Text($"Description");
                        //ImGui.InputTextMultiline($"##NewOptionDescription", ref _refNewOptionDescription, 255, inputSize);
                        //ImguiUtils.ShowHoverTooltip("A description of what this enum option is.");

                        if (ImGui.Button("Add Option", buttonSize))
                        {
                            if (_selectedEntry.Options.Where(e => e.ID == _refNewOptionID).Any())
                            {
                                MessageBox.Show($"Option with {_refNewOptionID} already exists for {_selectedEntry.DisplayName}", "Error", MessageBoxButtons.OK);
                            }
                            else
                            {
                                var newOption = new ProjectEnumOption();
                                newOption.ID = _refNewOptionID;
                                newOption.Name = _refNewOptionName;
                                newOption.Description = _refNewOptionDescription;

                                BaseEditor.BankHandler.ProjectEnums.UpdateEnumEntryOption(_selectedEntry, newOption);
                                ReselectEntry = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
