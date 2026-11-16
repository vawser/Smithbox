using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
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

    // One create toggle and text per group slot, by slot index (a group has up to 10 slots;
    // these were four fixed title/summary/description/effect fields)
    private const int MaxSlots = TextEntryGroupDefinition.MaxSlots;
    private readonly bool[] NewEntry_CreateSlot = Enumerable.Repeat(true, MaxSlots).ToArray();
    private readonly string[] NewEntry_SlotText = Enumerable.Repeat("", MaxSlots).ToArray();

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

            if (fmgEntryGroup == null)
                return;

            if (AllowParameterUpdate)
            {
                NewEntry_BaseID = entry.ID;

                if (fmgEntryGroup.SupportsGrouping)
                {
                    // Every slot the group has is created by default. Slots the newly selected
                    // entry doesn't have get empty text, rather than the previous entry's.
                    for (int i = 0; i < MaxSlots; i++)
                    {
                        var slot = i < fmgEntryGroup.Slots.Count ? fmgEntryGroup.Slots[i] : null;

                        NewEntry_CreateSlot[i] = slot != null;
                        NewEntry_SlotText[i] = slot?.Entry?.Text ?? "";
                    }
                }
                else
                {
                    NewEntry_BasicText = entry.Text;
                }
            }
        }
    }

    /// <summary>
    /// The slot the incremental template is added to: the title, or the first slot when the
    /// group has no title slot
    /// </summary>
    private static int TemplateSlotIndex(FmgEntryGroup group)
    {
        var title = group.Slots.FindIndex(s => s.IsTitle);

        return title >= 0 ? title : 0;
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
                // One section per slot of the group, labelled by what the slot means
                for (int i = 0; i < fmgEntryGroup.Slots.Count && i < MaxSlots; i++)
                {
                    var slot = fmgEntryGroup.Slots[i];

                    if (i > 0)
                        GUI.Spacer();

                    GUI.SimpleHeader(slot.Label, LOC.Get("TEXT_Creator_Header_Slot_TT", slot.Label));

                    ImGui.Checkbox($"{LOC.Get("TEXT_Creator_Checkbox_Create_Slot", slot.Label)}##createSlot{i}", ref NewEntry_CreateSlot[i]);
                    GUI.Tooltip(LOC.Get("TEXT_Creator_Checkbox_Create_Slot_TT", slot.Label));

                    var height = textboxHeight * DPI.UIScale() + ImGui.CalcTextSize(NewEntry_SlotText[i]).Y;
                    ImGui.InputTextMultiline($"##newEntryText_Slot{i}", ref NewEntry_SlotText[i],
                        TextContents.GetEditBufferSize(NewEntry_SlotText[i]), new Vector2(-1, height));
                }
            }
            else
            {
                GUI.Spacer();
                GUI.SimpleHeader(
                        LOC.Get("TEXT_Creator_Header_Basic"),
                        LOC.Get("TEXT_Creator_Header_Basic_TT"));

                var height = textboxHeight * DPI.UIScale() + ImGui.CalcTextSize(NewEntry_BasicText).Y;
                if (ImGui.InputTextMultiline($"##newEntryText_BasicText", ref NewEntry_BasicText, TextContents.GetEditBufferSize(NewEntry_BasicText), new Vector2(-1, height)))
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
                    // A copy: editing the rows used to change the loaded template itself
                    IncrementTemplate = entry.Entries?.Count > 0
                        ? new List<string>(entry.Entries)
                        : new List<string>() { "" };
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

        // IDs this batch has already used, so entries in the same batch can't collide
        ClaimedIds.Clear();

        // Inclusive on purpose: step 0 is the Base ID itself (normally the selected entry, so it's
        // skipped as taken), then Amount more steps. The bundled templates rely on this: ER
        // Infusions is Amount 12 with 13 rows, the first being the base weapon's empty prefix.
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

        // Every ID collided: nothing to create, so don't add an undo step that does nothing.
        // The modal stays open so the parameters can be adjusted.
        if (groupedActions.Count == 0)
            return;

        Parent.ActionManager.ExecuteAction(new FmgGroupedAction(groupedActions));
        ShowModal = false;
    }

    private bool HasIdCollision = false;

    /// <summary>
    /// (FMG, ID) pairs created by the current CreateEntries batch. The actions only run once the
    /// whole batch is built, so checking the FMG alone let entries in one batch share an ID.
    /// Keyed per FMG: a group's title, summary and description entries share their ID by design.
    /// </summary>
    private readonly HashSet<(FMG, int)> ClaimedIds = new();

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
                var templateSlot = TemplateSlotIndex(fmgEntryGroup);

                for (int i = 0; i < fmgEntryGroup.Slots.Count && i < MaxSlots; i++)
                {
                    if (!NewEntry_CreateSlot[i])
                        continue;

                    // The incremental template goes on the title (or the first slot) only
                    HandleNewSlotEntry(entry, fmgEntryGroup.Slots[i], NewEntry_SlotText[i], newID,
                        i == templateSlot ? incrementText : null, actions);
                }
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
    /// Create the entry for one group slot, in the slot's FMG. The incremental template text is
    /// added when given (only for the template slot). Replaces the four title/summary/
    /// description/effect handlers, as a group can have up to 10 slots of any meaning.
    /// </summary>
    private void HandleNewSlotEntry(FMG.Entry entry, FmgEntryGroupSlot slot, string slotText, int newId, string incrementText, List<EditorAction> actions)
    {
        if (slot?.Wrapper?.File == null)
            return;

        var sourceEntry = new FMG.Entry(slot.Wrapper.File, entry.ID, entry.Text);

        if (!IsAvailableID(sourceEntry, newId))
        {
            HasIdCollision = true;
            return;
        }

        var newText = slotText;

        if (AllowIncrementalTemplate && incrementText != null)
        {
            newText = ApplyTemplateAsPrefix ? $"{incrementText}{newText}" : $"{newText}{incrementText}";
        }

        actions.Add(CreateNewEntry(sourceEntry, newId, newText));
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

        // Already used by an earlier entry of this batch in the same FMG (e.g. ID Spacing 0)
        if (ClaimedIds.Contains((currentFmg, id)))
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

        ClaimedIds.Add((currentFmg, id));

        return new AddFmgEntry(Parent, Parent.Selection.SelectedContainerWrapper, entry, newEntry, id);
    }
}
