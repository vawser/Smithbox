using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public class TextLanguageSyncTool
{
    public TextEditorView View;
    public ProjectEntry Project;

    public TextContainerCategory CurrentSourceLanguage = TextContainerCategory.None;

    public Dictionary<TextContainerCategory, bool> TargetLanguages = new();
    public bool SetupTargetLanguageDict = false;

    public bool DisplayTargetLanguagesSection = true;
    public bool DisplayOptionsLanguagesSection = true;

    public bool SetupSyncOptions = false;

    public TextLanguageSyncTool(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("LanguageSync", ImGuiChildFlags.Borders);

        UIHelper.WrappedText(LOC.Get("TEXT_LanguageSync_Hint"));

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_LanguageSync_Header_Source_Lang"),
            LOC.Get("TEXT_LanguageSync_Header_Source_Lang_TT"));

        var previewName = LOC.Get(CurrentSourceLanguage.GetDisplayName());

        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo("##sourceLanguagePicker", previewName))
        {
            foreach (var entry in TextUtils.GetSupportedLanguages(Project))
            {
                var curType = (TextContainerCategory)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName))
                {
                    CurrentSourceLanguage = curType;

                    // Set the new source language to false in the target language truth list
                    TargetLanguages[CurrentSourceLanguage] = false;
                }
            }
            ImGui.EndCombo();
        }

        UIHelper.Spacer();
        UIHelper.ConditionalHeader(
            LOC.Get("TEXT_LanguageSync_Header_Target_Lang"),
            LOC.Get("TEXT_LanguageSync_Header_Target_Lang_TT"),
            ref DisplayTargetLanguagesSection);

        if (CurrentSourceLanguage is TextContainerCategory.None)
        {
            UIHelper.WrappedText(LOC.Get("TEXT_LanguageSync_Select_Source_Lang"));
        }
        else if (DisplayTargetLanguagesSection)
        {
            if (SetupTargetLanguageDict)
            {
                foreach (var entry in TargetLanguages)
                {
                    if (entry.Key == CurrentSourceLanguage)
                        continue;

                    var displayName = LOC.Get(entry.Key.GetDisplayName());

                    var curState = entry.Value;
                    ImGui.Checkbox($"{displayName}##langToggle{entry.Key}", ref curState);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        TargetLanguages[entry.Key] = curState;
                    }
                    UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Lang_Select_TT"));
                }
            }
        }

        UIHelper.Spacer();
        UIHelper.ConditionalHeader(
            LOC.Get("TEXT_LanguageSync_Header_Options"),
            LOC.Get("TEXT_LanguageSync_Header_Options_TT"),
            ref DisplayOptionsLanguagesSection);

        if (DisplayOptionsLanguagesSection)
        {
            ImGui.Checkbox(
                $"{LOC.Get("TEXT_LanguageSync_Checkbox_Include_Default")}##includeDefault",
                ref CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries);
            UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Checkbox_Include_Default_TT"));

            ImGui.Checkbox(
                $"{LOC.Get("TEXT_LanguageSync_Checkbox_Include_Added")}##includeAdded",
                ref CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries);
            UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Checkbox_Include_Added_TT"));

            ImGui.Checkbox(
                $"{LOC.Get("TEXT_LanguageSync_Checkbox_Include_Modified")}##includeModified",
                ref CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries);
            UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Checkbox_Include_Modified_TT"));

            ImGui.Checkbox(
                $"{LOC.Get("TEXT_LanguageSync_Checkbox_Apply_Prefix")}##applyPrefix",
                ref CFG.Current.TextEditor_Language_Sync_Apply_Prefix);
            UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Checkbox_Apply_Prefix_TT"));
        }

        if (CFG.Current.TextEditor_Language_Sync_Apply_Prefix)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_LanguageSync_Header_Sync_Prefix"),
                LOC.Get("TEXT_LanguageSync_Header_Sync_Prefix_TT"));

            UIHelper.SinglelineTextInput("##prefixTextInput", ref CFG.Current.TextEditor_Language_Sync_Prefix);
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_LanguageSync_Header_Actions"),
            LOC.Get("TEXT_LanguageSync_Header_Actions_TT"));

        UIHelper.MultiButtonInput("languageSyncActions",
            "syncLanguages", 
            LOC.Get("TEXT_LanguageSync_Action_Sync"),
            LOC.Get("TEXT_LanguageSync_Action_Sync_TT"),
            ApplyMassSync);

        ImGui.EndChild();
    }

    public void OnGui()
    {
        if (!SetupTargetLanguageDict)
        {
            TargetLanguages = new();

            foreach (var entry in TextUtils.GetSupportedLanguages(Project))
            {
                var curType = (TextContainerCategory)entry;

                TargetLanguages.Add(curType, false);
            }

            SetupTargetLanguageDict = true;
        }
    }

    public void ApplyMassSync()
    {
        if (CurrentSourceLanguage is TextContainerCategory.None)
        {
            Smithbox.LogError<TextLanguageSyncTool>(LOC.Get("No source language has been selected."));
            return; 
        }

        List<EditorAction> actions = new();

        foreach (var entry in Project.Handler.TextData.PrimaryBank.Containers)
        {
            var container = entry.Value;

            if (entry.Value.FmgWrappers == null)
            {
                Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(entry.Value);
            }

            if (container.ContainerDisplayCategory == CurrentSourceLanguage)
            {
                foreach (var tEntry in Project.Handler.TextData.PrimaryBank.Containers)
                {
                    var tContainer = tEntry.Value;

                    if (tEntry.Value.FmgWrappers == null)
                    {
                        Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(tEntry.Value);
                    }

                    if (TargetLanguages.ContainsKey(tContainer.ContainerDisplayCategory))
                    {
                        if (TargetLanguages[tContainer.ContainerDisplayCategory])
                        {
                            if (container.FileEntry.Filename == tContainer.FileEntry.Filename)
                            {
                                var curActions = SyncLanguage(tContainer, container);
                                foreach (var action in curActions)
                                {
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                }
            }
        }

        var compandAction = new CompoundAction(actions);
        View.ActionManager.ExecuteAction(compandAction);
    }

    public void DisplaySyncOptions(int targetFmgId = -1)
    {
        var syncTargetWrapper = View.Selection.SelectedContainerWrapper;

        if (syncTargetWrapper == null)
            return;

        if (ImGui.BeginMenu($"{LOC.Get("TEXT_LanguageSync_Header_Sync_With")}##syncWithMenuHeader"))
        {
            foreach (var entry in Project.Handler.TextData.PrimaryBank.Containers)
            {
                var syncSrcWrapper = entry.Value;

                var proceed = false;

                if (syncSrcWrapper.FileEntry.Filename == syncTargetWrapper.FileEntry.Filename &&
                   syncSrcWrapper.ContainerDisplayCategory != syncTargetWrapper.ContainerDisplayCategory)
                {
                    proceed = true;
                }

                if (proceed)
                {
                    if (CFG.Current.TextEditor_Language_Sync_Display_Primary_Only)
                    {
                        proceed = false;

                        if (syncSrcWrapper.ContainerDisplayCategory == CFG.Current.TextEditor_Primary_Category)
                        {
                            proceed = true;
                        }
                    }
                }

                if (entry.Value.FmgWrappers == null)
                {
                    _ = Project.Handler.TextData.PrimaryBank.LoadFmgWrappersAsync(entry.Value);
                }

                if (syncSrcWrapper.FmgWrappers == null)
                    proceed = false;

                // Not current selection, but the same file in a different category
                if (proceed)
                {
                    var displayName = syncSrcWrapper.FileEntry.Filename;

                    if (CFG.Current.TextEditor_Container_List_Display_Community_Names)
                    {
                        // To get nice DS2 names, apply the FMG display name stuff on the container level
                        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            displayName = TextUtils.GetFmgDisplayName(Project, syncSrcWrapper, -1, syncSrcWrapper.FileEntry.Filename);
                        }
                        else
                        {
                            displayName = syncSrcWrapper.GetContainerDisplayName();
                        }
                    }

                    var selectableName = LOC.Get(syncSrcWrapper.ContainerDisplayCategory.GetDisplayName());

                    if (ImGui.Selectable($"{selectableName}: {displayName}##{syncSrcWrapper.FileEntry.Filename}"))
                    {
                        var actions = SyncLanguage(syncTargetWrapper, syncSrcWrapper, targetFmgId);

                        var compandAction = new CompoundAction(actions);
                        View.ActionManager.ExecuteAction(compandAction);
                    }
                    UIHelper.Tooltip(
                        LOC.Get("TEXT_LanguageSync_Action_Sync_TT", selectableName));
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("TEXT_LanguageSync_Header_Sync_With_TT"));
    }

    private List<EditorAction> SyncLanguage(TextContainerWrapper syncTargetContainerWrapper, TextContainerWrapper syncSrcContainerWrapper, int targetFmgId = -1)
    {
        List<EditorAction> actions = new();

        if (syncTargetContainerWrapper.FmgWrappers == null || syncTargetContainerWrapper.FmgWrappers.Count == 0)
        {
            Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(syncTargetContainerWrapper);
        }

        if (syncSrcContainerWrapper.FmgWrappers == null || syncSrcContainerWrapper.FmgWrappers.Count == 0)
        {
            Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(syncSrcContainerWrapper);
        }

        foreach (var syncTargetWrapper in syncTargetContainerWrapper.FmgWrappers)
        {

            foreach (var syncSrcWrapper in syncSrcContainerWrapper.FmgWrappers)
            {
                if (targetFmgId == -1 && syncTargetWrapper.ID == syncSrcWrapper.ID
                    || targetFmgId == syncTargetWrapper.ID && targetFmgId == syncSrcWrapper.ID)
                {
                    // Get the added/modified entries, comparing the sync source with its vanilla version
                    var vanillaSrcWrapper = GetVanillaSrcWrapper(syncSrcContainerWrapper, syncSrcWrapper);

                    if (vanillaSrcWrapper != null)
                    {
                        var result = FmgDifferenceFinder.GetFmgDifferenceResult(View, vanillaSrcWrapper, syncSrcWrapper);

                        foreach (var syncSrcEntry in syncSrcWrapper.File.Entries)
                        {
                            var parentEntry = syncTargetWrapper.File.Entries.First();

                            // Addition
                            if (CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries && result.AdditionCache.ContainsKey(syncSrcEntry.ID))
                            {
                                var newText = result.AdditionCache[syncSrcEntry.ID];

                                // Guard against multiple usage, if the entry ID already exists, we can assume the addition has already occured
                                if (!syncTargetWrapper.File.Entries.Any(e => e.ID == syncSrcEntry.ID))
                                {
                                    var newEntry = new FMG.Entry(syncTargetWrapper.File, syncSrcEntry.ID, newText);

                                    actions.Add(
                                        new AddFmgEntry(View, syncTargetContainerWrapper, parentEntry, newEntry, syncSrcEntry.ID));
                                }
                                // If already added, handle like a modified entry
                                else
                                {
                                    var targetEntry = syncTargetWrapper.File.Entries.FirstOrDefault(e => e.ID == syncSrcEntry.ID);

                                    if (targetEntry != null)
                                    {
                                        actions.Add(
                                            new ChangeFmgEntryText(View, syncTargetContainerWrapper, targetEntry, result.AdditionCache[syncSrcEntry.ID], true));
                                    }
                                }
                            }
                            // Modified
                            else if (CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries && result.ModifiedCache.ContainsKey(syncSrcEntry.ID))
                            {
                                var targetEntry = syncTargetWrapper.File.Entries.FirstOrDefault(e => e.ID == syncSrcEntry.ID);

                                if (targetEntry != null)
                                {
                                    actions.Add(
                                        new ChangeFmgEntryText(View, syncTargetContainerWrapper, targetEntry, result.ModifiedCache[syncSrcEntry.ID], true));
                                }
                            }
                            // Any
                            else if (CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries && result.DefaultCache.ContainsKey(syncSrcEntry.ID))
                            {
                                var targetEntry = syncTargetWrapper.File.Entries.FirstOrDefault(e => e.ID == syncSrcEntry.ID);

                                if (targetEntry != null)
                                {
                                    actions.Add(
                                        new ChangeFmgEntryText(View, syncTargetContainerWrapper, targetEntry, result.DefaultCache[syncSrcEntry.ID], true));
                                }
                            }
                        }

                        actions.Add(new SortFmgList(syncTargetWrapper));
                    }
                }
            }
        }

        var syncTargetDisplayName = LOC.Get(syncTargetContainerWrapper.ContainerDisplayCategory.GetDisplayName());
        var syncSrcDisplayName = LOC.Get(syncSrcContainerWrapper.ContainerDisplayCategory.GetDisplayName());

        Smithbox.Log(this,
            LOC.Get("TEXT_LanguageSync_Sync_Operation_PASS",
            syncTargetDisplayName, syncTargetContainerWrapper.FileEntry.Filename, syncSrcDisplayName));

        return actions;
    }

    public TextFmgWrapper GetVanillaSrcWrapper(TextContainerWrapper srcContainerWrapper, TextFmgWrapper srcWrapper)
    {
        var vanillaContainer = Project.Handler.TextData.VanillaBank.Containers
            .Where(e => e.Value.ContainerDisplayCategory == srcContainerWrapper.ContainerDisplayCategory)
            .Where(e => e.Value.FileEntry.Filename == srcContainerWrapper.FileEntry.Filename)
            .FirstOrDefault();

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            vanillaContainer = Project.Handler.TextData.VanillaBank.Containers
            .Where(e => e.Value.ContainerDisplayCategory == srcContainerWrapper.ContainerDisplayCategory)
            .Where(e => e.Value.ContainerDisplaySubCategory == srcContainerWrapper.ContainerDisplaySubCategory)
            .Where(e => e.Value.FileEntry.Filename == srcContainerWrapper.FileEntry.Filename)
            .FirstOrDefault();
        }

        if (vanillaContainer.Value == null)
            return null;

        var vanillaFmg = vanillaContainer.Value.FmgWrappers
        .Where(e => e.ID == srcWrapper.ID).FirstOrDefault();

        if (vanillaFmg == null)
            return null;

        return vanillaFmg;
    }
}
