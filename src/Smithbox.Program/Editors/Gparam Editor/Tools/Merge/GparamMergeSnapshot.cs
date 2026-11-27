using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// A GPARAM's state as references: its header, its group list, each group's name, comments and field list, each
/// field's name, Unk and value list, and each value's ID, time and value. <see cref="Restore"/> puts all of it back
/// into the same objects, so whatever an older undo step or the selection holds stays live
/// (Claude/Documentation/Gparam.md §7.3).
///
/// <see cref="GparamMerge.Apply(GPARAM, IEnumerable{GparamMergeItem})"/> only ever changes list membership, header and
/// property values, and value contents in place; anything it adds is a new object (a clone of theirs), which a
/// restore simply drops. So a snapshot taken before Apply undoes it exactly. No editor dependencies.
/// </summary>
public class GparamMergeSnapshot
{
    private readonly GPARAM Gparam;

    private readonly bool Unk0D;
    private readonly float Unk40, Unk50;
    private readonly List<GPARAM.Param> Params;
    private readonly List<GPARAM.UnkParamExtra> Extras;

    private readonly List<GroupState> Groups = new();

    private sealed record GroupState(GPARAM.Param Group, string Name, List<string> Comments, List<GPARAM.IField> Fields,
        List<FieldState> FieldStates);

    private sealed record FieldState(GPARAM.IField Field, string Name, short Unk, List<GPARAM.IFieldValue> Values,
        List<(int ID, float TimeOfDay, object Value)> Contents);

    private GparamMergeSnapshot(GPARAM gparam)
    {
        Gparam = gparam;

        Unk0D = gparam.Unk0D;
        Unk40 = gparam.Unk40;
        Unk50 = gparam.Unk50;
        Params = gparam.Params.ToList();
        Extras = GparamUtils.SnapshotParamExtras(gparam);

        foreach (var group in Params)
        {
            if (group == null)
                continue;

            var fields = group.Fields.ToList();
            var states = new List<FieldState>(fields.Count);

            foreach (var field in fields)
            {
                if (field == null)
                    continue;

                var values = field.Values.ToList();
                states.Add(new FieldState(field, field.Name, GparamMerge.FieldUnkOf(field), values,
                    values.Select(v => (v.ID, v.TimeOfDay, v.Value)).ToList()));
            }

            // The comment list object itself: Apply replaces it, never edits it
            Groups.Add(new GroupState(group, group.Name, group.Comments, fields, states));
        }
    }

    public static GparamMergeSnapshot Take(GPARAM gparam) => new(gparam);

    public void Restore()
    {
        Gparam.Unk0D = Unk0D;
        Gparam.Unk40 = Unk40;
        Gparam.Unk50 = Unk50;

        Gparam.Params.Clear();
        Gparam.Params.AddRange(Params);

        foreach (var g in Groups)
        {
            g.Group.Name = g.Name;
            g.Group.Comments = g.Comments;
            g.Group.Fields.Clear();
            g.Group.Fields.AddRange(g.Fields);

            foreach (var f in g.FieldStates)
            {
                f.Field.Name = f.Name;
                GparamMerge.SetFieldUnkOf(f.Field, f.Unk);

                var list = f.Field.ValueList();
                list.Clear();

                for (var i = 0; i < f.Values.Count; i++)
                {
                    var value = f.Values[i];
                    var (id, time, content) = f.Contents[i];
                    value.ID = id;
                    value.TimeOfDay = time;
                    value.Value = content;
                    list.Add(value);
                }

                f.Field.SyncCapacity();
            }
        }

        Gparam.UnkParamExtras = Extras.Select(e => e.Clone()).ToList();
    }
}
