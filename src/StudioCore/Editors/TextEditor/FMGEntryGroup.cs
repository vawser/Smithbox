using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;


/// <summary>
///     A group of entries that may be associated (such as title, summary, description) along with respective FMGs.
/// </summary>
public class FMGEntryGroup
{
    private int _ID = -1;
    public FMG.Entry Description;
    public FMGInfo DescriptionInfo;
    public FMG.Entry ExtraText;
    public FMGInfo ExtraTextInfo;
    public FMG.Entry Summary;
    public FMGInfo SummaryInfo;
    public FMG.Entry TextBody;
    public FMGInfo TextBodyInfo;
    public FMG.Entry Title;
    public FMGInfo TitleInfo;

    public int ID
    {
        set
        {
            _ID = value;
            if (TextBody != null)
            {
                TextBody.ID = _ID;
            }

            if (Title != null)
            {
                Title.ID = _ID;
            }

            if (Summary != null)
            {
                Summary.ID = _ID;
            }

            if (Description != null)
            {
                Description.ID = _ID;
            }

            if (ExtraText != null)
            {
                ExtraText.ID = _ID;
            }
        }
        get => _ID;
    }

    /// <summary>
    ///     Gets next unused entry ID.
    /// </summary>
    public int GetNextUnusedID(int idIncrement)
    {
        var id = ID;
        if (TextBody != null)
        {
            List<FMG.Entry> entries = TextBodyInfo.GetPatchedEntries();
            do
            {
                id = id + idIncrement;
            } while (entries.Find(e => e.ID == id) != null);
        }
        else if (Title != null)
        {
            List<FMG.Entry> entries = TitleInfo.GetPatchedEntries();
            do
            {
                id = id + idIncrement;
            } while (entries.Find(e => e.ID == id) != null);
        }
        else if (Summary != null)
        {
            List<FMG.Entry> entries = SummaryInfo.GetPatchedEntries();
            do
            {
                id = id + idIncrement; ;
            } while (entries.Find(e => e.ID == id) != null);
        }
        else if (Description != null)
        {
            List<FMG.Entry> entries = DescriptionInfo.GetPatchedEntries();
            do
            {
                id = id + idIncrement;
            } while (entries.Find(e => e.ID == id) != null);
        }
        else if (ExtraText != null)
        {
            List<FMG.Entry> entries = ExtraTextInfo.GetPatchedEntries();
            do
            {
                id = id + idIncrement;
            } while (entries.Find(e => e.ID == id) != null);
        }

        return id;
    }

    /// <summary>
    ///     Sets ID of all entries to the next unused entry ID.
    /// </summary>
    public void SetNextUnusedID()
    {
        ID = GetNextUnusedID(CFG.Current.FMG_DuplicateIncrement);
    }

    /// <summary>
    ///     Places all entries within this EntryGroup into their assigned FMGs.
    /// </summary>
    public void ImplementEntryGroup()
    {
        if (TextBody != null)
        {
            TextBodyInfo.AddEntry(TextBody);
        }

        if (Title != null)
        {
            TitleInfo.AddEntry(Title);
        }

        if (Summary != null)
        {
            SummaryInfo.AddEntry(Summary);
        }

        if (Description != null)
        {
            DescriptionInfo.AddEntry(Description);
        }

        if (ExtraText != null)
        {
            ExtraTextInfo.AddEntry(ExtraText);
        }
    }

    /// <summary>
    ///     Duplicates all entries within their assigned FMGs.
    ///     New entries are inserted into their assigned FMGs.
    /// </summary>
    /// <returns>New EntryGroup.</returns>
    public FMGEntryGroup DuplicateFMGEntries()
    {
        FMGEntryGroup newGroup = new();
        if (TextBody != null)
        {
            newGroup.TextBodyInfo = TextBodyInfo;
            newGroup.TextBody = TextBodyInfo.CloneEntry(TextBody);
            TextBodyInfo.AddEntry(newGroup.TextBody);
        }

        if (Title != null)
        {
            newGroup.TitleInfo = TitleInfo;
            newGroup.Title = TitleInfo.CloneEntry(Title);
            TitleInfo.AddEntry(newGroup.Title);
        }

        if (Summary != null)
        {
            newGroup.SummaryInfo = SummaryInfo;
            newGroup.Summary = SummaryInfo.CloneEntry(Summary);
            SummaryInfo.AddEntry(newGroup.Summary);
        }

        if (Description != null)
        {
            newGroup.DescriptionInfo = DescriptionInfo;
            newGroup.Description = DescriptionInfo.CloneEntry(Description);
            DescriptionInfo.AddEntry(newGroup.Description);
        }

        if (ExtraText != null)
        {
            newGroup.ExtraTextInfo = ExtraTextInfo;
            newGroup.ExtraText = ExtraTextInfo.CloneEntry(ExtraText);
            ExtraTextInfo.AddEntry(newGroup.ExtraText);
        }

        newGroup.ID = ID;
        return newGroup;
    }

    /// <summary>
    ///     Clones this EntryGroup and returns a duplicate.
    /// </summary>
    /// <returns>Cloned EntryGroup.</returns>
    public FMGEntryGroup CloneEntryGroup()
    {
        FMGEntryGroup newGroup = new();
        if (TextBody != null)
        {
            newGroup.TextBodyInfo = TextBodyInfo;
            newGroup.TextBody = TextBodyInfo.CloneEntry(TextBody);
        }

        if (Title != null)
        {
            newGroup.TitleInfo = TitleInfo;
            newGroup.Title = TitleInfo.CloneEntry(Title);
        }

        if (Summary != null)
        {
            newGroup.SummaryInfo = SummaryInfo;
            newGroup.Summary = SummaryInfo.CloneEntry(Summary);
        }

        if (Description != null)
        {
            newGroup.DescriptionInfo = DescriptionInfo;
            newGroup.Description = DescriptionInfo.CloneEntry(Description);
        }

        if (ExtraText != null)
        {
            newGroup.ExtraTextInfo = ExtraTextInfo;
            newGroup.ExtraText = ExtraTextInfo.CloneEntry(ExtraText);
        }

        return newGroup;
    }

    /// <summary>
    ///     Removes all entries from their assigned FMGs.
    /// </summary>
    public void DeleteEntries()
    {
        if (TextBody != null)
        {
            TextBodyInfo.DeleteEntry(TextBody);
        }

        if (Title != null)
        {
            TitleInfo.DeleteEntry(Title);
        }

        if (Summary != null)
        {
            SummaryInfo.DeleteEntry(Summary);
        }

        if (Description != null)
        {
            DescriptionInfo.DeleteEntry(Description);
        }

        if (ExtraText != null)
        {
            ExtraTextInfo.DeleteEntry(ExtraText);
        }
    }
}
