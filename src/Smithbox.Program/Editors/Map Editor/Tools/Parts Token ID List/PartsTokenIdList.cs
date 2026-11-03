using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A PTIDL file (AC6): the PartsTokenParam IDs of the enemies of a map, as ASCII text with one decimal ID per line,
/// each ending in '\n'. The lists are kept in map/partstokenidlist/partstokenidlist.ptidlbnd.dcx, named after their maps.
/// A list holds the PartsTokenParamID of each Enemy part, then each DummyEnemy part, whose ID isn't -1,
/// in the order the parts are written to the MSB. This matches every list of the base game that has an MSB.
/// </summary>
public class PartsTokenIdList
{
    public List<int> IDs = new();

    public static PartsTokenIdList Read(byte[] data)
    {
        var list = new PartsTokenIdList();
        var text = Encoding.ASCII.GetString(data);

        foreach (var line in text.Split('\n'))
        {
            var value = line.Trim();

            if (value == "")
                continue;

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                throw new FormatException($"\"{value}\" isn't an ID.");

            list.IDs.Add(id);
        }

        return list;
    }

    public byte[] Write()
    {
        var sb = new StringBuilder();

        foreach (var id in IDs)
        {
            sb.Append(id.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
        }

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    /// <summary>
    /// The enemies of a loaded map that the list of the map holds an entry for, in the order of their entries.
    /// </summary>
    public static List<Entity> GetTokenEnemies(MapContainer map)
    {
        var enemies = map.Objects.Where(e => e.WrappedObject is MSB_AC6.Part.Enemy);
        var dummies = map.Objects.Where(e => e.WrappedObject is MSB_AC6.Part.DummyEnemy);

        return enemies.Concat(dummies)
            .Where(e => ((MSB_AC6.Part.EnemyBase)e.WrappedObject).PartsTokenParamID != -1)
            .ToList();
    }

    /// <summary>
    /// The IDs the list of a loaded map should hold, from the PartsTokenParamIDs of its enemies.
    /// </summary>
    public static List<int> GetMapIDs(MapContainer map)
    {
        return GetTokenEnemies(map)
            .Select(e => ((MSB_AC6.Part.EnemyBase)e.WrappedObject).PartsTokenParamID)
            .ToList();
    }
}
