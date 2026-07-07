using DotNext.Resources;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkAsyncThreadPool;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextEntryCreatorTool
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public bool ShowModal = false;

    private bool DisplayTemplatePopup = false;

    public TextEntryCreatorTool(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup($"{LOC.Get("TEXT_Creator_Modal_Name")}##textCreatorModal");
        }

        if (DisplayTemplatePopup)
        {
            ImGui.OpenPopup($"{LOC.Get("TEXT_Creator_Template_Modal_Name")}##templateModal");
        }

        EntryCreationMenu();
        TemplatePopupContextMenu();
    }

    public void EntryCreationMenu()
    {
        var entry = Parent.Selection._selectedFmgEntry;

        if (entry == null)
            return;

        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (fmgEntryGroup == null)
            return;

        if (ImGui.BeginPopupModal(
            $"{LOC.Get("TEXT_Creator_Modal_Name")}##textCreatorModal", 
            ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
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

    private int NewEntry_BaseID = -1;
    private int NewEntry_Amount = 12;
    private int NewEntry_IncrementID = 100;

    private bool NewEntry_CreateTitle = true;
    private bool NewEntry_CreateSummary = true;
    private bool NewEntry_CreateDescription = true;
    private bool NewEntry_CreateEffect = false;

    private string NewEntry_TitleText = "";
    private string NewEntry_SummaryText = "";
    private string NewEntry_DescriptionText = "";
    private string NewEntry_EffectText = "";
    private string NewEntry_BasicText = "";

    private bool AllowParameterUpdate = true;

    private bool AllowIncrementalTemplate = true;
    private bool ApplyTemplateAsPrefix = true;

    private List<string> IncrementTemplate = new()
    {
        "",
        "Heavy ",
        "Keen ",
        "Quality ",
        "Fire ",
        "Flame Art ",
        "Lightning ",
        "Sacred ",
        "Magic ",
        "Cold ",
        "Poison ",
        "Blood ",
        "Occult "
    };

    public void UpdateParameters(FMG.Entry entry)
    {
        if (entry != null && entry.Text != null)
        {
            var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

            if (AllowParameterUpdate)
            {
                NewEntry_BaseID = entry.ID;

                if (fmgEntryGroup.SupportsGrouping)
                {
                    NewEntry_CreateTitle = fmgEntryGroup.SupportsTitle;
                    NewEntry_CreateSummary = fmgEntryGroup.SupportsSummary;
                    NewEntry_CreateDescription = fmgEntryGroup.SupportsDescription;
                    NewEntry_CreateEffect = fmgEntryGroup.SupportsEffect;

                    if (fmgEntryGroup.Title != null && fmgEntryGroup.Title.Text != null)
                        NewEntry_TitleText = fmgEntryGroup.Title.Text;

                    if (fmgEntryGroup.Summary != null && fmgEntryGroup.Summary.Text != null)
                        NewEntry_SummaryText = fmgEntryGroup.Summary.Text;

                    if (fmgEntryGroup.Description != null && fmgEntryGroup.Description.Text != null)
                        NewEntry_DescriptionText = fmgEntryGroup.Description.Text;

                    if (fmgEntryGroup.Effect != null && fmgEntryGroup.Effect.Text != null)
                        NewEntry_EffectText = fmgEntryGroup.Effect.Text;
                }
                else
                {
                    NewEntry_BasicText = entry.Text;
                }
            }
        }
    }

    public void CreationMenu()
    {
        var entry = Parent.Selection._selectedFmgEntry;

        if (entry == null)
        {
            GUI.WrappedText(LOC.Get("TEXT_Creator_Select_Entry_First"));
            return;
        }

        if (entry.Text == null)
        {
            GUI.WrappedText(LOC.Get("TEXT_Creator_Select_Non_Null_First"));
            return;
        }

        var fmgEntryGroup = Parent.EntryGroupManager.GetEntryGroup(entry);

        if (fmgEntryGroup == null)
            return;

        var textboxHeight = 30;

        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_Actions"),
            LOC.Get("TEXT_Creator_Header_Actions_TT"));

        GUI.MultiButtonInput("creatorActions",
            "createEntries", 
            LOC.Get("TEXT_Creator_Action_Create_Entries"),
            LOC.Get("TEXT_Creator_Action_Create_Entries_TT"),
            CreateEntries);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_Parameters"),
            LOC.Get("TEXT_Creator_Header_Parameters_TT"));

        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_Base_ID"),
            LOC.Get("TEXT_Creator_Header_Base_ID_TT"));

        ImGui.InputInt("##entryBaseID", ref NewEntry_BaseID);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_Amount"),
            LOC.Get("TEXT_Creator_Header_Amount_TT"));

        ImGui.InputInt("##entryAmount", ref NewEntry_Amount);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_ID_Spacing"),
            LOC.Get("TEXT_Creator_Header_ID_Spacing_TT"));

        ImGui.InputInt("##entryIncrementID", ref NewEntry_IncrementID);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_Creator_Header_Options"),
            LOC.Get("TEXT_Creator_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Update_On_Selection")}##allowUpdateOnSelection", ref AllowParameterUpdate);
        GUI.Tooltip(LOC.Get("TEXT_Creator_Checkbox_Update_On_Selection_TT"));
        
        GUI.Spacer();
        if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Creator_Header_Entries")}##entriesHeader", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (fmgEntryGroup.SupportsGrouping)
            {
                if (fmgEntryGroup.SupportsTitle)
                {
                    GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Title"), 
                        LOC.Get("TEXT_Creator_Header_Title_TT"));

                    ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Create_Title")}##createTitle", ref NewEntry_CreateTitle);
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Checkbox_Create_Title_TT"));

                    var height = (textboxHeight + ImGui.CalcTextSize(NewEntry_TitleText).Y) * DPI.UIScale();
                    if (ImGui.InputTextMultiline($"##newEntryText_Title", ref NewEntry_TitleText, 2000, new Vector2(-1, height)))
                    {
                    }
                }

                if (fmgEntryGroup.SupportsSummary)
                {
                    GUI.Spacer();
                    GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Summary"),
                        LOC.Get("TEXT_Creator_Header_Summary_TT"));

                    ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Create_Summary")}##createSummary", ref NewEntry_CreateSummary);
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Checkbox_Create_Summary_TT"));

                    var height = (textboxHeight + ImGui.CalcTextSize(NewEntry_SummaryText).Y) * DPI.UIScale();
                    if (ImGui.InputTextMultiline($"##newEntryText_Summary", ref NewEntry_SummaryText, 2000, new Vector2(-1, height)))
                    {
                    }
                }

                if (fmgEntryGroup.SupportsDescription)
                {
                    GUI.Spacer();
                    GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Description"),
                        LOC.Get("TEXT_Creator_Header_Description_TT"));

                    ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Create_Description")}##createDescription", ref NewEntry_CreateDescription);
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Header_Description_TT"));

                    var height = (textboxHeight + ImGui.CalcTextSize(NewEntry_DescriptionText).Y) * DPI.UIScale();
                    if (ImGui.InputTextMultiline($"##newEntryText_Description", ref NewEntry_DescriptionText, 2000, new Vector2(-1, height)))
                    {
                    }
                }

                if (fmgEntryGroup.SupportsEffect)
                {
                    GUI.Spacer();
                    GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Effect"),
                        LOC.Get("TEXT_Creator_Header_Effect_TT"));

                    ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Create_Effect")}##createEffect", ref NewEntry_CreateEffect);
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Checkbox_Create_Effect_TT"));

                    var height = (textboxHeight + ImGui.CalcTextSize(NewEntry_EffectText).Y) * DPI.UIScale();
                    if (ImGui.InputTextMultiline($"##newEntryText_Effect", ref NewEntry_EffectText, 2000, new Vector2(-1, height)))
                    {
                    }
                }
            }
            else
            {
                GUI.Spacer();
                GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Basic"),
                        LOC.Get("TEXT_Creator_Header_Basic_TT"));

                var height = (textboxHeight + ImGui.CalcTextSize(NewEntry_BasicText).Y) * DPI.UIScale();
                if (ImGui.InputTextMultiline($"##newEntryText_BasicText", ref NewEntry_BasicText, 2000, new Vector2(-1, height)))
                {
                }
            }
        }

        GUI.Spacer();
        if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_Creator_Header_Incremental_Template")}##incrementalTemplateHeader", ImGuiTreeNodeFlags.DefaultOpen))
        {
            GUI.WrappedText(LOC.Get("TEXT_Creator_Incremental_Template_Hint"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_Creator_Template_Header_Options"),
                LOC.Get("TEXT_Creator_Template_Header_Options_TT"));

            ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Template_Enable")}##enableTemplate", ref AllowIncrementalTemplate);
            GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Enable_TT"));

            ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Template_Apply_Prefix")}##applyPrefix", ref ApplyTemplateAsPrefix);
            GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Apply_Prefix_TT"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_Creator_Template_Header_Templates"),
                LOC.Get("TEXT_Creator_Template_Header_Templates_TT"));

            // Add
            if (ImGui.Button($"{Icons.Plus}##incrementTemplate_Add", DPI.IconButtonSize))
            {
                IncrementTemplate.Add("");
            }
            GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Add_TT"));

            ImGui.SameLine();

            // Remove
            if (IncrementTemplate.Count < 2)
            {
                ImGui.BeginDisabled();

                if (ImGui.Button($"{Icons.Minus}##incrementTemplate_Remove_disabled", DPI.IconButtonSize))
                {
                    IncrementTemplate.RemoveAt(IncrementTemplate.Count - 1);
                }
                GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Remove_TT"));

                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"{Icons.Minus}##incrementTemplate_Remove", DPI.IconButtonSize))
                {
                    IncrementTemplate.RemoveAt(IncrementTemplate.Count - 1);
                }
                GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Remove_TT"));
            }

            ImGui.SameLine();

            // Reset
            if (ImGui.Button($"{LOC.Get("TEXT_Creator_Template_Reset_Action")}##incrementTemplate_Reset", DPI.SelectorButtonSize))
            {
                IncrementTemplate = new List<string>() { "" };
            }
            GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Reset_TT"));

            ImGui.SameLine();

            if (ImGui.ArrowButton("##templateSelect", ImGuiDir.Right))
            {
                DisplayTemplatePopup = true;
            }

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.BeginTable($"incrementEntryTbl", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthStretch);

                for (int i = 0; i < IncrementTemplate.Count; i++)
                {
                    var curText = IncrementTemplate[i];

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{i}");
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Template_Entry_TT", i));

                    ImGui.TableSetColumnIndex(1);

                    GUI.SetInputWidth();
                    if (ImGui.InputText($"##templateEntry_{i}", ref curText, 255))
                    {
                        IncrementTemplate[i] = curText;
                    }
                }

                ImGui.EndTable();
            }
        }
    }

    public void TemplatePopupContextMenu()
    {
        if (ImGui.BeginPopup(
            $"{LOC.Get("TEXT_Creator_Template_Modal_Name")}##templateModal",
            ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Creator_Template_Select_None")}##selectNone"))
            {
                DisplayTemplatePopup = false;
            }

            var templates = Project.Handler.TextData.Templates.OrderBy(e => e.OrderID);

            foreach(var entry in templates)
            {
                var displayName = LOC.Get(entry.LocalizationKey);

                if (ImGui.Selectable(displayName))
                {
                    IncrementTemplate = entry.Entries;
                    ApplyTemplateAsPrefix = entry.ApplyAsPrefix;
                    NewEntry_IncrementID = entry.IncrementID;
                    NewEntry_Amount = entry.Amount;

                    DisplayTemplatePopup = false;
                }
            }

            ImGui.EndPopup();
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

        var baseId = NewEntry_BaseID;
        var newID = NewEntry_BaseID;

        List<EditorAction> groupedActions = new();

        for (int i = 0; i <= NewEntry_Amount; i++)
        {
            var incrementText = IncrementTemplate.ElementAtOrDefault(i);

            var newActionList = CreateNewEntries(entry, fmgEntryGroup, newID, incrementText);
            newID = newID + NewEntry_IncrementID;

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
    public List<EditorAction> CreateNewEntries(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newID, string incrementText)
    {
        if (IsAvailableID(entry, newID))
        {
            List<EditorAction> actions = new List<EditorAction>();

            if (fmgEntryGroup.SupportsGrouping)
            {
                if(NewEntry_CreateTitle)
                    HandleNewTitleEntry(entry, fmgEntryGroup, newID, incrementText, actions);

                if (NewEntry_CreateSummary)
                    HandleNewSummaryEntry(entry, fmgEntryGroup, newID, actions);

                if (NewEntry_CreateDescription)
                    HandleNewDescriptionEntry(entry, fmgEntryGroup, newID, actions);

                if (NewEntry_CreateEffect)
                    HandleNewEffectEntry(entry, fmgEntryGroup, newID, actions);
            }
            else
            {
                var curFmg = entry.Parent;

                if (curFmg != null)
                {
                    HandleNewBasicEntry(entry, fmgEntryGroup, newID, incrementText, actions);
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
            Smithbox.LogError(this, LOC.Get("TEXT_Creator_Entries_ID_Collision", newID));

            HasIdCollision = false;
        }

        return null;
    }

    /// <summary>
    /// Handle title entry creation
    /// </summary>
    private void HandleNewTitleEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, string incrementText, List<EditorAction> actions)
    {
        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;

        // Title
        if (fmgEntryGroup.SupportsTitle)
        {
            TextFmgWrapper wrapper = Parent.EntryGroupManager.GetAssociatedTitleWrapper(selectedFmgWrapper.ID);
            var sourceEntry = new FMG.Entry(wrapper.File, entry.ID, entry.Text);

            if (IsAvailableID(sourceEntry, newId))
            {
                var newTitleText = NewEntry_TitleText;

                if (AllowIncrementalTemplate)
                {
                    if(ApplyTemplateAsPrefix)
                    {
                        newTitleText = $"{incrementText}{newTitleText}";
                    }
                    else
                    {
                        newTitleText = $"{newTitleText}{incrementText}";
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
                actions.Add(CreateNewEntry(sourceEntry, newId, NewEntry_SummaryText));
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
                actions.Add(CreateNewEntry(sourceEntry, newId, NewEntry_DescriptionText));
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
                actions.Add(CreateNewEntry(sourceEntry, newId, NewEntry_EffectText));
            }
            else
            {
                HasIdCollision = true;
            }
        }
    }
    private void HandleNewBasicEntry(FMG.Entry entry, FmgEntryGroup fmgEntryGroup, int newId, string incrementText, List<EditorAction> actions)
    {
        if (IsAvailableID(entry, newId))
        {
            var newText = NewEntry_BasicText;

            if (AllowIncrementalTemplate)
            {
                if (ApplyTemplateAsPrefix)
                {
                    newText = $"{incrementText}{newText}";
                }
                else
                {
                    newText = $"{newText}{incrementText}";
                }
            }

            actions.Add(CreateNewEntry(entry, newId, newText));
        }
        else
        {
            HasIdCollision = true;
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
