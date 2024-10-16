using SoulsFormats;
using StudioCore.Core.Project;
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
    private TextEditorScreen Screen;
    public TextSelectionManager Selection;

    public TextDifferenceManager(TextEditorScreen screen)
    {
        Screen = screen;
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

    /// <summary>
    /// Generate difference truth for currently selected FMG
    /// </summary>
    public void TrackFmgDifferences()
    {
        AdditionCache = new();
        DifferenceCache = new();

        var containerCategory = Selection.SelectedContainer.Category;
        var containerSubCategory = Selection.SelectedContainer.SubCategory;
        var containerName = Selection.SelectedContainer.Name;
        var fmgID = Selection.SelectedFmgInfo.ID;

        if (TextBank.VanillaBankLoaded)
        {
            var vanillaContainer = TextBank.VanillaFmgBank
                .Where(e => e.Value.Category == containerCategory)
                .Where(e => e.Value.Name == containerName)
                .FirstOrDefault();

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                vanillaContainer = TextBank.VanillaFmgBank
                .Where(e => e.Value.Category == containerCategory)
                .Where(e => e.Value.SubCategory == containerSubCategory)
                .Where(e => e.Value.Name == containerName)
                .FirstOrDefault();
            }

            var vanillaFmg = vanillaContainer.Value.FmgInfos
            .Where(e => e.ID == fmgID).FirstOrDefault();

            Dictionary<string, string> vanillaEntries = new();

            foreach(var entry in vanillaFmg.File.Entries)
            {
                // DS2
                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    vanillaEntries.Add($"{entry.ID}{entry.Parent.Name}{containerSubCategory}", entry.Text);
                }
                // Other
                else
                {
                    vanillaEntries.Add($"{entry.ID}", entry.Text);
                }
            }

            foreach(var entry in Selection.SelectedFmgInfo.File.Entries)
            {
                string entryId = $"{entry.ID}";

                // DS2
                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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
        }
    }

    public bool IsDifferentToVanilla(FMG.Entry entry)
    {
        var entryId = $"{entry.ID}";
        var containerSubCategory = Selection.SelectedContainer.SubCategory;

        // DS2
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            entryId = $"{entryId}{entry.Parent.Name}{containerSubCategory}";

            if (DifferenceCache.ContainsKey(entryId))
            {
                return DifferenceCache[entryId];
            }
        }
        // Ptjer
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
        var entryId = $"{entry.ID}";
        var containerSubCategory = Selection.SelectedContainer.SubCategory;

        // DS2
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
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
