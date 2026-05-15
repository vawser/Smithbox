using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextEntryCreatorTool
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public bool ShowModal = false;

    private int _newId = -1;
    private string _newBasicText = "";

    private string _newTitleText = "";
    private string _newSummaryText = "";
    private string _newDescriptionText = "";
    private string _newEffectText = "";

    public TextEntryCreatorTool(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup("Text Entry Creator");
        }

        EntryCreationMenu();
    }

    public void EntryCreationMenu()
    {
        var entry = Parent.Selection._selectedFmgEntry;

        if (entry == null)
            return;

        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (fmgEntryGroup == null)
            return;

        if (ImGui.BeginPopupModal("Text Entry Creator", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {

            ImGui.BeginChild("TextEntryCreator", new Vector2(600f * DPI.UIScale(), 0f),
                ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

            CreationMenu();

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    public void DisplayTool()
    {
        ImGui.BeginChild("EntryCreatorSection", ImGuiChildFlags.Borders);

        CreationMenu();

        ImGui.EndChild();
    }

    public void CreationMenu()
    {
        var entry = Parent.Selection._selectedFmgEntry;

        if (entry == null)
            return;

        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (fmgEntryGroup == null)
            return;

        UIHelper.SimpleHeader("Number to Create", "The number of entries to create.");

        UIHelper.IntInput("CreationCount", ref CFG.Current.TextEditor_CreationModal_CreationCount);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (CFG.Current.TextEditor_CreationModal_CreationCount < 1)
            {
                CFG.Current.TextEditor_CreationModal_CreationCount = 1;
            }
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("ID Increment", "The ID increment applied to entries (after the first).");

        UIHelper.IntInput("IdIncrement", ref CFG.Current.TextEditor_CreationModal_IncrementCount);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (CFG.Current.TextEditor_CreationModal_IncrementCount < 1)
            {
                CFG.Current.TextEditor_CreationModal_IncrementCount = 1;
            }
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");

        if (ImGui.Checkbox("Enable Incremental Naming", ref CFG.Current.TextEditor_CreationModal_UseIncrementalNaming))
        {
            if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
            {
                CFG.Current.TextEditor_CreationModal_UseTemplateNaming = false;
            }
        }
        UIHelper.Tooltip("Whether to use incremental naming, which applies the current creation count number to the end of the text for a Title entry.");

        if (ImGui.Checkbox("Enable Template Naming", ref CFG.Current.TextEditor_CreationModal_UseTemplateNaming))
        {
            if (CFG.Current.TextEditor_CreationModal_UseTemplateNaming)
            {
                CFG.Current.TextEditor_CreationModal_UseIncrementalNaming = false;
            }
        }
        UIHelper.Tooltip("Whether to use incremental naming, which applies the current creation count number to the end of the text for a Title entry.");

        if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Incremental Prefix", "Characters to apply before the current creation number in the title when using incremental naming.");

            UIHelper.SinglelineTextInput("IncrementalPrefix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Prefix);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Incremental Postfix", "Characters to apply after the current creation number in the title when using incremental naming.");

            UIHelper.SinglelineTextInput("IncrementalPostfix", ref CFG.Current.TextEditor_CreationModal_IncrementalTitling_Postfix);
        }

        if (CFG.Current.TextEditor_CreationModal_UseTemplateNaming)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Naming Template", "The template to use for naming.");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##incrementalNamingGeneratorList", CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template))
            {
                foreach (var (name, generator) in Parent.NamingTemplateManager.GeneratorDictionary)
                {
                    if ((ProjectType)generator.ProjectType == Project.Descriptor.ProjectType)
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

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("creatorActions",
            "fillFromSelection", "Fill Text from Selection", "", FillTextFromSelection,
            "createEntries", "Create Entries", "", CreateEntries);

        // Grouped
        if (fmgEntryGroup.SupportsGrouping)
        {
            if (fmgEntryGroup.SupportsTitle)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader("Title Entry", "");

                DisplayEditTable(1, ref _newId, ref _newTitleText);
            }

            if (fmgEntryGroup.SupportsSummary)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader("Summary Entry", "");

                DisplayEditTable(2, ref _newId, ref _newSummaryText);
            }

            if (fmgEntryGroup.SupportsDescription)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader("Description Entry", "");

                DisplayEditTable(3, ref _newId, ref _newDescriptionText);
            }

            if (fmgEntryGroup.SupportsEffect)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader("Effect Entry", "");

                DisplayEditTable(4, ref _newId, ref _newEffectText);
            }
        }
        // Simple
        else
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Entry", "");

            DisplayEditTable(0, ref _newId, ref _newBasicText);
        }
    }

    public void FillTextFromSelection()
    {
        var entry = Parent.Selection._selectedFmgEntry;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (entry == null)
        {
            return;
        }

        if (fmgEntryGroup == null)
        {
            return;
        }

        _newId = Parent.Selection._selectedFmgEntry.ID;

        if (fmgEntryGroup.SupportsGrouping)
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
            _newBasicText = Parent.Selection._selectedFmgEntry.Text;
        }
    }

    public void CreateEntries()
    {
        var entry = Parent.Selection._selectedFmgEntry;
        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (entry == null)
        {
            return;
        }

        if (fmgEntryGroup == null)
        {
            return;
        }

        var creationCount = CFG.Current.TextEditor_CreationModal_CreationCount;
        var incrementCount = CFG.Current.TextEditor_CreationModal_IncrementCount;
        var baseId = _newId;

        FmgEntryGeneratorBase generator = null;

        if (CFG.Current.TextEditor_CreationModal_UseTemplateNaming)
        {
            generator = Parent.NamingTemplateManager.GetGenerator(CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template);
        }

        List<EditorAction> groupedActions = new();

        for (int i = 0; i < creationCount; i++)
        {
            var offset = 0;
            if (i > 0)
                offset = incrementCount * i;

            var newActionList = CreateNewEntries(entry, fmgEntryGroup, baseId, i, generator, offset);
            baseId = baseId + incrementCount;

            if (newActionList != null)
            {
                foreach (var newEntry in newActionList)
                {
                    groupedActions.Add(newEntry);
                }
            }
        }

        // Reverse add order so they are added to the entry list in the expected order (0, 100, 200, etc)
        groupedActions.Reverse();

        Parent.ActionManager.ExecuteAction(new FmgGroupedAction(groupedActions));
        ShowModal = false;
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
        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;

        // Title
        if (fmgEntryGroup.SupportsTitle)
        {
            TextFmgWrapper wrapper = Parent.EntryGroupManager.GetAssociatedTitleWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
            {
                var newTitleText = _newTitleText;

                if (CFG.Current.TextEditor_CreationModal_UseIncrementalNaming)
                {
                    var prefix = CFG.Current.TextEditor_CreationModal_IncrementalTitling_Prefix;
                    var postfix = CFG.Current.TextEditor_CreationModal_IncrementalTitling_Postfix;

                    if (creationCount != 0)
                    {
                        newTitleText = $"{newTitleText} {prefix}{creationCount}{postfix}";
                    }
                }

                if (CFG.Current.TextEditor_CreationModal_UseTemplateNaming && generator != null)
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
        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;

        // Summary
        if (fmgEntryGroup.SupportsSummary)
        {
            TextFmgWrapper wrapper = Parent.EntryGroupManager.GetAssociatedSummaryWrapper(selectedFmgWrapper.ID);
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
        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;

        // Description
        if (fmgEntryGroup.SupportsDescription)
        {
            TextFmgWrapper wrapper = Parent.EntryGroupManager.GetAssociatedDescriptionWrapper(selectedFmgWrapper.ID);
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
        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;

        // Description
        if (fmgEntryGroup.SupportsEffect)
        {
            TextFmgWrapper wrapper = Parent.EntryGroupManager.GetAssociatedEffectWrapper(selectedFmgWrapper.ID);
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

        float tableWidth = 520f;

        var textboxHeight = 100;
        var height = (textboxHeight + ImGui.CalcTextSize(newText).Y) * DPI.UIScale();

        if (ImGui.BeginTable($"fmgNewTable{index}", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth(tableWidth);
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

        return new AddFmgEntry(Parent, Parent.Selection.SelectedContainerWrapper, entry, newEntry, id);
    }
}
