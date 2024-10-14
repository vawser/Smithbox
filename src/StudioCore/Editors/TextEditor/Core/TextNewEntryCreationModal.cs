using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextNewEntryCreationModal
{
    private TextEditorScreen Screen;
    private TextSelectionManager Selection;
    private TextEntryGroupManager EntryGroupManager;

    public bool ShowModal = false;

    private int _newId = -1;
    private string _newBasicText = "";

    private string _newTitleText = "";
    private string _newSummaryText = "";
    private string _newDescriptionText = "";
    private string _newEffectText = "";

    private int CreationCount = 1;
    private int IncrementAmount = 1;

    private bool UseIncrementalTitling = false;
    private string IncrementTitling_Prefix = "+";
    private string IncrementTitling_Postfix = "";

    public TextNewEntryCreationModal(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        EntryGroupManager = screen.EntryGroupManager;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup("Create FMG Entry");
        }

        EntryCreationMenu();
    }

    public void EntryCreationMenu()
    {
        if (ImGui.BeginPopupModal("Create FMG Entry", ref ShowModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            var entry = Selection._selectedFmgEntry;
            var fmgEntryGroup = EntryGroupManager.GetEntryGroup(entry);

            Vector2 buttonSize = new Vector2(520 * 0.5f, 24);

            if(ImGui.CollapsingHeader("Configuration", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Creation Count (e.g. 5 times)
                // Increment Amount (e.g. +10 to base ID)
                // Checkbox: Incremental Titles (add +X to the title text for each loop, X being the loop count, ignoring the first loop
                // Incremental Prefix
                // Increment Postfix
            }

            // Simple
            if (fmgEntryGroup == null)
            {
                DisplayEditTable(0, ref _newId, ref _newBasicText);
            }
            // Grouped
            else
            {
                if (fmgEntryGroup.Title != null)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Title");
                    ImGui.Separator();

                    DisplayEditTable(1, ref _newId, ref _newTitleText);
                }

                if (fmgEntryGroup.Summary != null)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Summary");
                    ImGui.Separator();

                    DisplayEditTable(2, ref _newId, ref _newSummaryText);
                }

                if (fmgEntryGroup.Description != null)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Description");
                    ImGui.Separator();

                    DisplayEditTable(3, ref _newId, ref _newDescriptionText);
                }

                if (fmgEntryGroup.Effect != null)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Effect");
                    ImGui.Separator();
                    DisplayEditTable(4, ref _newId, ref _newEffectText);
                }
            }

            if (ImGui.Button("Create", buttonSize))
            {
                var baseId = _newId;

                for (int i = 0; i < CreationCount; i++)
                {
                    CreateNewEntries(entry, fmgEntryGroup, baseId, i);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowModal = false;
            }

            ImGui.EndPopup();
        }
    }

    public void CreateNewEntries(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, int creationCount)
    {
        var displayIdError = false;

        if (IsAvailableID(entry, newId))
        {
            List<EditorAction> actions = new List<EditorAction>();

            // Check if entry type is considered 'grouped', if so create relevant entries in relevant FMGs
            // TODO: this means un-grouped rows won't automatically create grouped entries,
            // even if they perhaps should.
            if (fmgEntryGroup != null)
            {
                // Check to see if the entry group has a filled entry,
                // if it is null then we can ignore that 'type' when making the new entries

                if (fmgEntryGroup.Title != null)
                {
                    if (IsAvailableID(fmgEntryGroup.Title, newId))
                    {
                        var newTitleText = _newTitleText;

                        if (UseIncrementalTitling)
                        {
                            if(creationCount != 00)
                            {
                                newTitleText = $"{newTitleText} {IncrementTitling_Prefix}{creationCount}{IncrementTitling_Postfix}";
                            }
                        }

                        actions.Add(CreateNewEntry(fmgEntryGroup.Title, newId, newTitleText));
                    }
                    else
                    {
                        displayIdError = true;
                    }
                }

                if (fmgEntryGroup.Summary != null)
                {
                    if (IsAvailableID(fmgEntryGroup.Summary, newId))
                    {
                        actions.Add(CreateNewEntry(fmgEntryGroup.Summary, newId, _newSummaryText));
                    }
                    else
                    {
                        displayIdError = true;
                    }
                }

                if (fmgEntryGroup.Description != null)
                {
                    if (IsAvailableID(fmgEntryGroup.Description, newId))
                    {
                        actions.Add(CreateNewEntry(fmgEntryGroup.Description, newId, _newDescriptionText));
                    }
                    else
                    {
                        displayIdError = true;
                    }
                }

                if (fmgEntryGroup.Effect != null)
                {
                    if (IsAvailableID(fmgEntryGroup.Effect, newId))
                    {
                        actions.Add(CreateNewEntry(fmgEntryGroup.Effect, newId, _newEffectText));
                    }
                    else
                    {
                        displayIdError = true;
                    }
                }
            }
            else
            {
                var curFmg = entry.Parent;

                if (curFmg != null)
                {
                    actions.Add(CreateNewEntry(entry, newId, _newBasicText));
                }
            }

            if (actions.Count > 0)
            {
                var groupedAction = new FmgGroupedAction(actions);
                Screen.EditorActionManager.ExecuteAction(groupedAction);
            }

            ShowModal = false;
        }
        else
        {
            displayIdError = true;
        }

        // Display error message if ID is already in use by parent FMG
        if (displayIdError)
        {
            PlatformUtils.Instance.MessageBox(
                "ID is already in use or is invalid.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void DisplayEditTable(int index, ref int newId, ref string newText)
    {
        int tableWidth = 520;

        Vector2 buttonSize = new Vector2(520 * 0.5f, 24);

        var textboxHeight = 100;
        var height = (textboxHeight + ImGui.CalcTextSize(newText).Y) * DPI.GetUIScale();

        if (ImGui.BeginTable($"fmgNewTable{index}", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(tableWidth * 0.9f);
            if (ImGui.InputInt($"##newEntryIdInput{index}", ref newId))
            {
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Text");
            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##newEntryTextInput{index}", ref newText, 2000, new Vector2(-1, height)))
            {
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// Check if the passed ID is not already being used in the passed Entry's parent FMG
    /// </summary>
    public bool IsAvailableID(FMG.Entry currentEntry, int id)
    {
        var currentFmg = currentEntry.Parent;

        if(id < 0)
        {
            return false;
        }

        foreach (var entry in currentFmg.Entries)
        {
            if(entry.ID == id)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Creates a new FMG.Entry with the passed ID and text within the same FMG as the passed entry
    /// </summary>
    /// <param name="entry"></param>
    public AddFmgEntry CreateNewEntry(FMG.Entry entry, int id, string contents)
    {
        var currentFmg = entry.Parent;
        var newEntry = new FMG.Entry(currentFmg, id, contents);

        return new AddFmgEntry(entry, newEntry, id);
    }
}
