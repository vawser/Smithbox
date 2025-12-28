using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class LanguageSync
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public bool DisplayMassLanguageSyncWindow = false;
    public TextContainerCategory CurrentSourceLanguage = TextContainerCategory.None;

    public Dictionary<TextContainerCategory, bool> TargetLanguages = new();
    public bool SetupTargetLanguageDict = false;

    public bool DisplayTargetLanguagesSection = true;
    public bool DisplayOptionsLanguagesSection = true;

    public LanguageSync(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        if(!SetupTargetLanguageDict)
        {
            TargetLanguages = new();

            foreach (var entry in TextUtils.GetSupportedLanguages(Project))
            {
                var curType = (TextContainerCategory)entry;

                TargetLanguages.Add(curType, false);
            }

            SetupTargetLanguageDict = true;
        }

        DisplayMassSync();
    }

    public void DisplayMassSync()
    {
        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        if (DisplayMassLanguageSyncWindow)
        {
            var windowWidth = 500f;
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_MainBg);

            if (ImGui.Begin("Mass Language Sync##massLanguageSyncWindow", ref DisplayMassLanguageSyncWindow, flags))
            {
                UIHelper.SimpleHeader("srcLanguageHeader", "Source Language", "The language text to apply to the sync process against the target languages.", UI.Current.ImGui_AliasName_Text);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##sourceLanguagePicker", CurrentSourceLanguage.GetDisplayName()))
                {
                    foreach (var entry in TextUtils.GetSupportedLanguages(Project))
                    {
                        var curType = (TextContainerCategory)entry;

                        if (ImGui.Selectable($"{curType.GetDisplayName()}"))
                        {
                            CurrentSourceLanguage = curType;

                            // Set the new source language to false in the target language truth list
                            TargetLanguages[CurrentSourceLanguage] = false;
                        }
                    }
                    ImGui.EndCombo();
                }

                UIHelper.ConditionalHeader("targetLanguageHeader", "Target Languages", "The target languages to sync to.", UI.Current.ImGui_AliasName_Text, ref DisplayTargetLanguagesSection);

                if (CurrentSourceLanguage is TextContainerCategory.None)
                {
                    UIHelper.WrappedText("Select a source language first.");
                }
                else if(DisplayTargetLanguagesSection)
                {
                    if (SetupTargetLanguageDict)
                    {
                        foreach (var entry in TargetLanguages)
                        {
                            if (entry.Key == CurrentSourceLanguage)
                                continue;

                            var curState = entry.Value;
                            ImGui.Checkbox($"{entry.Key.GetDisplayName()}", ref curState);
                            if (ImGui.IsItemDeactivatedAfterEdit())
                            {
                                TargetLanguages[entry.Key] = curState;
                            }
                            UIHelper.Tooltip("If enabled, this language will be synced to.");
                        }
                    }
                }

                UIHelper.ConditionalHeader("optionsHeader", "Options", "The sync options.", UI.Current.ImGui_AliasName_Text, ref DisplayOptionsLanguagesSection);

                if (DisplayOptionsLanguagesSection)
                {
                    ImGui.Checkbox("Include Default Entries", ref CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries);
                    UIHelper.Tooltip("If enabled, all entries will be synced.");

                    ImGui.Checkbox("Include Added Entries", ref CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries);
                    UIHelper.Tooltip("If enabled, added entries will be synced.");

                    ImGui.Checkbox("Include Modified Entries", ref CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries);
                    UIHelper.Tooltip("If enabled, modified entries will be synced.");

                    ImGui.Checkbox("Apply Prefix to Synced Text", ref CFG.Current.TextEditor_LanguageSync_AddPrefix);
                    ImGui.InputText("##prefixTextInput", ref CFG.Current.TextEditor_LanguageSync_Prefix, 255);
                    UIHelper.Tooltip("The prefix to apply to the synced text.");
                }

                if (CurrentSourceLanguage is TextContainerCategory.None)
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Sync", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        DisplayMassLanguageSyncWindow = false;
                    }
                    ImGui.EndDisabled();

                    ImGui.SameLine();

                    if (ImGui.Button("Close", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        DisplayMassLanguageSyncWindow = false;
                    }
                }
                else
                {
                    if(ImGui.Button("Sync", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        ApplyMassSync();

                        DisplayMassLanguageSyncWindow = false;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Close", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        DisplayMassLanguageSyncWindow = false;
                    }
                }

                ImGui.End();
            }

            ImGui.PopStyleColor(1);
        }
    }

    public void ApplyMassSync()
    {
        List<EditorAction> actions = new();

        foreach(var entry in Editor.Project.TextData.PrimaryBank.Entries)
        {
            var container = entry.Value;

            if(container.ContainerDisplayCategory == CurrentSourceLanguage)
            {
                foreach (var tEntry in Editor.Project.TextData.PrimaryBank.Entries)
                {
                    var tContainer = tEntry.Value;

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
        Editor.EditorActionManager.ExecuteAction(compandAction);
    }

    public void DisplayMenubarOptions()
    {
        if (ImGui.Selectable("Open Mass Language Sync"))
        {
            DisplayMassLanguageSyncWindow = !DisplayMassLanguageSyncWindow;
        }

        ImGui.Separator();

        if (ImGui.MenuItem("Sync Default Entries"))
        {
            CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries = !CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries;
        }
        UIHelper.Tooltip($"If enabled, non-modified entries will be synced into another language.");
        UIHelper.ShowActiveStatus(CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries);

        if (ImGui.MenuItem("Sync Modified Entries"))
        {
            CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries = !CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries;
        }
        UIHelper.Tooltip($"If enabled, modified entries will be synced into another language.");
        UIHelper.ShowActiveStatus(CFG.Current.TextEditor_LanguageSync_IncludeModifiedEntries);

        if (ImGui.MenuItem("Sync Added Entries"))
        {
            CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries = !CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries;
        }
        UIHelper.Tooltip($"If enabled, added entries will be synced into another language.");
        UIHelper.ShowActiveStatus(CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries);

        if (ImGui.MenuItem("Apply Prefix to Added Entries"))
        {
            CFG.Current.TextEditor_LanguageSync_AddPrefix = !CFG.Current.TextEditor_LanguageSync_AddPrefix;
        }
        UIHelper.Tooltip($"If enabled, {CFG.Current.TextEditor_LanguageSync_Prefix} will be added to the text contents of any added entries that are synced into another language.");
        UIHelper.ShowActiveStatus(CFG.Current.TextEditor_LanguageSync_AddPrefix);
    }

    /// <summary>
    /// Options to sync to
    /// </summary>
    public void DisplaySyncOptions(int targetFmgId = -1)
    {
        var syncTargetWrapper = Editor.Selection.SelectedContainerWrapper;

        if (ImGui.BeginMenu("Sync With"))
        {
            foreach(var entry in Editor.Project.TextData.PrimaryBank.Entries)
            {
                var syncSrcWrapper = entry.Value;

                var proceed = false;

                if(syncSrcWrapper.FileEntry.Filename == syncTargetWrapper.FileEntry.Filename &&
                   syncSrcWrapper.ContainerDisplayCategory != syncTargetWrapper.ContainerDisplayCategory)
                {
                    proceed = true;
                }

                if (proceed)
                {
                    if (CFG.Current.TextEditor_LanguageSync_PrimaryOnly)
                    {
                        proceed = false;

                        if (syncSrcWrapper.ContainerDisplayCategory == CFG.Current.TextEditor_PrimaryCategory)
                        {
                            proceed = true;
                        }
                    }
                }

                // Not current selection, but the same file in a different category
                if (proceed)
                {
                    var displayName = syncSrcWrapper.FileEntry.Filename;

                    if (CFG.Current.TextEditor_DisplayCommunityContainerName)
                    {
                        // To get nice DS2 names, apply the FMG display name stuff on the container level
                        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            displayName = TextUtils.GetFmgDisplayName(Project, syncSrcWrapper, -1, syncSrcWrapper.FileEntry.Filename);
                        }
                        else
                        {
                            displayName = syncSrcWrapper.GetContainerDisplayName();
                        }
                    }

                    if (ImGui.Selectable($"{syncSrcWrapper.ContainerDisplayCategory.GetDisplayName()}: {displayName}##{syncSrcWrapper.FileEntry.Filename}"))
                    {
                        var actions = SyncLanguage(syncTargetWrapper, syncSrcWrapper, targetFmgId);

                        var compandAction = new CompoundAction(actions);
                        Editor.EditorActionManager.ExecuteAction(compandAction);
                    }
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Sync all unique changes from another category into this category.");
    }

    /// <summary>
    /// Sync currently selected category into chosen category
    /// </summary>
    private List<EditorAction> SyncLanguage(TextContainerWrapper syncTargetContainerWrapper, TextContainerWrapper syncSrcContainerWrapper, int targetFmgId = -1)
    {
        List<EditorAction> actions = new();

        foreach(var syncTargetWrapper in syncTargetContainerWrapper.FmgWrappers)
        {
            foreach (var syncSrcWrapper in syncSrcContainerWrapper.FmgWrappers)
            {
                if(targetFmgId == -1 && syncTargetWrapper.ID == syncSrcWrapper.ID 
                    || targetFmgId == syncTargetWrapper.ID && targetFmgId == syncSrcWrapper.ID)
                {
                    // Get the added/modified entries, comparing the sync source with its vanilla version
                    var vanillaSrcWrapper = GetVanillaSrcWrapper(syncSrcContainerWrapper, syncSrcWrapper);

                    if (vanillaSrcWrapper != null)
                    {
                        var result = FmgDifferenceFinder.GetFmgDifferenceResult(Editor, vanillaSrcWrapper, syncSrcWrapper);

                        foreach (var syncSrcEntry in syncSrcWrapper.File.Entries)
                        {
                            var parentEntry = syncTargetWrapper.File.Entries.First();

                            // Addition
                            if (CFG.Current.TextEditor_LanguageSync_IncludeUniqueEntries && result.AdditionCache.ContainsKey(syncSrcEntry.ID))
                            {
                                var newText = result.AdditionCache[syncSrcEntry.ID];

                                if (CFG.Current.TextEditor_LanguageSync_AddPrefix)
                                {
                                    newText = $"{CFG.Current.TextEditor_LanguageSync_Prefix}{newText}";
                                }

                                // Guard against multiple usage, if the entry ID already exists, we can assume the addition has already occured
                                if (!syncTargetWrapper.File.Entries.Any(e => e.ID == syncSrcEntry.ID))
                                {
                                    var newEntry = new FMG.Entry(syncTargetWrapper.File, syncSrcEntry.ID, newText);

                                    actions.Add(
                                        new AddFmgEntry(Editor, syncTargetContainerWrapper, parentEntry, newEntry, syncSrcEntry.ID));
                                }
                                // If already added, handle like a modified entry
                                else
                                {
                                    var targetEntry = syncTargetWrapper.File.Entries.FirstOrDefault(e => e.ID == syncSrcEntry.ID);

                                    if (targetEntry != null)
                                    {
                                        actions.Add(
                                            new ChangeFmgEntryText(Editor, syncTargetContainerWrapper, targetEntry, result.AdditionCache[syncSrcEntry.ID]));
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
                                        new ChangeFmgEntryText(Editor, syncTargetContainerWrapper, targetEntry, result.ModifiedCache[syncSrcEntry.ID]));
                                }
                            }
                            // Any
                            else if (CFG.Current.TextEditor_LanguageSync_IncludeDefaultEntries && result.DefaultCache.ContainsKey(syncSrcEntry.ID))
                            {
                                var targetEntry = syncTargetWrapper.File.Entries.FirstOrDefault(e => e.ID == syncSrcEntry.ID);

                                if (targetEntry != null)
                                {
                                    actions.Add(
                                        new ChangeFmgEntryText(Editor, syncTargetContainerWrapper, targetEntry, result.DefaultCache[syncSrcEntry.ID]));
                                }
                            }
                        }

                        actions.Add(new SortFmgList(Editor, syncTargetWrapper));
                    }
                }
            }
        }

        TaskLogs.AddLog($"Synced {syncTargetContainerWrapper.ContainerDisplayCategory.GetDisplayName()} {syncTargetContainerWrapper.FileEntry.Filename} with {syncSrcContainerWrapper.ContainerDisplayCategory.GetDisplayName()} version.");

        return actions;
    }

    public TextFmgWrapper GetVanillaSrcWrapper(TextContainerWrapper srcContainerWrapper, TextFmgWrapper srcWrapper)
    {
        var vanillaContainer = Editor.Project.TextData.VanillaBank.Entries
            .Where(e => e.Value.ContainerDisplayCategory == srcContainerWrapper.ContainerDisplayCategory)
            .Where(e => e.Value.FileEntry.Filename == srcContainerWrapper.FileEntry.Filename)
            .FirstOrDefault();

        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            vanillaContainer = Editor.Project.TextData.VanillaBank.Entries
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
