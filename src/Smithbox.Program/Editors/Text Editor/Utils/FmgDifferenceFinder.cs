using StudioCore.Application;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Simple FMG difference finder that is separate from the standard FMG difference manager so we don't rely on the current selection state in anyway.
/// </summary>
public static class FmgDifferenceFinder
{
    public static FmgDifferenceResult GetFmgDifferenceResult(TextEditorScreen editor, TextFmgWrapper srcFmg, TextFmgWrapper compareFmg)
    {
        var result = new FmgDifferenceResult();
        result.SourceFmgWrapper = srcFmg;
        result.ComparisonFmgWrapper = compareFmg;

        // Get the entries from the source FMG
        Dictionary<int, string> srcEntries = new();

        foreach (var entry in srcFmg.File.Entries)
        {
            // DS2
            if (editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (srcEntries.ContainsKey(entry.ID))
                {
                    srcEntries[entry.ID] = entry.Text;
                }
                else
                {
                    srcEntries.Add(entry.ID, entry.Text);
                }
            }
            // Other
            else
            {
                if (srcEntries.ContainsKey(entry.ID))
                {
                    srcEntries[entry.ID] = entry.Text;
                }
                else
                {
                    srcEntries.Add(entry.ID, entry.Text);
                }
            }
        }

        // Calc additions and modified
        foreach (var entry in compareFmg.File.Entries)
        {
            string entryText = $"{entry.Text}";

            if (!result.DefaultCache.ContainsKey(entry.ID))
            {
                result.DefaultCache.Add(entry.ID, entryText);
            }

            // DS2
            if (editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (srcEntries.ContainsKey(entry.ID))
                {
                    var vanillaText = srcEntries[entry.ID];

                    if (vanillaText != null)
                    {
                        if (!vanillaText.Equals(entry.Text))
                        {
                            if (!result.ModifiedCache.ContainsKey(entry.ID))
                            {
                                result.ModifiedCache.Add(entry.ID, entryText);
                            }
                        }
                    }
                    else
                    {
                        // If project entry is not null, we can assume that it is different
                        if (entry.Text != null)
                        {
                            if (!result.ModifiedCache.ContainsKey(entry.ID))
                            {
                                result.ModifiedCache.Add(entry.ID, entryText);
                            }
                        }
                    }
                }
                // Is a mod-unique row, there it is a difference
                else
                {
                    if (!result.AdditionCache.ContainsKey(entry.ID))
                    {
                        result.AdditionCache.Add(entry.ID, entryText);
                    }
                }
            }
            // Other
            else
            {
                if (srcEntries.ContainsKey(entry.ID))
                {
                    var vanillaText = srcEntries[entry.ID];

                    if (vanillaText != null)
                    {
                        if (!vanillaText.Equals(entry.Text))
                        {
                            if (!result.ModifiedCache.ContainsKey(entry.ID))
                            {
                                result.ModifiedCache.Add(entry.ID, entryText);
                            }
                        }
                    }
                    else
                    {
                        // If project entry is not null, we can assume that it is different
                        if (entry.Text != null)
                        {
                            if (!result.ModifiedCache.ContainsKey(entry.ID))
                            {
                                result.ModifiedCache.Add(entry.ID, entryText);
                            }
                        }
                    }
                }
                // Is a mod-unique row, there it is a difference
                else
                {
                    if (!result.AdditionCache.ContainsKey(entry.ID))
                    {
                        result.AdditionCache.Add(entry.ID, entryText);
                    }
                }
            }
        }

        return result;
    }
}

public class FmgDifferenceResult
{
    public TextFmgWrapper SourceFmgWrapper;
    public TextFmgWrapper ComparisonFmgWrapper;

    // <Entry ID>, <isAdded or isModified>
    public Dictionary<int, string> AdditionCache = new();
    public Dictionary<int, string> ModifiedCache = new();
    public Dictionary<int, string> DefaultCache = new();
}