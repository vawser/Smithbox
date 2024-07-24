using SoulsFormats;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.TextEditor;


/// <summary>
///     Base object that stores an FMG and information regarding it.
/// </summary>
public class FMGInfo
{
    public FMGFileSet FileSet;

    public FmgEntryCategory EntryCategory;
    public FmgEntryTextType EntryType;
    public string FileName;
    public FMG Fmg;
    public FmgIDType FmgID;
    public string Name;

    /// <summary>
    ///     List of associated children to this FMGInfo used to get patch entry data.
    /// </summary>
    public List<FMGInfo> PatchChildren = new();

    public FMGInfo PatchParent;
    public FmgFileCategory FileCategory;

    private string _patchPrefix = null;
    public string PatchPrefix
    {
        get
        {
            _patchPrefix ??= Name.Replace(Smithbox.BankHandler.FMGBank.RemovePatchStrings(Name), "");
            return _patchPrefix;
        }
    }

    public void AddParent(FMGInfo parent)
    {
        PatchParent = parent;
        parent.PatchChildren.Add(this);
    }

    public FMGInfo Clone()
    {
        return (FMGInfo)MemberwiseClone();
    }

    /// <summary>
    ///     Returns a patched list of Entry & FMGInfo value pairs from this FMGInfo and its children.
    ///     If a PatchParent exists, it will be checked instead.
    /// </summary>
    public List<EntryFMGInfoPair> GetPatchedEntryFMGPairs(bool sort = true)
    {
        if (PatchParent != null && !CFG.Current.FMG_NoFmgPatching)
        {
            return PatchParent.GetPatchedEntryFMGPairs(sort);
        }

        List<EntryFMGInfoPair> list = new();
        foreach (FMG.Entry entry in Fmg.Entries)
        {
            list.Add(new EntryFMGInfoPair(this, entry));
        }

        if (!CFG.Current.FMG_NoFmgPatching)
        {
            // Check and apply patch entries
            foreach (FMGInfo child in PatchChildren.OrderBy(e => (int)e.FmgID))
            {
                foreach (FMG.Entry entry in child.Fmg.Entries)
                {
                    EntryFMGInfoPair match = list.Find(e => e.Entry.ID == entry.ID);
                    if (match != null)
                    {
                        // This is a patch entry
                        // Only non-null text will overrwrite
                        if (entry.Text != null)
                        {
                            match.Entry = entry;
                            match.FmgInfo = child;
                        }
                    }
                    else
                    {
                        list.Add(new EntryFMGInfoPair(child, entry));
                    }
                }
            }
        }

        if (sort)
        {
            list = list.OrderBy(e => e.Entry.ID).ToList();
        }

        return list;
    }

    /// <summary>
    ///     Returns a patched list of entries in this FMGInfo and its children.
    ///     If a PatchParent exists, it will be checked instead.
    /// </summary>
    public List<FMG.Entry> GetPatchedEntries(bool sort = true)
    {
        if (PatchParent != null && !CFG.Current.FMG_NoFmgPatching)
        {
            return PatchParent.GetPatchedEntries(sort);
        }

        List<FMG.Entry> list = new();
        list.AddRange(Fmg.Entries);

        if (!CFG.Current.FMG_NoFmgPatching)
        {
            // Check and apply patch entries
            foreach (FMGInfo child in PatchChildren.OrderBy(e => (int)e.FmgID))
            {
                foreach (FMG.Entry entry in child.Fmg.Entries)
                {
                    FMG.Entry match = list.Find(e => e.ID == entry.ID);
                    if (match != null)
                    {
                        // This is a patch entry
                        if (entry.Text != null)
                        {
                            // Text is not null, so it will overwrite non-patch entries.
                            list.Remove(match);
                            list.Add(entry);
                        }
                    }
                    else
                    {
                        list.Add(entry);
                    }
                }
            }
        }

        if (sort)
        {
            list = list.OrderBy(e => e.ID).ToList();
        }

        return list;
    }

    /// <summary>
    ///     Returns title FMGInfo that shares this FMGInfo's EntryCategory.
    ///     If none are found, an exception will be thrown.
    /// </summary>
    public FMGInfo GetTitleFmgInfo()
    {
        foreach (var info in FileSet.FmgInfos)
        {
            if (info.EntryCategory == EntryCategory && info.EntryType == FmgEntryTextType.Title && info.PatchPrefix == PatchPrefix)
            {
                return info;
            }
        }
        throw new InvalidOperationException($"Couldn't find title FMGInfo for {this.Name}");
    }

    /// <summary>
    ///     Adds an entry to the end of the FMG.
    /// </summary>
    public void AddEntry(FMG.Entry entry)
    {
        Fmg.Entries.Add(entry);
    }

    /// <summary>
    ///     Clones an FMG entry.
    /// </summary>
    /// <returns>Cloned entry</returns>
    public FMG.Entry CloneEntry(FMG.Entry entry)
    {
        FMG.Entry newEntry = new(entry.ID, entry.Text);
        return newEntry;
    }

    /// <summary>
    ///     Removes an entry from FMGInfo's FMG.
    /// </summary>
    public void DeleteEntry(FMG.Entry entry)
    {
        Fmg.Entries.Remove(entry);
    }
}