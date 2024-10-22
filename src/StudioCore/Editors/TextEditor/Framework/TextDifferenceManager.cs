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

        if (Selection.SelectedContainerWrapper == null)
            return;

        // Leave empty if disabled
        if(!CFG.Current.TextEditor_IncludeVanillaCache)
        {
            return;
        }

        var containerCategory = Selection.SelectedContainerWrapper.ContainerDisplayCategory;
        var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;
        var containerName = Selection.SelectedContainerWrapper.Filename;
        var fmgID = Selection.SelectedFmgWrapper.ID;

        if (TextBank.VanillaBankLoaded)
        {
            var vanillaContainer = TextBank.VanillaFmgBank
                .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
                .Where(e => e.Value.Filename == containerName)
                .FirstOrDefault();

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                vanillaContainer = TextBank.VanillaFmgBank
                .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
                .Where(e => e.Value.ContainerDisplaySubCategory == containerSubCategory)
                .Where(e => e.Value.Filename == containerName)
                .FirstOrDefault();
            }

            var vanillaFmg = vanillaContainer.Value.FmgWrappers
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

            foreach(var entry in Selection.SelectedFmgWrapper.File.Entries)
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
        }
    }

    public bool IsDifferentToVanilla(FMG.Entry entry)
    {
        var entryId = $"{entry.ID}";
        var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;

        // DS2
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
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
        var entryId = $"{entry.ID}";
        var containerSubCategory = Selection.SelectedContainerWrapper.ContainerDisplaySubCategory;

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
