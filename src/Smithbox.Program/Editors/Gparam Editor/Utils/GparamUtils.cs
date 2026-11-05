using SoulsFormats;
using System.Text.RegularExpressions;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public static class GparamUtils
{
    /// <summary>
    /// The first value ID at or after <paramref name="start"/> that no value in the field uses, nor any in
    /// <paramref name="alsoTaken"/> (IDs about to be added in the same action).
    /// </summary>
    public static int NextFreeValueId(IField field, int start, ISet<int> alsoTaken = null)
    {
        var used = new HashSet<int>();

        if (field != null)
        {
            foreach (var val in field.Values)
            {
                used.Add(val.ID);
            }
        }

        var id = Math.Max(start, 0);

        while (used.Contains(id) || (alsoTaken != null && alsoTaken.Contains(id)))
        {
            id++;
        }

        return id;
    }

    /// <summary>
    /// Copy one GPARAM's contents into another, keeping the target instance. The lists are copied, but the groups in
    /// them are the same objects, so this works both to apply an import and to snapshot a GPARAM for its undo.
    /// Other actions on the undo stack hold the target instance and its groups, so it must never be swapped out.
    /// </summary>
    public static void CopyGparamState(GPARAM target, GPARAM source)
    {
        target.Version = source.Version;
        target.Unk0D = source.Unk0D;
        target.Count14 = source.Count14;
        target.Data30 = source.Data30;
        target.Unk40 = source.Unk40;
        target.Unk50 = source.Unk50;
        target.WideStrings = source.WideStrings;

        target.Params = source.Params != null ? new List<Param>(source.Params) : new List<Param>();
        target.UnkParamExtras = source.UnkParamExtras != null
            ? source.UnkParamExtras.Select(e => e.Clone()).ToList()
            : new List<UnkParamExtra>();
    }

    /// <summary>
    /// A copy of the GPARAM's UnkParamExtras, for an action to put back on undo
    /// </summary>
    public static List<UnkParamExtra> SnapshotParamExtras(GPARAM gparam)
    {
        if (gparam?.UnkParamExtras == null)
            return new List<UnkParamExtra>();

        return gparam.UnkParamExtras.Select(e => e.Clone()).ToList();
    }

    /// <summary>
    /// Rebuild the GPARAM's UnkParamExtras (one entry per group: its index and the value IDs its fields use) after
    /// groups, fields or values have changed.
    ///
    /// Each surviving group keeps its old entry's Unk0c and the order of its IDs that are still in use; IDs new to
    /// the group are appended in field order. Groups with no old entry get a fresh one (Unk0c = 0).
    /// Old entries are matched to groups by object, not index, so pass the group list as it was before the change
    /// (<paramref name="previousParams"/>) when groups were added, removed or reordered. Leave it null when only
    /// fields or values changed.
    /// </summary>
    public static void RebuildParamExtras(GPARAM gparam, IReadOnlyList<Param> previousParams = null)
    {
        if (gparam == null)
            return;

        var groupOrder = previousParams ?? gparam.Params;

        // The old entry for each group, found through the group it pointed at
        var oldExtras = new Dictionary<Param, UnkParamExtra>(ReferenceEqualityComparer.Instance);

        if (gparam.UnkParamExtras != null)
        {
            foreach (var extra in gparam.UnkParamExtras)
            {
                if (extra == null || extra.GroupIndex < 0 || extra.GroupIndex >= groupOrder.Count)
                    continue;

                var oldGroup = groupOrder[extra.GroupIndex];

                if (oldGroup != null && !oldExtras.ContainsKey(oldGroup))
                {
                    oldExtras.Add(oldGroup, extra);
                }
            }
        }

        var newExtras = new List<UnkParamExtra>(gparam.Params.Count);

        for (int i = 0; i < gparam.Params.Count; i++)
        {
            var group = gparam.Params[i];

            // The IDs this group's values use, in field order
            var usedIds = new List<int>();
            var usedSet = new HashSet<int>();

            if (group != null)
            {
                foreach (var field in group.Fields)
                {
                    if (field == null)
                        continue;

                    foreach (var val in field.Values)
                    {
                        if (usedSet.Add(val.ID))
                        {
                            usedIds.Add(val.ID);
                        }
                    }
                }
            }

            var entry = new UnkParamExtra
            {
                GroupIndex = i,
                Unk0c = 0
            };

            if (group != null && oldExtras.TryGetValue(group, out var old))
            {
                entry.Unk0c = old.Unk0c;

                // Old IDs still in use, in their old order
                var kept = new HashSet<int>();

                foreach (var id in old.Ids)
                {
                    if (usedSet.Contains(id) && kept.Add(id))
                    {
                        entry.Ids.Add(id);
                    }
                }

                // Then any the group has gained
                foreach (var id in usedIds)
                {
                    if (!kept.Contains(id))
                    {
                        entry.Ids.Add(id);
                    }
                }
            }
            else
            {
                entry.Ids.AddRange(usedIds);
            }

            newExtras.Add(entry);
        }

        gparam.UnkParamExtras = newExtras;
    }

    /// <summary>
    /// Search for valid duplicate name for a GPARAM file
    /// </summary>
    public static string CreateDuplicateFileName(string fileName)
    {
        Match mapMatch = Regex.Match(fileName, @"[0-9]{4}");

        if (mapMatch.Success)
        {
            var res = mapMatch.Groups[0].Value;

            int slot = 0;
            string slotStr = "";

            try
            {
                int number;
                int.TryParse(res, out number);

                slot = number + 1;
            }
            catch { }

            if (slot >= 100 && slot < 999)
            {
                slotStr = "0";
            }
            if (slot >= 10 && slot < 99)
            {
                slotStr = "00";
            }
            if (slot >= 0 && slot < 9)
            {
                slotStr = "000";
            }

            var finalSlotStr = $"{slotStr}{slot}";
            var final = fileName.Replace(res, finalSlotStr);

            return final;
        }
        else
        {
            Match dupeMatch = Regex.Match(fileName, @"__[0-9]{1}");

            if (dupeMatch.Success)
            {
                var res = dupeMatch.Groups[0].Value;

                Match numMatch = Regex.Match(res, @"[0-9]{1}");

                var num = numMatch.Groups[0].Value;
                try
                {
                    int number;
                    int.TryParse(res, out number);

                    number = number + 1;

                    return $"{fileName}__{number}";
                }
                catch
                {
                    return $"{fileName}__1";
                }
            }
            else
            {
                return $"{fileName}__1";
            }
        }
    }

    public static string GetReadableObjectTypeName(IField field)
    {
        string typeName = $"{field.GetType()}";

        if (field is GPARAM.IntField)
        {
            typeName = "Signed Integer";
        }
        if (field is GPARAM.UintField)
        {
            typeName = "Unsigned Integer";
        }
        if (field is GPARAM.ShortField)
        {
            typeName = "Signed Short";
        }
        if (field is GPARAM.SbyteField)
        {
            typeName = "Signed Byte";
        }
        if (field is GPARAM.ByteField)
        {
            typeName = "Byte";
        }
        if (field is GPARAM.FloatField)
        {
            typeName = "Float";
        }
        if (field is GPARAM.Vector2Field)
        {
            typeName = "Vector2";
        }
        if (field is GPARAM.Vector3Field)
        {
            typeName = "Vector3";
        }
        if (field is GPARAM.Vector4Field)
        {
            typeName = "Vector4";
        }
        if (field is GPARAM.BoolField)
        {
            typeName = "Boolean";
        }
        if (field is GPARAM.ColorField)
        {
            typeName = "Color";
        }

        return typeName;
    }
}
