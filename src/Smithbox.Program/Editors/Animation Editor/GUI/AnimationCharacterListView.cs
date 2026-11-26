#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Characters window: every character with an anibnd, with its alias. Clicking one opens it; with unsaved edits,
/// the editor asks first, since only one character is open at a time.
/// </summary>
public class AnimationCharacterListView
{
    private readonly AnimationEditorView View;
    private string Filter = "";

    // Aliases by character id, looked up once each (the filter matches them too)
    private readonly Dictionary<string, string> Aliases = new();

    // A character waiting on the discard prompt
    private string? PendingOpen;
    private const string DiscardPromptId = "###animationEditorDiscardPrompt";

    public AnimationCharacterListView(AnimationEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        var bank = View.Bank;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationCharacterFilter", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        if (View.IsLoading)
            GUI.WrappedText(LOC.Get("ANIM_Characters_Loading", View.LoadingName));
        else if (View.OpenError != null)
            GUI.WrappedText(LOC.Get("ANIM_Characters_Open_Failed", View.LoadingName, View.OpenError));

        if (bank.Characters.Count == 0)
        {
            GUI.WrappedText(LOC.Get("ANIM_Characters_None"));
            return;
        }

        if (!ImGui.BeginChild("animationCharacterList", default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        foreach (var name in bank.Characters)
        {
            var alias = AliasOf(name);
            if (Filter.Length > 0 && !name.Contains(Filter, StringComparison.OrdinalIgnoreCase) &&
                !alias.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                continue;

            var open = bank.Character?.Name == name;
            if (ImGui.Selectable($"{name}##animationCharacter_{name}", open) && !open && !View.IsLoading)
                RequestOpen(name);

            GUI.DisplayAlias(alias);
        }

        ImGui.EndChild();
    }

    private string AliasOf(string name)
    {
        if (!Aliases.TryGetValue(name, out var alias))
            Aliases[name] = alias = AliasHelper.GetCharacterAlias(View.Project, name);
        return alias;
    }

    private void RequestOpen(string name)
    {
        // The prompt is opened by DisplayDiscardPrompt, outside this child window, so its ID is the one it is drawn with
        if (View.Bank.HasUnsavedChanges)
        {
            PendingOpen = name;
            return;
        }

        View.OpenCharacter(name);
    }

    /// <summary>The prompt shown before another character is opened over unsaved edits (outside the list's child window)</summary>
    public void DisplayDiscardPrompt()
    {
        if (PendingOpen != null && !ImGui.IsPopupOpen(DiscardPromptId))
            ImGui.OpenPopup(DiscardPromptId);

        if (!ImGui.BeginPopupModal($"{LOC.Get("ANIM_Discard_Title")}{DiscardPromptId}", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        GUI.WrappedText(LOC.Get("ANIM_Discard_Text", View.Bank.Character?.Name ?? "", PendingOpen ?? ""));

        if (ImGui.Button($"{LOC.Get("ANIM_Discard_Save")}##animationDiscardSave"))
        {
            View.Save();
            Finish(!View.Bank.HasUnsavedChanges);
        }

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Discard_Discard")}##animationDiscardDiscard"))
            Finish(true);

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Discard_Cancel")}##animationDiscardCancel"))
            Finish(false);

        ImGui.EndPopup();
    }

    private void Finish(bool open)
    {
        if (open && PendingOpen != null)
            View.OpenCharacter(PendingOpen);

        PendingOpen = null;
        ImGui.CloseCurrentPopup();
    }
}
