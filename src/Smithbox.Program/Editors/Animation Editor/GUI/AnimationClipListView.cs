#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Clips window: the open character's clips, each with its alias. A clip with work not saved is marked *, one
/// saved but not exported to its anibnd +, and a clip added under a new ID (not exported yet) (new). A clip's context
/// menu duplicates, renames or deletes it, each an undoable step applied to the anibnd on Export.
/// </summary>
public class AnimationClipListView
{
    private readonly AnimationEditorView View;
    private string Filter = "";

    // Aliases by clip, looked up once for the open character (the filter matches them too)
    private readonly Dictionary<AnimationClipSource, string> Aliases = new();
    private AnimationCharacter? AliasesFor;

    // The Duplicate / Rename prompt: the clip it is for, and the name typed
    private enum NamePromptKind { Duplicate, Rename }
    private NamePromptKind PromptKind;
    private AnimationClipSource? PromptClip;
    private string PromptName = "";
    private string? PromptError;
    private bool PromptOpening;
    private const string NamePromptId = "###animationClipNamePrompt";

    public AnimationClipListView(AnimationEditorView view)
    {
        View = view;
    }

    public void ClearFilter() => Filter = "";

    /// <summary>Filters the list to a text (a clip shown from another editor), so it is in view</summary>
    public void SetFilter(string text) => Filter = text;

    public void Display()
    {
        var bank = View.Bank;

        if (bank.Character is not { } character)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        if (AliasesFor != character)
        {
            Aliases.Clear();
            AliasesFor = character;
        }

        if (!character.SetName.Equals(character.Name, StringComparison.OrdinalIgnoreCase))
            GUI.WrappedText(LOC.Get("ANIM_Clips_Shared_Set", character.SetName));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationClipFilter", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        // Used Only: the clips the time acts or behaviors use, plus every clip with work (so it never hides an edit)
        ImGui.Checkbox($"{LOC.Get("ANIM_Clips_Used_Only")}##animationClipsUsedOnly", ref CFG.Current.Interface.Interface_AnimationEditor_Clips_Used_Only);
        GUI.Tooltip(LOC.Get(character.TimeActs == null && character.BehaviorClips == null
            ? "ANIM_Clips_Used_Only_Unknown" : "ANIM_Clips_Used_Only_TT"));
        ImGui.SameLine();

        ImGui.TextDisabled(View.BatchClips.Count > 1
            ? LOC.Get("ANIM_Clips_Count_Batch", character.Clips.Count, View.BatchClips.Count)
            : LOC.Get("ANIM_Clips_Count", character.Clips.Count));

        Shortcuts();

        if (!ImGui.BeginChild("animationClipList", default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        // Characters have thousands of clips (the ER player over 1,300 in one anibnd), so only the rows in view are drawn
        var usedOnly = CFG.Current.Interface.Interface_AnimationEditor_Clips_Used_Only;
        var shown = Filter.Length == 0 && !usedOnly
            ? character.Clips
            : character.Clips.FindAll(c =>
                (!usedOnly || character.IsUsed(c.AnimationId) || c.IsNew || bank.IsEdited(c)) &&
                (Filter.Length == 0 || c.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase) ||
                 AliasOf(character, c).Contains(Filter, StringComparison.OrdinalIgnoreCase)));

        if (usedOnly)
            ImGui.TextDisabled(LOC.Get("ANIM_Clips_Used_Shown", shown.Count));

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        // Taken from the list after the loop, which a delete would change
        AnimationClipSource? deleting = null;

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var clip = shown[i];

                // * not saved, + saved but not exported to the anibnd
                var label = clip.IsNew ? $"{clip.Name} {LOC.Get("ANIM_Clips_New_Marker")}" : clip.Name;
                if (bank.IsUnsaved(clip))
                    label += " *";
                else if (bank.IsUnexported(clip))
                    label += " +";

                if (ImGui.Selectable($"{label}##animationClip_{clip.BinderId}", View.SelectedClip == clip || View.BatchClips.Contains(clip)))
                    Click(shown, i, clip);

                if (ImGui.IsItemHovered())
                    GUI.Tooltip($"{clip.BinderPath}\n{AnimationClipView.StateText(bank, clip)}");

                if (ContextMenu(clip))
                    deleting = clip;

                GUI.DisplayAlias(AliasOf(character, clip));
            }
        }

        clipper.End();
        ImGui.EndChild();

        if (deleting != null)
            Delete(deleting);
    }

    // The clip last clicked, where a Shift+click range starts
    private AnimationClipSource? Anchor;

    /// <summary>
    /// A click selects the clip (it plays). Ctrl+click also adds it to the batch the Batch tool acts on, or takes it
    /// out; Shift+click adds every clip from the last one clicked, in the list as filtered. A plain click ends the batch.
    /// </summary>
    private void Click(List<AnimationClipSource> shown, int index, AnimationClipSource clip)
    {
        var io = ImGui.GetIO();
        var batch = View.BatchClips;

        if (io.KeyShift && Anchor != null && shown.IndexOf(Anchor) is >= 0 and var from)
        {
            for (var i = Math.Min(from, index); i <= Math.Max(from, index); i++)
            {
                if (!batch.Contains(shown[i]))
                    batch.Add(shown[i]);
            }

            View.SelectClip(clip);
            return;
        }

        if (io.KeyCtrl)
        {
            // The clip selected before is the batch's first
            if (batch.Count == 0 && View.SelectedClip is { } current && current != clip)
                batch.Add(current);

            if (!batch.Remove(clip))
            {
                batch.Add(clip);
                View.SelectClip(clip);
            }

            Anchor = clip;
            return;
        }

        batch.Clear();
        Anchor = clip;
        View.SelectClip(clip);
    }

    private string AliasOf(AnimationCharacter character, AnimationClipSource clip)
    {
        if (!Aliases.TryGetValue(clip, out var alias))
            Aliases[clip] = alias = AliasHelper.GetAnimationAlias(View.Project, character.Name, clip.Name);
        return alias;
    }

    // ---- clip actions

    /// <summary>Ctrl+D duplicates and Delete deletes the selected clip, with the Clips window focused</summary>
    private void Shortcuts()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.AnimationEditor_Clips) || ImGui.GetIO().WantTextInput ||
            View.SelectedClip is not { } selected)
            return;

        if (InputManager.IsPressed(KeybindID.Duplicate))
            OpenPrompt(NamePromptKind.Duplicate, selected);
        else if (InputManager.IsPressed(KeybindID.Delete))
            Delete(selected);
    }

    /// <summary>A clip's context menu; true when Delete was chosen</summary>
    private bool ContextMenu(AnimationClipSource clip)
    {
        if (!ImGui.BeginPopupContextItem($"##animationClipContext_{clip.BinderId}"))
            return false;

        var delete = false;

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Clips_Duplicate")}##animationClipDuplicate", InputManager.GetHint(KeybindID.Duplicate)))
            OpenPrompt(NamePromptKind.Duplicate, clip);
        GUI.Tooltip(LOC.Get("ANIM_Clips_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Clips_Rename")}##animationClipRename"))
            OpenPrompt(NamePromptKind.Rename, clip);
        GUI.Tooltip(LOC.Get("ANIM_Clips_Rename_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Clips_Delete")}##animationClipDelete", InputManager.GetHint(KeybindID.Delete)))
            delete = true;
        GUI.Tooltip(LOC.Get("ANIM_Clips_Delete_TT"));

        ImGui.Separator();
        OpenInTimeActEditor(View, clip, menu: true);

        ImGui.EndPopup();
        return delete;
    }

    /// <summary>
    /// Open in Time Act Editor (W14): selects the clip's time act entry there (<c>tae/select</c>). Disabled, saying
    /// why, when the clip's ID has no entry (a new ID before Make Time Act Entry) or the time acts aren't read yet.
    /// As a menu item in the Clips list's context menu, as a button in the Clip tool.
    /// </summary>
    public static void OpenInTimeActEditor(AnimationEditorView view, AnimationClipSource clip, bool menu)
    {
        if (view.Bank.Character is not { } character)
            return;

        var hasEntry = character.TimeActs?.For(clip.AnimationId) != null;
        var label = $"{LOC.Get("ANIM_Open_Time_Act")}##animationOpenTimeAct";
        var clicked = menu ? ImGui.MenuItem(label, "", false, hasEntry) : Button(label, hasEntry);

        if (clicked)
            EditorCommandQueue.AddCommand($"tae/select/{character.SetName}/{clip.AnimationId}");

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(LOC.Get(hasEntry ? "ANIM_Open_Time_Act_TT" : "ANIM_Open_Time_Act_No_Entry"));

        static bool Button(string label, bool enabled)
        {
            ImGui.BeginDisabled(!enabled);
            var pressed = ImGui.Button(label);
            ImGui.EndDisabled();
            return pressed;
        }
    }

    private void OpenPrompt(NamePromptKind kind, AnimationClipSource clip)
    {
        PromptKind = kind;
        PromptClip = clip;
        PromptError = null;
        PromptName = kind == NamePromptKind.Duplicate ? View.Bank.NextFreeName(clip) ?? clip.Name : clip.Name;
        PromptOpening = true;
    }

    private void Delete(AnimationClipSource clip)
    {
        View.Execute(new AnimationDeleteClipAction(View.Bank, clip, LOC.Get("ANIM_Action_Delete", clip.Name)));

        if (View.SelectedClip == clip)
            View.SelectClip(null);
    }

    /// <summary>
    /// The Duplicate / Rename prompt (outside the list's child window). The name applies when the field is left after
    /// an edit (Enter, Tab or a click, not on Cancel) or with Apply.
    /// </summary>
    public void DisplayNamePrompt()
    {
        if (PromptOpening)
        {
            ImGui.OpenPopup(NamePromptId);
            PromptOpening = false;
        }

        var titleKey = PromptKind == NamePromptKind.Duplicate ? "ANIM_Clips_Duplicate_Title" : "ANIM_Clips_Rename_Title";
        if (!ImGui.BeginPopupModal($"{LOC.Get(titleKey)}{NamePromptId}", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (PromptClip is not { } clip || View.Bank.Character?.Clips.Contains(clip) != true)
        {
            ClosePrompt();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get(PromptKind == NamePromptKind.Duplicate ? "ANIM_Clips_Duplicate_Text" : "ANIM_Clips_Rename_Text", clip.Name));

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(220 * DPI.UIScale());
        ImGui.InputText("##animationClipPromptName", ref PromptName, 32);
        var committed = GUI.InputCommitted();

        if (PromptError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, PromptError);

        var apply = ImGui.Button($"{LOC.Get("ANIM_Clips_Prompt_Apply")}##animationClipPromptApply");

        ImGui.SameLine();
        var cancel = ImGui.Button($"{LOC.Get("ANIM_Discard_Cancel")}##animationClipPromptCancel");

        // The click that left the field was on Cancel
        if (ImGui.IsItemActive())
            committed = false;

        if (cancel)
            ClosePrompt();
        else if (apply || committed)
            ApplyPrompt(clip);

        ImGui.EndPopup();
    }

    private void ApplyPrompt(AnimationClipSource clip)
    {
        var bank = View.Bank;

        if (bank.Get(clip)?.Clone() is not { } content)
        {
            PromptError = LOC.Get("ANIM_Clip_Unreadable", clip.Name);
            return;
        }

        var created = bank.CreateClip(clip, PromptName, out PromptError);
        if (created == null)
            return;

        if (PromptKind == NamePromptKind.Duplicate)
        {
            View.Execute(new AnimationAddClipAction(bank, created, content,
                LOC.Get("ANIM_Action_Duplicate", created.Name, clip.Name)));
        }
        else
        {
            // A new clip under the new name with this one's content, and this one deleted, as one step
            var rename = new CompoundAction(new List<EditorAction>
            {
                new AnimationAddClipAction(bank, created, content, ""),
                new AnimationDeleteClipAction(bank, clip, "")
            })
            {
                DisplayName = LOC.Get("ANIM_Action_Rename", clip.Name, created.Name)
            };
            View.Execute(rename);
        }

        View.SelectClip(created);
        ClosePrompt();
    }

    private void ClosePrompt()
    {
        PromptClip = null;
        PromptError = null;
        ImGui.CloseCurrentPopup();
    }
}
