#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Models window: a tab of every character with an anibnd, and a tab of every object or asset whose container holds
/// one (found by reading the containers, on the side, once the tab is first shown), each with its alias. Clicking one
/// opens it; with unsaved edits, the editor asks first, since only one model is open at a time.
/// </summary>
public class AnimationCharacterListView
{
    private readonly AnimationEditorView View;
    private string Filter = "";
    private bool ExactFilter;

    // Aliases by model, looked up once each (the filter matches them too)
    private readonly Dictionary<(AnimatedModelKind, string), string> Aliases = new();

    // A model waiting on the discard prompt
    private (string Name, AnimatedModelKind Kind)? PendingOpen;
    private const string DiscardPromptId = "###animationEditorDiscardPrompt";

    // A tab to select on the next frame (a command opened a model of the other kind)
    private AnimatedModelKind? SelectTab;

    public AnimationCharacterListView(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>Shows the tab of a kind of model on the next frame</summary>
    public void ShowTab(AnimatedModelKind kind) => SelectTab = kind;

    public void Display()
    {
        GUI.TitleHeader(LOC.Get("ANIM_Window_Characters"), LOC.Get("ANIM_Characters_Header_TT"));
        EditorFilters.DisplayFramedListFilter("animationModelList_Filter", ref Filter, ref ExactFilter);

        if (View.IsLoading)
            GUI.WrappedText(LOC.Get("ANIM_Characters_Loading", View.LoadingName));
        else if (View.OpenError != null)
            GUI.WrappedText(LOC.Get("ANIM_Characters_Open_Failed", View.LoadingName, View.OpenError));

        if (!ImGui.BeginTabBar("animationModelTabs"))
            return;

        if (ImGui.BeginTabItem($"{LOC.Get("ANIM_Tab_Characters")}##animationCharacterTab", TabFlags(AnimatedModelKind.Character)))
        {
            DisplayCharacters();
            ImGui.EndTabItem();
        }

        var assetTab = View.Bank.UsesAssets ? LOC.Get("ANIM_Tab_Assets") : LOC.Get("ANIM_Tab_Objects");
        if (ImGui.BeginTabItem($"{assetTab}##animationAssetTab", TabFlags(AnimatedModelKind.Asset)))
        {
            DisplayAssets();
            ImGui.EndTabItem();
        }

        SelectTab = null;
        ImGui.EndTabBar();
    }

    private ImGuiTabItemFlags TabFlags(AnimatedModelKind kind) =>
        SelectTab == kind ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

    private void DisplayCharacters()
    {
        var characters = View.Bank.Characters;
        if (characters.Count == 0)
        {
            GUI.WrappedText(LOC.Get("ANIM_Characters_None"));
            return;
        }

        DisplayList("animationCharacterList", characters, AnimatedModelKind.Character);
    }

    private void DisplayAssets()
    {
        var bank = View.Bank;
        bank.StartAssetScan();

        if (!bank.AssetScanDone)
            GUI.WrappedText(LOC.Get("ANIM_Assets_Scanning", bank.AssetScanProgress, bank.AssetScanTotal));

        var assets = bank.Assets;
        if (assets.Count == 0)
        {
            if (bank.AssetScanDone)
                GUI.WrappedText(LOC.Get(bank.UsesAssets ? "ANIM_Assets_None" : "ANIM_Objects_None"));
            return;
        }

        DisplayList("animationAssetList", assets, AnimatedModelKind.Asset);
    }

    private void DisplayList(string id, IReadOnlyList<string> names, AnimatedModelKind kind)
    {
        if (!ImGui.BeginChild(id, default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        var bank = View.Bank;
        foreach (var name in names)
        {
            var alias = AliasOf(kind, name);
            if (Filter.Length > 0 && !EditorFilters.IsMatch(Filter, name, ExactFilter, alias))
                continue;

            var open = bank.Character is { } character && character.Kind == kind && character.Name == name;
            if (ImGui.Selectable($"{name}##{id}_{name}", open) && !open && !View.IsLoading)
                RequestOpen(name, kind);

            GUI.DisplayAlias(alias);
        }

        ImGui.EndChild();
    }

    private string AliasOf(AnimatedModelKind kind, string name)
    {
        if (!Aliases.TryGetValue((kind, name), out var alias))
        {
            Aliases[(kind, name)] = alias = kind == AnimatedModelKind.Asset
                ? AliasHelper.GetAssetAlias(View.Project, name)
                : AliasHelper.GetCharacterAlias(View.Project, name);
        }

        return alias;
    }

    /// <summary>Opens a model, asking first over unsaved work (the list, and other editors' commands)</summary>
    public void RequestOpen(string name, AnimatedModelKind kind)
    {
        // The prompt is opened by DisplayDiscardPrompt, outside this child window, so its ID is the one it is drawn with
        if (View.Bank.HasUnsavedChanges)
        {
            PendingOpen = (name, kind);
            return;
        }

        View.OpenCharacter(name, kind);
    }

    /// <summary>The prompt shown before another model is opened over unsaved edits (outside the list's child window)</summary>
    public void DisplayDiscardPrompt()
    {
        if (PendingOpen != null && !ImGui.IsPopupOpen(DiscardPromptId))
            ImGui.OpenPopup(DiscardPromptId);

        if (!ImGui.BeginPopupModal($"{LOC.Get("ANIM_Discard_Title")}{DiscardPromptId}", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        GUI.WrappedText(LOC.Get("ANIM_Discard_Text", View.Bank.Character?.Name ?? "", PendingOpen?.Name ?? ""));

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
        if (open && PendingOpen is { } pending)
            View.OpenCharacter(pending.Name, pending.Kind);

        PendingOpen = null;
        ImGui.CloseCurrentPopup();
    }
}
