using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Translation Coverage: for each language other than the source, the entries that have source
/// text but are missing (or empty) in that language, and the ones whose text is the same as the
/// source (possibly untranslated). Worked out when Run is pressed, not every frame.
/// </summary>
public class TextCoverageReport
{
    private TextEditorView View;
    private ProjectEntry Project;

    private TextContainerCategory SourceCategory = TextContainerCategory.None;

    private class FileResult
    {
        public string ContainerName;
        public string FmgName;
        public int FmgId;

        /// <summary>For the jump to a missing entry, which only exists in the source</summary>
        public TextContainerCategory SourceCategory;
        public TextContainerCategory TargetCategory;

        public List<int> MissingIds = new();
        public List<int> IdenticalIds = new();
    }

    private class LanguageResult
    {
        public TextContainerCategory Category;
        public int Missing;
        public int Identical;
        public List<FileResult> Files = new();

        /// <summary>The project has this language's files (languages it lacks aren't listed)</summary>
        public bool HasFiles;
    }

    private List<LanguageResult> Results;

    public TextCoverageReport(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        // Bordered section, like the other tools
        ImGui.BeginChild("TranslationCoverageSection", ImGuiChildFlags.Borders);

        DisplayContents();

        ImGui.EndChild();
    }

    private void DisplayContents()
    {
        if (SourceCategory == TextContainerCategory.None)
            SourceCategory = CFG.Current.TextEditor.TextEditor_Primary_Category;

        GUI.WrappedText(LOC.Get("TEXT_Coverage_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXT_Coverage_Header_Source"), LOC.Get("TEXT_Coverage_Header_Source_TT"));

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##coverageSource", LOC.Get(SourceCategory.GetDisplayName())))
        {
            foreach (var language in TextUtils.GetSupportedLanguages(Project))
            {
                if (ImGui.Selectable($"{LOC.Get(language.GetDisplayName())}##coverageSrc{language}", language == SourceCategory))
                {
                    SourceCategory = language;
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.Button($"{LOC.Get("TEXT_Coverage_Run")}##coverageRun", DPI.StandardButtonSize))
        {
            Results = Build();
        }
        GUI.Tooltip(LOC.Get("TEXT_Coverage_Run_TT"));

        if (Results == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXT_Coverage_Header_Results"), LOC.Get("TEXT_Coverage_Header_Results_TT"));

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_Coverage_No_Languages"));
            return;
        }

        foreach (var language in Results)
        {
            var label = LOC.Get("TEXT_Coverage_Language_Row",
                LOC.Get(language.Category.GetDisplayName()), language.Missing, language.Identical);

            if (!ImGui.TreeNode($"{label}##coverageLang{language.Category}"))
                continue;

            if (language.Files.Count == 0)
            {
                GUI.WrappedText(LOC.Get("TEXT_Coverage_Complete"));
            }

            foreach (var file in language.Files)
            {
                var fileLabel = LOC.Get("TEXT_Coverage_File_Row",
                    file.ContainerName, file.FmgName, file.MissingIds.Count, file.IdenticalIds.Count);

                if (!ImGui.TreeNode($"{fileLabel}##coverageFile{language.Category}{file.ContainerName}{file.FmgId}"))
                    continue;

                DisplayIds(file, file.MissingIds, isMissing: true);
                DisplayIds(file, file.IdenticalIds, isMissing: false);

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }
    }

    /// <summary>
    /// The entry IDs of one list, clipped; clicking one jumps to it (a missing entry in the
    /// source language, as the target doesn't have it)
    /// </summary>
    private void DisplayIds(FileResult file, List<int> ids, bool isMissing)
    {
        if (ids.Count == 0)
            return;

        ImGui.TextDisabled(LOC.Get(isMissing ? "TEXT_Coverage_Missing_Header" : "TEXT_Coverage_Identical_Header"));

        var height = System.Math.Min(ids.Count, 10) * ImGui.GetTextLineHeightWithSpacing() + 8f * DPI.UIScale();

        ImGui.BeginChild($"##coverageIds{file.TargetCategory}{file.ContainerName}{file.FmgId}{isMissing}",
            new Vector2(0, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(ids.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var id = ids[i];

                if (ImGui.Selectable($"{id}##coverageId{i}"))
                {
                    var category = isMissing ? file.SourceCategory : file.TargetCategory;

                    EditorCommandQueue.AddCommand($"text/select/{category}/{file.ContainerName}/{file.FmgName}/{id}");
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private List<LanguageResult> Build()
    {
        var bank = Project.Handler.TextData.PrimaryBank;
        var isDS2 = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;
        var hideUnused = CFG.Current.TextEditor.TextEditor_Container_List_Hide_Unused_Containers;

        var languages = TextUtils.GetSupportedLanguages(Project)
            .Where(l => l != SourceCategory && l is not TextContainerCategory.None and not TextContainerCategory.Common)
            .ToList();

        var results = languages.ToDictionary(l => l, l => new LanguageResult { Category = l });

        var containers = bank.Containers.Values.ToList();

        foreach (var source in containers.Where(c => c.ContainerDisplayCategory == SourceCategory))
        {
            if (hideUnused && source.IsContainerUnused())
                continue;

            if (!EnsureLoaded(bank, source))
                continue;

            foreach (var target in containers)
            {
                if (!results.TryGetValue(target.ContainerDisplayCategory, out var languageResult))
                    continue;

                if (target.FileEntry.Filename != source.FileEntry.Filename)
                    continue;

                if (isDS2 && target.ContainerDisplaySubCategory != source.ContainerDisplaySubCategory)
                    continue;

                if (!EnsureLoaded(bank, target))
                    continue;

                languageResult.HasFiles = true;

                foreach (var sourceFmg in source.FmgWrappers)
                {
                    var targetFmg = target.FmgWrappers.FirstOrDefault(w => w.ID == sourceFmg.ID);

                    var targetById = new Dictionary<int, FMG.Entry>();
                    if (targetFmg != null)
                    {
                        foreach (var entry in targetFmg.File.Entries)
                            targetById.TryAdd(entry.ID, entry);
                    }

                    var file = new FileResult
                    {
                        ContainerName = source.FileEntry.Filename,
                        FmgName = sourceFmg.Name,
                        FmgId = sourceFmg.ID,
                        SourceCategory = SourceCategory,
                        TargetCategory = target.ContainerDisplayCategory
                    };

                    var seen = new HashSet<int>();

                    foreach (var sourceEntry in sourceFmg.File.Entries)
                    {
                        // Only entries with source text need translating; repeated IDs count once
                        if (string.IsNullOrEmpty(sourceEntry.Text) || !seen.Add(sourceEntry.ID))
                            continue;

                        if (!targetById.TryGetValue(sourceEntry.ID, out var targetEntry) || string.IsNullOrEmpty(targetEntry.Text))
                        {
                            file.MissingIds.Add(sourceEntry.ID);
                        }
                        else if (targetEntry.Text == sourceEntry.Text)
                        {
                            file.IdenticalIds.Add(sourceEntry.ID);
                        }
                    }

                    if (file.MissingIds.Count > 0 || file.IdenticalIds.Count > 0)
                    {
                        languageResult.Files.Add(file);
                        languageResult.Missing += file.MissingIds.Count;
                        languageResult.Identical += file.IdenticalIds.Count;
                    }
                }
            }
        }

        foreach (var result in results.Values)
        {
            result.Files = result.Files
                .OrderBy(f => f.ContainerName)
                .ThenBy(f => f.FmgId)
                .ToList();
        }

        return results.Values.Where(r => r.HasFiles).ToList();
    }

    private static bool EnsureLoaded(TextBank bank, TextContainerWrapper container)
    {
        if (container.FmgWrappers == null || container.FmgWrappers.Count == 0)
        {
            bank.LoadFmgWrappers(container);
        }

        return container.FmgWrappers?.Count > 0;
    }
}
