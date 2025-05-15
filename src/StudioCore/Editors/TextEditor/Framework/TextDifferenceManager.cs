using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor.Multiselection;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextDifferenceManager
{
    private TextEditorScreen Editor;
    public TextSelectionManager Selection;

    public TextDifferenceManager(TextEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Holds the row IDs for the current FMG where the row is unique to the project.
    /// </summary>
    private Dictionary<string, bool> AdditionCache = new();

    /// <summary>
    /// Holds the row IDs for the current FMG where the row text is different to vanilla.
    /// </summary>
    private Dictionary<string, bool> DifferenceCache = new();

    private bool CacheFilled = false;

    /// <summary>
    /// Generate difference truth for currently selected FMG
    /// </summary>
    public void TrackFmgDifferences(int setFmgId = -1)
    {
        CacheFilled = false;
        AdditionCache = new();
        DifferenceCache = new();

        if (Selection.SelectedContainerWrapper == null)
            return;

        // Leave empty if disabled
        if(!CFG.Current.TextEditor_IncludeVanillaCache)
        {
            return;
        }

        var containerCategory = Selection.SelectedContainerWrapper.ContainerDisplayCategory;
        var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;
        var containerName = Selection.SelectedContainerWrapper.FileEntry.Filename;

        // Fmg ID for comparison is selected FMG
        var fmgID = Selection.SelectedFmgWrapper.ID;

        // Set the ID to passed instead of selected if called from the FmgExporter
        if(setFmgId != -1)
        {
            fmgID = setFmgId;
        }

        // Get vanilla container and entries
        var vanillaContainer = Editor.Project.TextData.VanillaBank.Entries
            .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
            .Where(e => e.Value.FileEntry.Filename == containerName)
            .FirstOrDefault();

        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            vanillaContainer = Editor.Project.TextData.VanillaBank.Entries
            .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
            .Where(e => e.Value.ContainerDisplaySubCategory == containerSubCategory)
            .Where(e => e.Value.FileEntry.Filename == containerName)
            .FirstOrDefault();
        }

        if (vanillaContainer.Value == null)
            return;

        var vanillaFmg = vanillaContainer.Value.FmgWrappers
        .Where(e => e.ID == fmgID).FirstOrDefault();

        if (vanillaFmg == null)
            return;

        Dictionary<string, string> vanillaEntries = new();

        foreach(var entry in vanillaFmg.File.Entries)
        {
            // DS2
            if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                var key = $"{entry.ID}{entry.Parent.Name}{containerSubCategory}";
                if(vanillaEntries.ContainsKey(key))
                {
                    vanillaEntries[key] = entry.Text;
                }
                else
                {
                    vanillaEntries.Add(key, entry.Text);
                }
            }
            // Other
            else
            {
                var key = $"{entry.ID}";
                if (vanillaEntries.ContainsKey(key))
                {
                    vanillaEntries[key] = entry.Text;
                }
                else
                {
                    vanillaEntries.Add(key, entry.Text);
                }
            }
        }

        // Get primary container and enetries
        var primaryContainer = Editor.Project.TextData.PrimaryBank.Entries
            .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
            .Where(e => e.Value.FileEntry.Filename == containerName)
            .FirstOrDefault();

        if (primaryContainer.Value == null)
            return;

        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            primaryContainer = Editor.Project.TextData.PrimaryBank.Entries
            .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
            .Where(e => e.Value.ContainerDisplaySubCategory == containerSubCategory)
            .Where(e => e.Value.FileEntry.Filename == containerName)
            .FirstOrDefault();
        }

        var primaryFmg = primaryContainer.Value.FmgWrappers
        .Where(e => e.ID == fmgID).FirstOrDefault();

        if (primaryFmg == null)
            return;

        foreach (var entry in primaryFmg.File.Entries)
        {
            string entryId = $"{entry.ID}";

            // DS2
            if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                entryId = $"{entryId}{entry.Parent.Name}{containerSubCategory}";

                if (vanillaEntries.ContainsKey($"{entryId}"))
                {
                    var vanillaText = vanillaEntries[$"{entryId}"];

                    if (vanillaText != null)
                    {
                        if (!vanillaText.Equals(entry.Text))
                        {
                            if (!DifferenceCache.ContainsKey($"{entryId}"))
                            {
                                DifferenceCache.Add($"{entryId}", true);
                            }
                        }
                    }
                    else
                    {
                        // If project entry is not null, we can assume that it is different
                        if(entry.Text != null)
                        {
                            if (!DifferenceCache.ContainsKey($"{entryId}"))
                            {
                                DifferenceCache.Add($"{entryId}", true);
                            }
                        }
                    }
                }
                // Is a mod-unique row, there it is a difference
                else
                {
                    if (!AdditionCache.ContainsKey($"{entryId}"))
                    {
                        AdditionCache.Add($"{entryId}", true);
                    }

                    if (!DifferenceCache.ContainsKey($"{entryId}"))
                    {
                        DifferenceCache.Add($"{entryId}", true);
                    }
                }
            }
            // Other
            else
            {
                if (vanillaEntries.ContainsKey($"{entry.ID}"))
                {
                    var vanillaText = vanillaEntries[$"{entry.ID}"];

                    if (vanillaText != null)
                    {
                        if (!vanillaText.Equals(entry.Text))
                        {
                            if (!DifferenceCache.ContainsKey($"{entryId}"))
                            {
                                DifferenceCache.Add($"{entryId}", true);
                            }
                        }
                    }
                    else
                    {
                        // If project entry is not null, we can assume that it is different
                        if (entry.Text != null)
                        {
                            if (!DifferenceCache.ContainsKey($"{entryId}"))
                            {
                                DifferenceCache.Add($"{entryId}", true);
                            }
                        }
                    }
                }
                // Is a mod-unique row, there it is a difference
                else
                {
                    if (!AdditionCache.ContainsKey($"{entryId}"))
                    {
                        AdditionCache.Add($"{entryId}", true);
                    }

                    if (!DifferenceCache.ContainsKey($"{entryId}"))
                    {
                        DifferenceCache.Add($"{entryId}", true);
                    }
                }
            }
        }

        CacheFilled = true;
    }

    public bool IsDifferentToVanilla(FMG.Entry entry)
    {
        // Ignore if the cache is being filled, or the bank isn't loaded
        if (!CacheFilled)
            return false;

        if (entry == null)
            return false;

        var entryId = $"{entry.ID}";

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (Selection.SelectedContainerWrapper == null)
                return false;

            var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;

            entryId = $"{entryId}{entry.Parent.Name}{containerSubCategory}";

            if (DifferenceCache.ContainsKey(entryId))
            {
                return DifferenceCache[entryId];
            }
        }
        else
        {
            if (DifferenceCache.ContainsKey(entryId))
            {
                return DifferenceCache[entryId];
            }
        }

        return false;
    }

    public bool IsUniqueToProject(FMG.Entry entry)
    {
        // Ignore if the cache is being filled, or the bank isn't loaded
        if (!CacheFilled)
            return false;

        if (entry == null)
            return false;

        var entryId = $"{entry.ID}";

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (Selection.SelectedContainerWrapper == null)
                return false;

            var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;

            entryId = $"{entryId}{entry.Parent.Name}{containerSubCategory}";

            if (AdditionCache.ContainsKey(entryId))
            {
                return AdditionCache[entryId];
            }
        }
        // Other
        else
        {
            if (AdditionCache.ContainsKey(entryId))
            {
                return AdditionCache[entryId];
            }
        }

        return false;
    }
}
