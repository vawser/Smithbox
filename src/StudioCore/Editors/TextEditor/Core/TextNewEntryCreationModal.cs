using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Prefabs;
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
    private TextNamingTemplateManager NamingTemplateManager;

    public bool ShowModal = false;

    private int _newId = -1;
    private string _newBasicText = "";

    private string _newTitleText = "";
    private string _newSummaryText = "";
    private string _newDescriptionText = "";
    private string _newEffectText = "";

    public TextNewEntryCreationModal(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        EntryGroupManager = screen.EntryGroupManager;
        NamingTemplateManager = screen.NamingTemplateManager;
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
        if (ImGui.BeginPopupModal("Create FMG Entry", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var entry = Selection._selectedFmgEntry;
            var fmgEntryGroup = EntryGroupManager.GetEntryGroup(entry);

            Vector2 buttonSize = new Vector2(520 * 0.5f, 24);

            if(ImGui.CollapsingHeader("Configuration", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"createConfigurationTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
                    //ImGui.TableHeadersRow();

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Creation Count");

                    ImGui.TableSetColumnIndex(1);

                    if (ImGui.InputInt("##creationCount", ref CFG.Current.TextEditor_CreationModal_CreationCount))
                    {
                        if (CFG.Current.TextEditor_CreationModal_CreationCount < 1)
                        {
                            CFG.Current.TextEditor_CreationModal_CreationCount = 1;
                        }
                    }
                    UIHelper.ShowHoverTooltip("The number of entries to create.");

                    // Row 2
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Increment Count");

                    ImGui.TableSetColumnIndex(1);

                    if (ImGui.InputInt("##incrementCount", ref CFG.Current.TextEditor_CreationModal_IncrementCount))
                    {
                        if (CFG.Current.TextEditor_CreationModal_IncrementCount < 1)
                        {
                            CFG.Current.TextEditor_CreationModal_IncrementCount = 1;
                        }
                    }
                    UIHelper.ShowHoverTooltip("The amount to increment the ID by for each created entry after the first.");

                    // Row 3
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Use Incremental Titling");

                    ImGui.TableSetColumnIndex(1);

                    if(ImGui.Checkbox("##useIncrementalTitling", ref CFG.Current.TextEditor_CreationModal_UseIncrementalTitling))
                    {
                        if(CFG.Current.TextEditor_CreationModal_UseIncrementalTitling)
                        {
                            CFG.Current.TextEditor_CreationModal_UseIncrementalNaming = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("Whether to use incremental titling, which applies the current creation count number to the end of the text for a Title entry.");

                    if (CFG.Current.TextEditor_CreationModal_UseIncrementalTitling)
                    {
                        // Row 4
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);

                        ImGui.Text("Incremental Prefix");

                        ImGui.TableSetColumnIndex(1);

                        ImGui.InputText("##incrementalTitlingPrefix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Prefix, 255);
                        UIHelper.ShowHoverTooltip("Characters to apply before the current creation number in the title when using Incremental Titling.");

                        // Row 5
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);

                        ImGui.Text("Incremental Postfix");

                        ImGui.TableSetColumnIndex(1);


                        ImGui.InputText("##incrementalTitlingPostfix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Postfix, 255);
                        UIHelper.ShowHoverTooltip("Characters to apply after the current creation number in the title when using Incremental Titling.");
                    }

                    // Row 6
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Use Incremental Naming");

                    ImGui.TableSetColumnIndex(1);

                    if(ImGui.Checkbox("##useIncrementalNaming", ref CFG.Current.TextEditor_CreationModal_UseIncrementalNaming))
                    {
                        if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
                        {
                            CFG.Current.TextEditor_CreationModal_UseIncrementalTitling = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("Whether to use incremental naming, which applies a template to the Title entry text.");

                    if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
                    {
                        // Row 7
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);

                        ImGui.Text("Naming Template");

                        ImGui.TableSetColumnIndex(1);

                        if(ImGui.BeginCombo("##incrementalNamingGeneratorList", CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template))
                        {
                            foreach(var (name, generator) in NamingTemplateManager.GeneratorDictionary)
                            {
                                if ((ProjectType)generator.ProjectType == Smithbox.ProjectType)
                                {
                                    if (ImGui.Selectable(name))
                                    {
                                        CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template = name;
                                    }
                                }
                            }

                            ImGui.EndCombo();
                        }
                        UIHelper.ShowHoverTooltip("The naming template to use.");
                    }

                    ImGui.EndTable();
                }
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
                var creationCount = CFG.Current.TextEditor_CreationModal_CreationCount;
                var incrementCount = CFG.Current.TextEditor_CreationModal_IncrementCount;
                var baseId = _newId;

                FmgEntryGeneratorBase generator = null;

                if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
                {
                    generator = NamingTemplateManager.GetGenerator(CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template);
                }

                List<EditorAction> groupedActions = new();

                for (int i = 0; i < creationCount; i++)
                {
                    var offset = 0;
                    if (i > 0)
                        offset = incrementCount * i;

                    var newActionList = CreateNewEntries(entry, fmgEntryGroup, baseId, i, generator, offset);
                    baseId = baseId + incrementCount;

                    if(newActionList != null)
                    {
                        foreach(var newEntry in newActionList)
                        {
                            groupedActions.Add(newEntry);
                        }
                    }
                }

                // Reverse add order so they are added to the entry list in the expected order (0, 100, 200, etc)
                groupedActions.Reverse();

                Screen.EditorActionManager.ExecuteAction(new FmgGroupedAction(groupedActions));
                ShowModal = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowModal = false;
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Create new entries based on passed parameters.
    /// </summary>
    public List<EditorAction> CreateNewEntries(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, int creationCount, 
        FmgEntryGeneratorBase generator, int offset)
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

                        if (CFG.Current.TextEditor_CreationModal_UseIncrementalTitling)
                        {
                            var prefix = CFG.Current.TextEditor_CreationModal_IncrementalTitling_Prefix;
                            var postfix = CFG.Current.TextEditor_CreationModal_IncrementalTitling_Postfix;

                            if (creationCount != 0)
                            {
                                newTitleText = $"{newTitleText} {prefix}{creationCount}{postfix}";
                            }
                        }

                        if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming && generator != null)
                        {
                            foreach(var row in generator.DefinitionList)
                            {
                                if(row.Offset == offset)
                                {
                                    if (row.PossessiveAdjust && newTitleText.Contains("'s"))
                                    {
                                        var owner = newTitleText.Split("'s")[0];
                                        var part = newTitleText.Split("'s")[1];

                                        newTitleText = $"{owner}'s {row.PrependText.Trim()}{part}{row.AppendText}";
                                    }
                                    else
                                    {
                                        newTitleText = $"{row.PrependText} {newTitleText}{row.AppendText}";
                                    }
                                }
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
                return actions;
            }
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

        return null;
    }

    /// <summary>
    /// Display the input tables for the passed input variables
    /// </summary>
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
