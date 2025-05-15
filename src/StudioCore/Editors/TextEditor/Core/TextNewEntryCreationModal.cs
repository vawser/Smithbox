using HKLib.hk2018.hkHashMapDetail;
using Hexa.NET.ImGui;
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
using StudioCore.Core;

namespace StudioCore.Editors.TextEditor;

public class TextNewEntryCreationModal
{
    private TextEditorScreen Editor;
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
        Editor = screen;
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

            var buttonSize = new Vector2(520 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
            var halfButtonSize = new Vector2(260 * DPI.GetUIScale(), 24 * DPI.GetUIScale());

            if (ImGui.CollapsingHeader("Configuration", ImGuiTreeNodeFlags.DefaultOpen))
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
                    UIHelper.Tooltip("The number of entries to create.");

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
                    UIHelper.Tooltip("The amount to increment the ID by for each created entry after the first.");

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
                    UIHelper.Tooltip("Whether to use incremental titling, which applies the current creation count number to the end of the text for a Title entry.");

                    if (CFG.Current.TextEditor_CreationModal_UseIncrementalTitling)
                    {
                        // Row 4
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);

                        ImGui.Text("Incremental Prefix");

                        ImGui.TableSetColumnIndex(1);

                        ImGui.InputText("##incrementalTitlingPrefix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Prefix, 255);
                        UIHelper.Tooltip("Characters to apply before the current creation number in the title when using Incremental Titling.");

                        // Row 5
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);

                        ImGui.Text("Incremental Postfix");

                        ImGui.TableSetColumnIndex(1);


                        ImGui.InputText("##incrementalTitlingPostfix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Postfix, 255);
                        UIHelper.Tooltip("Characters to apply after the current creation number in the title when using Incremental Titling.");
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
                    UIHelper.Tooltip("Whether to use incremental naming, which applies a template to the Title entry text.");

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
                                if ((ProjectType)generator.ProjectType == Editor.Project.ProjectType)
                                {
                                    if (ImGui.Selectable(name))
                                    {
                                        CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template = name;
                                    }
                                }
                            }

                            ImGui.EndCombo();
                        }
                        UIHelper.Tooltip("The naming template to use.");
                    }

                    ImGui.EndTable();
                }

                if (ImGui.Button("Inherit Text from Selection", buttonSize))
                {
                    _newId = Selection._selectedFmgEntry.ID;

                    if(fmgEntryGroup.SupportsGrouping)
                    {
                        if (fmgEntryGroup.Title != null)
                        {
                            _newTitleText = fmgEntryGroup.Title.Text;
                        }

                        if (fmgEntryGroup.Summary != null)
                        {
                            _newSummaryText = fmgEntryGroup.Summary.Text;
                        }

                        if (fmgEntryGroup.Description != null)
                        {
                            _newDescriptionText = fmgEntryGroup.Description.Text;
                        }

                        if (fmgEntryGroup.Effect != null)
                        {
                            _newEffectText = fmgEntryGroup.Effect.Text;
                        }
                    }
                    else
                    {
                        _newBasicText = Selection._selectedFmgEntry.Text;
                    }
                }
                UIHelper.Tooltip("Fill creation text input with contents of current selection.");
            }

            // Grouped
            if(fmgEntryGroup.SupportsGrouping)
            {
                if (fmgEntryGroup.SupportsTitle)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Title");
                    ImGui.Separator();

                    DisplayEditTable(1, ref _newId, ref _newTitleText);
                }

                if (fmgEntryGroup.SupportsSummary)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Summary");
                    ImGui.Separator();

                    DisplayEditTable(2, ref _newId, ref _newSummaryText);
                }

                if (fmgEntryGroup.SupportsDescription)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Description");
                    ImGui.Separator();

                    DisplayEditTable(3, ref _newId, ref _newDescriptionText);
                }

                if (fmgEntryGroup.SupportsEffect)
                {
                    ImGui.Separator();
                    UIHelper.WrappedText("Effect");
                    ImGui.Separator();
                    DisplayEditTable(4, ref _newId, ref _newEffectText);
                }
            }
            // Simple
            else
            {
                DisplayEditTable(0, ref _newId, ref _newBasicText);
            }

            if (ImGui.Button("Create", halfButtonSize))
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

                Editor.EditorActionManager.ExecuteAction(new FmgGroupedAction(groupedActions));
                ShowModal = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", halfButtonSize))
            {
                ShowModal = false;
            }

            ImGui.EndPopup();
        }
    }

    private bool HasIdCollision = false;

    /// <summary>
    /// Create new entries based on passed parameters.
    /// </summary>
    public List<EditorAction> CreateNewEntries(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, int creationCount, 
        FmgEntryGeneratorBase generator, int offset)
    {
        if (IsAvailableID(entry, newId))
        {
            List<EditorAction> actions = new List<EditorAction>();

            if (fmgEntryGroup.SupportsGrouping)
            {
                HandleNewTitleEntry(entry, fmgEntryGroup, newId, creationCount, generator, offset, actions);
                HandleNewSummaryEntry(entry, fmgEntryGroup, newId, actions);
                HandleNewDescriptionEntry(entry, fmgEntryGroup, newId, actions);
                HandleNewEffectEntry(entry, fmgEntryGroup, newId, actions);
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

        // Display error message if ID is already in use by parent FMG
        if (HasIdCollision)
        {
            PlatformUtils.Instance.MessageBox(
                "ID is already in use or is invalid.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            HasIdCollision = false;
        }

        return null;
    }

    /// <summary>
    /// Handle title entry creation
    /// </summary>
    private void HandleNewTitleEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, int creationCount,
        FmgEntryGeneratorBase generator, int offset, List<EditorAction> actions)
    {
        var selectedFmgWrapper = Selection.SelectedFmgWrapper;

        // Title
        if (fmgEntryGroup.SupportsTitle)
        {
            TextFmgWrapper wrapper = EntryGroupManager.GetAssociatedTitleWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
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
                    foreach (var row in generator.DefinitionList)
                    {
                        if (row.Offset == offset)
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

                actions.Add(CreateNewEntry(sourceEntry, newId, newTitleText));
            }
            else
            {
                HasIdCollision = true;
            }
        }
    }

    /// <summary>
    /// Handle summary entry creation
    /// </summary>
    private void HandleNewSummaryEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, List<EditorAction> actions)
    {
        var selectedFmgWrapper = Selection.SelectedFmgWrapper;

        // Summary
        if (fmgEntryGroup.SupportsSummary)
        {
            TextFmgWrapper wrapper = EntryGroupManager.GetAssociatedSummaryWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
            {
                actions.Add(CreateNewEntry(sourceEntry, newId, _newSummaryText));
            }
            else
            {
                HasIdCollision = true;
            }
        }
    }

    /// <summary>
    /// Handle description entry creation
    /// </summary>
    private void HandleNewDescriptionEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, List<EditorAction> actions)
    {
        var selectedFmgWrapper = Selection.SelectedFmgWrapper;

        // Description
        if (fmgEntryGroup.SupportsDescription)
        {
            TextFmgWrapper wrapper = EntryGroupManager.GetAssociatedDescriptionWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
            {
                actions.Add(CreateNewEntry(sourceEntry, newId, _newDescriptionText));
            }
            else
            {
                HasIdCollision = true;
            }
        }
    }

    /// <summary>
    /// Handle effect entry creation
    /// </summary>
    private void HandleNewEffectEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, List<EditorAction> actions)
    {
        var selectedFmgWrapper = Selection.SelectedFmgWrapper;

        // Description
        if (fmgEntryGroup.SupportsEffect)
        {
            TextFmgWrapper wrapper = EntryGroupManager.GetAssociatedEffectWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
            {
                actions.Add(CreateNewEntry(sourceEntry, newId, _newEffectText));
            }
            else
            {
                HasIdCollision = true;
            }
        }
    }

    /// <summary>
    /// Display the input tables for the passed input variables
    /// </summary>
    public void DisplayEditTable(int index, ref int newId, ref string newText)
    {
        // Fill null entries
        if(newText == null)
        {
            newText = "";
        }

        int tableWidth = 520;

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

        return new AddFmgEntry(Editor, Selection.SelectedContainerWrapper, entry, newEntry, id);
    }
}
