using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Sets the value, ID and time of day of value rows, each to its own new value, across any number of fields and
/// files. Used by the mass edit, so its whole edit is one undo step. A GPARAM whose IDs change has its param extras
/// rebuilt, as they list the IDs its groups use.
/// </summary>
public class MassSetValuesAction : EditorAction
{
    public record Entry(GPARAM Gparam, GPARAM.IFieldValue Value, object NewValue, int NewID, float NewTimeOfDay);

    private ProjectEntry Project { get; set; }
    private List<Entry> Entries { get; set; }

    private List<(object Value, int ID, float TimeOfDay)> StoredOld { get; set; } = new();
    private Dictionary<GPARAM, List<GPARAM.UnkParamExtra>> StoredParamExtras { get; set; } = new();

    public MassSetValuesAction(ProjectEntry project, List<Entry> entries)
    {
        Project = project;
        Entries = entries;
    }

    public override ActionEvent Execute()
    {
        StoredOld = new();
        StoredParamExtras = new();

        foreach (var gparam in Entries.Where(e => e.Value.ID != e.NewID).Select(e => e.Gparam).Distinct())
        {
            StoredParamExtras[gparam] = GparamUtils.SnapshotParamExtras(gparam);
        }

        foreach (var entry in Entries)
        {
            StoredOld.Add((entry.Value.Value, entry.Value.ID, entry.Value.TimeOfDay));

            entry.Value.Value = entry.NewValue;
            entry.Value.ID = entry.NewID;
            entry.Value.TimeOfDay = entry.NewTimeOfDay;
        }

        foreach (var gparam in StoredParamExtras.Keys)
        {
            GparamUtils.RebuildParamExtras(gparam);
        }

        MarkDirty();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        // Last first, in case a row is listed more than once
        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            var (value, id, timeOfDay) = StoredOld[i];

            Entries[i].Value.Value = value;
            Entries[i].Value.ID = id;
            Entries[i].Value.TimeOfDay = timeOfDay;
        }

        foreach (var (gparam, extras) in StoredParamExtras)
        {
            gparam.UnkParamExtras = extras;
        }

        MarkDirty();

        return ActionEvent.ObjectAddedRemoved;
    }

    private void MarkDirty()
    {
        var bank = Project.Handler.GparamData.PrimaryBank;

        foreach (var gparam in Entries.Select(e => e.Gparam).Distinct())
        {
            bank.MarkAsDirty(gparam);
        }
    }
}
