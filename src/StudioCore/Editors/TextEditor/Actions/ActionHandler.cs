using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class ActionHandler
{
    private TextEditorScreen Screen;
    public ActionHandler(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void DuplicateHandler()
    {
        var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
        var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
        var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

        List<EditorAction> actions = new List<EditorAction>();

        for (int k = 0; k < CFG.Current.FMG_DuplicateAmount; k++)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                FMG.Entry r = entries[i];
                if (entryIds.Contains(r.ID))
                {
                    var entry = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);
                    actions.Add(new DuplicateFMGEntryAction(entry));
                }
            }
        }

        var compoundAction = new CompoundAction(actions);
        Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);
        Smithbox.EditorHandler.TextEditor._searchFilterCached = "";
    }

    public void DeleteHandler()
    {
        var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
        var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
        var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

        List<EditorAction> actions = new List<EditorAction>();

        for (var i = 0; i < entries.Count; i++)
        {
            FMG.Entry r = entries[i];
            if (entryIds.Contains(r.ID))
            {
                var entry = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);
                actions.Add(new DeleteFMGEntryAction(entry));
            }
        }

        var compoundAction = new CompoundAction(actions);
        Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);

        Smithbox.EditorHandler.TextEditor._activeEntryGroup = null;
        Smithbox.EditorHandler.TextEditor._activeIDCache = -1;
        Smithbox.EditorHandler.TextEditor._searchFilterCached = "";
    }

    public void SyncDescriptionHandler()
    {
        var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
        var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
        var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

        List<EditorAction> actions = new List<EditorAction>();

        FMG.Entry sourceEntry = null;

        for (var i = 0; i < entries.Count; i++)
        {
            FMG.Entry r = entries[i];
            if (entryIds[0] == r.ID)
            {
                sourceEntry = r;
                break;
            }
        }

        if (sourceEntry != null)
        {
            var sourceEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(sourceEntry.ID, fmgInfo);

            for (var i = 1; i < entries.Count; i++)
            {
                FMG.Entry r = entries[i];
                if (entryIds.Contains(r.ID))
                {
                    var entryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);

                    actions.Add(new SyncFMGEntryAction(entryGroup, sourceEntryGroup));
                }
            }

            var compoundAction = new CompoundAction(actions);
            Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);
        }
    }
}
