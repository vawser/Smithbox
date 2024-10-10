using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.Editors.TextEditor;

public class TextCommandQueue
{
    private TextEditorScreen Screen;

    public TextCommandQueue(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Parse(string[] initcmd)
    {
        var doFocus = false;

        // text / select / container name / fmg name / fmg entry id
        if (initcmd != null && initcmd[0] == "select")
        {
            if (initcmd.Length > 1)
            {

                /*
                // Select FMG
                doFocus = true;
                // Use three possible keys: entry category is for param references,
                // binder id and FMG name are for soapstone references.
                // This can be revisited as more high-level categories get added.
                int? searchId = null;
                FmgEntryCategory? searchCategory = null;
                string searchName = null;
                if (int.TryParse(initcmd[1], out var intId) && intId >= 0)
                {
                    searchId = intId;
                }
                // Enum.TryParse allows arbitrary ints (thanks C#), so checking definition is required
                else if (Enum.TryParse(initcmd[1], out FmgEntryCategory cat)
                         && Enum.IsDefined(typeof(FmgEntryCategory), cat))
                {
                    searchCategory = cat;
                }
                else
                {
                    searchName = initcmd[1];
                }

                foreach (FMGInfo info in currentFmgBank.FmgInfoBank)
                {
                    var match = false;
                    // This matches top-level item FMGs
                    if (info.EntryCategory.Equals(searchCategory) && info.PatchParent == null
                                                                  && info.EntryType is FmgEntryTextType.Title
                                                                      or FmgEntryTextType.TextBody)
                    {
                        match = true;
                    }
                    else if (searchId is int binderId && binderId == (int)info.FmgID)
                    {
                        match = true;
                    }
                    else if (info.Name == searchName)
                    {
                        match = true;
                    }

                    if (match)
                    {
                        _activeFmgInfo = info;
                        break;
                    }
                }

                if (initcmd.Length > 2 && _activeFmgInfo != null)
                {
                    // Select Entry
                    var parsed = int.TryParse(initcmd[2], out var id);
                    if (parsed)
                    {
                        _activeEntryGroup = currentFmgBank.GenerateEntryGroup(id, _activeFmgInfo);
                    }
                }
                */
            }
        }
    }
}