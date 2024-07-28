using Andre.Formats;
using DotNext;
using Google.Protobuf.WellKnownTypes;
using HKX2;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.MapEditor.LightmapAtlasEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextEditor.Tools;
using StudioCore.Tasks;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using static SoulsFormats.MQB;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.Editor;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class EditorAction
{
    public abstract ActionEvent Execute();
    public abstract ActionEvent Undo();
}

public class PropertiesChangedAction : EditorAction
{
    private readonly object ChangedObject;
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public PropertiesChangedAction(object changed)
    {
        ChangedObject = changed;
    }

    public PropertiesChangedAction(PropertyInfo prop, object changed, object newval)
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        change.OldValue = prop.GetValue(ChangedObject);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);
    }

    public PropertiesChangedAction(PropertyInfo prop, int index, object changed, object newval)
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)change.Property.GetValue(ChangedObject);
            change.OldValue = a.GetValue(index);
        }
        else
        {
            change.OldValue = prop.GetValue(ChangedObject);
        }

        change.NewValue = newval;
        change.ArrayIndex = index;
        Changes.Add(change);
    }

    public void AddPropertyChange(PropertyInfo prop, object newval, int index = -1)
    {
        var change = new PropertyChange();
        change.Property = prop;
        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)change.Property.GetValue(ChangedObject);
            change.OldValue = a.GetValue(index);
        }
        else
        {
            change.OldValue = prop.GetValue(ChangedObject);
        }

        change.NewValue = newval;
        change.ArrayIndex = index;
        Changes.Add(change);
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(ChangedObject);
                a.SetValue(change.NewValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(ChangedObject, change.NewValue);
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(ChangedObject);
                a.SetValue(change.OldValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(ChangedObject, change.OldValue);
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public object NewValue;
        public object OldValue;
        public PropertyInfo Property;
    }
}

public class AddParamsAction : EditorAction
{
    private readonly bool appOnly;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private readonly List<Param.Row> Removed = new();
    private readonly List<int> RemovedIndex = new();
    private readonly bool replParams;
    private string ParamString;

    public AddParamsAction(Param param, string pstring, List<Param.Row> rows, bool appendOnly, bool replaceParams,
        int index = -1)
    {
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        appOnly = appendOnly;
        replParams = replaceParams;
        InsertIndex = index;
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);
            if (InsertIndex > -1)
            {
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.InsertRow(InsertIndex, newrow);
            }
            else
            {
                if (Param[row.ID] != null)
                {
                    if (replParams)
                    {
                        Param.Row existing = Param[row.ID];
                        RemovedIndex.Add(Param.IndexOfRow(existing));
                        Removed.Add(existing);
                        Param.RemoveRow(existing);
                    }
                    else
                    {
                        newrow.Name = row.Name != null ? row.Name + "_1" : "";
                        var newID = row.ID + 1;
                        while (Param[newID] != null)
                        {
                            newID++;
                        }

                        newrow.ID = newID;
                        Param.InsertRow(Param.IndexOfRow(Param[newID - 1]) + 1, newrow);
                    }
                }

                if (Param[row.ID] == null)
                {
                    newrow.Name = row.Name != null ? row.Name : "";
                    if (appOnly)
                    {
                        Param.AddRow(newrow);
                    }
                    else
                    {
                        var index = 0;
                        foreach (Param.Row r in Param.Rows)
                        {
                            if (r.ID > newrow.ID)
                            {
                                break;
                            }

                            index++;
                        }

                        Param.InsertRow(index, newrow);
                    }
                }
            }

            Clones.Add(newrow);
        }

        // Refresh diff cache
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
        return ActionEvent.NoEvent;
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.RemoveRow(Clones[i]);
        }

        for (var i = Removed.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemovedIndex[i], Removed[i]);
        }

        Clones.Clear();
        RemovedIndex.Clear();
        Removed.Clear();
        return ActionEvent.NoEvent;
    }
}

public class DeleteParamsAction : EditorAction
{
    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamsAction(Param param, List<Param.Row> rows)
    {
        Param = param;
        Deletables.AddRange(rows);
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Deletables)
        {
            RemoveIndices.Add(Param.IndexOfRow(row));
            Param.RemoveRowAt(RemoveIndices.Last());
        }

        if (SetSelection)
        {
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Deletables.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemoveIndices[i], Deletables[i]);
        }

        if (SetSelection)
        {
        }

        // Refresh diff cache
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
        return ActionEvent.NoEvent;
    }
}

public class DuplicateFMGEntryAction : EditorAction
{
    private readonly FMGEntryGroup EntryGroup;
    private FMGEntryGroup NewEntryGroup;

    public DuplicateFMGEntryAction(FMGEntryGroup entryGroup)
    {
        EntryGroup = entryGroup;
    }

    public override ActionEvent Execute()
    {
        NewEntryGroup = EntryGroup.DuplicateFMGEntries();
        NewEntryGroup.SetNextUnusedID();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        NewEntryGroup.DeleteEntries();
        return ActionEvent.NoEvent;
    }
}

public class DeleteFMGEntryAction : EditorAction
{
    private FMGEntryGroup BackupEntryGroup = new();
    private FMGEntryGroup EntryGroup;

    public DeleteFMGEntryAction(FMGEntryGroup entryGroup)
    {
        EntryGroup = entryGroup;
    }

    public override ActionEvent Execute()
    {
        BackupEntryGroup = EntryGroup.CloneEntryGroup();
        EntryGroup.DeleteEntries();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EntryGroup = BackupEntryGroup;
        EntryGroup.ImplementEntryGroup();
        return ActionEvent.NoEvent;
    }
}

public class SyncFMGEntryAction : EditorAction
{
    private FMGEntryGroup EntryGroup = new();
    private FMGEntryGroup BackupEntryGroup = new();
    private FMGEntryGroup NewEntryGroup = new();

    public SyncFMGEntryAction(FMGEntryGroup entryGroup, FMGEntryGroup sourceEntryGroup)
    {
        EntryGroup = entryGroup;
        BackupEntryGroup = entryGroup.CloneEntryGroup();
        NewEntryGroup = sourceEntryGroup;
    }

    public override ActionEvent Execute()
    {
        EntryGroup.Description.Text = NewEntryGroup.Description.Text;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EntryGroup.Description.Text = BackupEntryGroup.Description.Text;
        return ActionEvent.NoEvent;
    }
}

public class ReplaceFMGEntryTextAction : EditorAction
{
    private FMG.Entry Entry;
    private FMG.Entry BackupEntry;
    private string NewText = "";

    public ReplaceFMGEntryTextAction(FMG.Entry entryGroup, string newText)
    {
        BackupEntry = new FMG.Entry(entryGroup.ID, entryGroup.Text);
        Entry = entryGroup;
        NewText = newText;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = BackupEntry.Text;
        return ActionEvent.NoEvent;
    }
}

public class GenerateFMGEntryAction : EditorAction
{
    private FMGInfo FMGInfo = new();
    private FMGEntryGroup BaseEntryGroup = new();
    private FmgEntryGeneratorBase Source;
    private List<FMGEntryGroup> NewEntries = new();

    public GenerateFMGEntryAction(FMGInfo fmgInfo, FMGEntryGroup baseEntryGroup, FmgEntryGeneratorBase source)
    {
        FMGInfo = fmgInfo;
        BaseEntryGroup = baseEntryGroup;
        Source = source;
    }

    public override ActionEvent Execute()
    {
        // ID
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_Title.Count > i)
            {
                var adjustmentEntry = Source.FMG_Title[i];

                var newEntry = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(BaseEntryGroup.ID, FMGInfo);
                newEntry.DuplicateFMGEntries();
                newEntry.ID = BaseEntryGroup.ID + adjustmentEntry.Offset;
                NewEntries.Add(newEntry);
            }
        }

        // Title
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_Title.Count > i)
            {
                var adjustmentEntry = Source.FMG_Title[i];
                var newEntry = NewEntries[i];

                newEntry.Title.Text = ProcessText(newEntry.Title.Text, newEntry, adjustmentEntry);
            }
        }

        // TextBody
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_TextBody.Count > i)
            {
                var adjustmentEntry = Source.FMG_TextBody[i];
                var newEntry = NewEntries[i];

                newEntry.TextBody.Text = ProcessText(newEntry.TextBody.Text, newEntry, adjustmentEntry);
            }
        }

        // Summary
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_Summary.Count > i)
            {
                var adjustmentEntry = Source.FMG_Summary[i];
                var newEntry = NewEntries[i];

                newEntry.Summary.Text = ProcessText(newEntry.Summary.Text, newEntry, adjustmentEntry);
            }
        }

        // Description
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_Description.Count > i)
            {
                var adjustmentEntry = Source.FMG_Description[i];
                var newEntry = NewEntries[i];

                newEntry.Description.Text = ProcessText(newEntry.Description.Text, newEntry, adjustmentEntry);
            }
        }

        // ExtraText
        for (int i = 0; i < Source.Count; i++)
        {
            if (Source.FMG_ExtraText.Count > i)
            {
                var adjustmentEntry = Source.FMG_ExtraText[i];
                var newEntry = NewEntries[i];

                newEntry.ExtraText.Text = ProcessText(newEntry.ExtraText.Text, newEntry, adjustmentEntry);
            }
        }

        Smithbox.EditorHandler.TextEditor.RefreshTextEditorCache();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var entry in NewEntries)
        {
            entry.DeleteEntries();
        }

        Smithbox.EditorHandler.TextEditor.RefreshTextEditorCache();

        return ActionEvent.NoEvent;
    }

    public string ProcessText(string baseText, FMGEntryGroup newEntry, FmgEntryGeneratorRow adjustmentEntry)
    {
        var newText = baseText;

        // Search and Replace
        if (adjustmentEntry.ReplaceList.Count > 0)
        {
            foreach (var entry in adjustmentEntry.ReplaceList)
            {
                if (entry.SearchText != "")
                {
                    newText = newText.Replace(entry.SearchText, entry.ReplaceText);
                }
            }
        }

        // Adjust the prepend/append to account for possesiveness
        if (adjustmentEntry.PossessiveAdjust)
        {
            var possessiveWord = "";

            // ([a-z]*'s).*
            var result = Regex.Match(newText, @"([a-z]*'s)", RegexOptions.IgnoreCase);
            possessiveWord = result.Value;
            if (possessiveWord != "")
            {
                var tempText = newText.Replace(possessiveWord, "").TrimStart(' ');
                tempText = $"{adjustmentEntry.PrependText}{tempText}{adjustmentEntry.AppendText}";
                newText = $"{possessiveWord} {tempText}";
            }
            else
            {
                newText = $"{adjustmentEntry.PrependText}{newText}{adjustmentEntry.AppendText}";
            }
        }
        else
        {
            // Text Prepend/Append
            newText = $"{adjustmentEntry.PrependText}{newText}{adjustmentEntry.AppendText}";
        }

        return newText;
    }
}

public class CompoundAction : EditorAction
{
    private readonly List<EditorAction> Actions;

    private Action<bool> PostExecutionAction;

    public CompoundAction(List<EditorAction> actions)
    {
        Actions = actions;
    }

    public bool HasActions => Actions.Any();

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var evt = ActionEvent.NoEvent;
        for (var i = 0; i < Actions.Count; i++)
        {
            EditorAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Execute();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return evt;
    }

    public override ActionEvent Undo()
    {
        var evt = ActionEvent.NoEvent;
        for (var i = Actions.Count - 1; i >= 0; i--)
        {
            EditorAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Undo();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }
}

public class LightmapAtlasChangeAtlasID : EditorAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private int OldValue;
    private int NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeAtlasID(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, int newValue, int oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override ActionEvent Execute()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.AtlasID = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.AtlasID = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangePartName : EditorAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangePartName(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override ActionEvent Execute()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.PartName = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.PartName = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeMaterialName : EditorAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeMaterialName(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override ActionEvent Execute()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.MaterialName = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.MaterialName = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVOffset : EditorAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVOffset(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override ActionEvent Execute()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVOffset = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVOffset = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVScale : EditorAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVScale(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if(!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override ActionEvent Execute()
    {
        if(ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVScale = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVScale = OldValue;

        return ActionEvent.NoEvent;
    }
}
