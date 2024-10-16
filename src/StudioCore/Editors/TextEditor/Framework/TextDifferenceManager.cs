using SoulsFormats;
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
    private Dictionary<int, bool> AdditionCache = new();

    /// <summary>
    /// Holds the row IDs for the current FMG where the row text is different to vanilla.
    /// </summary>
    private Dictionary<int, bool> DifferenceCache = new();

    /// <summary>
    /// Generate difference truth for currently selected FMG
    /// </summary>
    public void TrackFmgDifferences()
    {
        AdditionCache = new();
        DifferenceCache = new();

        var containerCategory = Selection.SelectedContainer.Category;
        var containerName = Selection.SelectedContainer.Name;
        var fmgID = Selection.SelectedFmgInfo.ID;

        if (TextBank.VanillaBankLoaded)
        {
            var vanillaContainer = TextBank.VanillaFmgBank
                .Where(e => e.Value.Category == containerCategory)
                .Where(e => e.Value.Name == containerName)
                .FirstOrDefault();

            var vanillaFmg = vanillaContainer.Value.FmgInfos
                .Where(e => e.ID == fmgID).FirstOrDefault();

            Dictionary<int, string> vanillaEntries = new();

            foreach(var entry in vanillaFmg.File.Entries)
            {
                vanillaEntries.Add(entry.ID, entry.Text);
            }

            foreach(var entry in Selection.SelectedFmgInfo.File.Entries)
            {
                if(vanillaEntries.ContainsKey(entry.ID))
                {
                    var vanillaText = vanillaEntries[entry.ID];

                    if (vanillaText != null)
                    {
                        if (!vanillaText.Equals(entry.Text))
                        {
                            if (!DifferenceCache.ContainsKey(entry.ID))
                            {
                                DifferenceCache.Add(entry.ID, true);
                            }
                        }
                    }
                }
                // Is a mod-unique row, there it is a difference
                else
                {
                    if (!AdditionCache.ContainsKey(entry.ID))
                    {
                        AdditionCache.Add(entry.ID, true);
                    }

                    if (!DifferenceCache.ContainsKey(entry.ID))
                    {
                        DifferenceCache.Add(entry.ID, true);
                    }
                }
            }
        }
    }

    public bool IsDifferentToVanilla(FMG.Entry entry)
    {
        if(DifferenceCache.ContainsKey(entry.ID))
        {
            return DifferenceCache[entry.ID];
        }

        return false;
    }

    public bool IsUniqueToProject(FMG.Entry entry)
    {
        if (AdditionCache.ContainsKey(entry.ID))
        {
            return AdditionCache[entry.ID];
        }

        return false;
    }
}
