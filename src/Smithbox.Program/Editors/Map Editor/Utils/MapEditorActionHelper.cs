using Andre.Formats;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The IDs in use on a map, so that assigning IDs to many objects doesn't have to search the map for each one.
/// Only valid while the objects of the map are not otherwise changing.
/// </summary>
public class UniqueIdCache
{
    public HashSet<long> EntityIDs;
    public Dictionary<string, HashSet<int>> InstanceIDs;
}

/// <summary>
/// For functions that multiple EntityActions make use of.
/// </summary>
public static class MapEditorActionHelper
{
    public static void RandomiseRotation(MapEditorView view, MsbEntity sel, MapContainer map, RandomiseRotationDirection dir, float range)
    {
        var rand = new Random();
        var rotAdd = (float)rand.NextDouble() * range;

        Transform objT = sel.GetLocalTransform();

        var rot_x = objT.EulerRotation.X;
        var rot_y = objT.EulerRotation.Y;
        var rot_z = objT.EulerRotation.Z;

        var newRot = Transform.Default;

        if (dir is RandomiseRotationDirection.X)
        {
            rot_x = objT.EulerRotation.X + rotAdd;
        }

        if (dir is RandomiseRotationDirection.Y)
        {
            rot_y = objT.EulerRotation.Y + rotAdd;
        }

        if (dir is RandomiseRotationDirection.Z)
        {
            rot_z = objT.EulerRotation.Z + rotAdd;
        }

        var newRotation = new Vector3(rot_x, rot_y, rot_z);

        if (view.Project.Descriptor.ProjectType == ProjectType.DS2S || view.Project.Descriptor.ProjectType == ProjectType.DS2)
        {
            // All other map object types
            sel.SetPropertyValue("Rotation", newRotation);

            // Enemies
            if (sel.WrappedObject is Param.Row row)
            {
                Param.Cell? ppx = row["RotationX"];
                if (ppx != null)
                {
                    PropertyInfo pprop = ppx.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppx);

                    pprop.SetValue(ppx, newRotation.X * Utils.Rad2Deg);
                }

                Param.Cell? ppy = row["RotationY"];
                if (ppy != null)
                {
                    PropertyInfo pprop = ppy.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppy);

                    pprop.SetValue(ppy, newRotation.Y * Utils.Rad2Deg - 180.0f);
                }

                Param.Cell? ppz = row["RotationZ"];
                if (ppz != null)
                {
                    PropertyInfo pprop = ppz.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppz);

                    pprop.SetValue(ppz, newRotation.Z * Utils.Rad2Deg);
                }
            }
            else  if (sel.WrappedObject is MergedParamRow mergedRow)
            {
                Param.Cell? ppx = mergedRow["RotationX"];
                if (ppx != null)
                {
                    PropertyInfo pprop = ppx.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppx);

                    pprop.SetValue(ppx, newRotation.X * Utils.Rad2Deg);
                }

                Param.Cell? ppy = mergedRow["RotationY"];
                if (ppy != null)
                {
                    PropertyInfo pprop = ppy.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppy);

                    pprop.SetValue(ppy, newRotation.Y * Utils.Rad2Deg - 180.0f);
                }

                Param.Cell? ppz = mergedRow["RotationZ"];
                if (ppz != null)
                {
                    PropertyInfo pprop = ppz.GetType().GetProperty("Value");

                    var value = pprop.GetValue(ppz);

                    pprop.SetValue(ppz, newRotation.Z * Utils.Rad2Deg);
                }
            }
        }
        else
        {
            sel.SetPropertyValue("Rotation", newRotation);
        }
    }

    public static void SetUniqueEntityID(MapEditorView view, MsbEntity sel, MapContainer map, UniqueIdCache cache = null)
    {
        if (sel.WrappedObject is BTL.Light)
            return;

        if (view.Project.Descriptor.ProjectType == ProjectType.DS2S || view.Project.Descriptor.ProjectType == ProjectType.DS2)
            return;

        if (view.Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            SetUniqueEntityID_AC6(view, sel, map, cache);
        }
        else if (view.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            SetUniqueEntityID_ER(view, sel, map, cache);
        }
        else
        {
            SetUniqueEntityID_Int(view, sel, map, cache);
        }
    }

    /// <summary>
    /// Gets the Entity IDs in use on the map, which must not include the ID being replaced.
    /// </summary>
    private static HashSet<long> GetUsedEntityIDs(MapContainer map, UniqueIdCache cache)
    {
        if (cache?.EntityIDs != null)
            return cache.EntityIDs;

        HashSet<long> vals = new();

        foreach (var e in map?.Objects)
        {
            switch (PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject))
            {
                case int entInt:
                    vals.Add(entInt);
                    break;
                case uint entUint:
                    vals.Add(entUint);
                    break;
            }
        }

        if (cache != null)
            cache.EntityIDs = vals;

        return vals;
    }

    /// <summary>
    /// Gets the first unused ID in the range, preferring those after the original ID. Returns 0 if the range is full.
    /// </summary>
    /// <param name="preferredMinId">IDs below this are only used if nothing else is free</param>
    private static long FindUnusedID(HashSet<long> used, long originalID, long minId, long maxId, long preferredMinId = 0)
    {
        // Prefer IDs after the original ID first
        for (var id = Math.Max(Math.Max(originalID + 1, minId), preferredMinId); id < maxId; id++)
        {
            if (!used.Contains(id))
                return id;
        }

        // No match in preferred range, get first of possible values.
        for (var id = minId; id < maxId; id++)
        {
            if (!used.Contains(id))
                return id;
        }

        return 0;
    }

    public static void SetUniqueEntityID_AC6(MapEditorView view, MsbEntity sel, MapContainer map, UniqueIdCache cache = null)
    {
        uint originalID = (uint)sel.GetPropertyValue("EntityID");

        if (originalID == 0 )
            return;

        sel.SetPropertyValue("EntityID", (uint)0);

        var vals = GetUsedEntityIDs(map, cache);

        uint minId = 100;
        uint maxId = 9999;

        var newID = (uint)FindUnusedID(vals, originalID, minId, maxId);

        vals.Add(newID);
        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetUniqueEntityID_ER(MapEditorView view, MsbEntity sel, MapContainer map, UniqueIdCache cache = null)
    {
        uint originalID = (uint)sel.GetPropertyValue("EntityID");

        // Don't set entity ID if the original entity doesn't have one
        if (originalID == 0)
            return;

        sel.SetPropertyValue("EntityID", (uint)0);

        // For enemies, only fill vals with other enemy IDs, as ER enemies use 7 digits, not 8 like the other map objects
        var vals = GetUsedEntityIDs(map, cache);

        var mapIdParts = map.Name.Replace("m", "").Split("_");

        uint minId = 0;
        uint maxId = 9999;

        minId = uint.Parse($"{mapIdParts[0]}{mapIdParts[1]}0000");
        maxId = uint.Parse($"{mapIdParts[0]}{mapIdParts[1]}9999");

        // Is open-world tile
        if (mapIdParts[0] == "60")
        {
            minId = uint.Parse($"10{mapIdParts[1]}{mapIdParts[2]}0000");
            maxId = uint.Parse($"10{mapIdParts[1]}{mapIdParts[2]}9999");
        }
        // Is DLC open-world tile
        if (mapIdParts[0] == "61")
        {
            minId = uint.Parse($"20{mapIdParts[1]}{mapIdParts[2]}0000");
            maxId = uint.Parse($"20{mapIdParts[1]}{mapIdParts[2]}9999");
        }

        var newID = (uint)FindUnusedID(vals, originalID, minId, maxId);

        vals.Add(newID);
        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetUniqueEntityID_Int(MapEditorView view, MsbEntity sel, MapContainer map, UniqueIdCache cache = null)
    {
        int originalID = (int)sel.GetPropertyValue("EntityID");

        if (originalID == 0 || originalID == -1)
            return;

        sel.SetPropertyValue("EntityID", -1);

        // Get currently used Entity IDs
        var vals = GetUsedEntityIDs(map, cache);

        // Build set of all 'valid' Entity IDs
        var mapIdParts = map.Name.Replace("m", "").Split("_");

        int minId = 0;
        int maxId = 9999;

        // Get the first non-zero digit from mapIdParts[1]
        var part = mapIdParts[1][1];

        minId = int.Parse($"{mapIdParts[0]}{part}0000");
        maxId = int.Parse($"{mapIdParts[0]}{part}9999");

        // This is to ignore the 4 digit Entity IDs used in some DS1 maps
        var preferredMinId = view.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R ? 10000 : 0;

        var newID = (int)FindUnusedID(vals, originalID, minId, maxId, preferredMinId);

        vals.Add(newID);
        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetSelfPartNames(MapEditorView view, MsbEntity sel, MapContainer map)
    {
        if (view.Project.Descriptor.ProjectType == ProjectType.ER)
        {
            if (sel.WrappedObject is MSBE.Part.Asset)
            {
                string partName = (string)sel.GetPropertyValue("Name");

                if (partName == null)
                    return;

                string modelName = (string)sel.GetPropertyValue("ModelName");

                if (modelName == null)
                    return;

                string[] names = (string[])sel.GetPropertyValue("PartNames");

                if (names == null)
                    return;

                string[] newNames = new string[names.Length];

                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i];

                    if (name == null)
                        continue;

                    // Name is a AEG reference
                    if (name.Contains(modelName) && 
                        (name.Contains("AEG") || name.Contains("aeg")))
                    {
                        name = partName;
                    }

                    newNames[i] = name;
                }

                sel.SetPropertyValue("PartNames", newNames);

                string unkPartName = (string)sel.GetPropertyValue("UnkT54PartName");

                if (unkPartName != null)
                {
                    if (unkPartName.Contains(modelName) && 
                        (unkPartName.Contains("AEG") || unkPartName.Contains("aeg")))
                    {
                        sel.SetPropertyValue("UnkT54PartName", partName);
                    }
                }
            }
        }

        if (view.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            if (sel.WrappedObject is MSB_AC6.Part.Asset)
            {
                string partName = (string)sel.GetPropertyValue("Name");

                if (partName == null)
                    return;

                string modelName = (string)sel.GetPropertyValue("ModelName");

                if (modelName == null)
                    return;

                string[] names = (string[])sel.GetPropertyValue("PartNames");

                if (names == null)
                    return;

                string[] newNames = new string[names.Length];

                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i];

                    if (name == null)
                        continue;

                    // Name is a AEG reference
                    if (name.Contains(modelName) && 
                        (name.Contains("AEG") || name.Contains("aeg")))
                    {
                        name = partName;
                    }

                    newNames[i] = name;
                }

                sel.SetPropertyValue("PartNames", newNames);
            }
        }

        if (view.Project.Descriptor.ProjectType == ProjectType.NR)
        {
            if (sel.WrappedObject is MSB_NR.Part.Asset)
            {
                string partName = (string)sel.GetPropertyValue("Name");

                if (partName == null)
                    return;

                string modelName = (string)sel.GetPropertyValue("ModelName");

                if (modelName == null)
                    return;

                string partName1 = (string)sel.GetPropertyValue("PartName1");

                if (partName1 != null)
                {
                    if (partName1.Contains(modelName) &&
                        (partName1.Contains("AEG") || partName1.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName1", partName);
                    }
                }

                string partName2 = (string)sel.GetPropertyValue("PartName2");

                if (partName2 != null)
                {
                    if (partName2.Contains(modelName) &&
                        (partName2.Contains("AEG") || partName2.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName2", partName);
                    }
                }

                string partName3 = (string)sel.GetPropertyValue("PartName3");

                if (partName3 != null)
                {
                    if (partName3.Contains(modelName) &&
                        (partName3.Contains("AEG") || partName3.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName3", partName);
                    }
                }

                string partName4 = (string)sel.GetPropertyValue("PartName4");

                if (partName4 != null)
                {
                    if (partName4.Contains(modelName) &&
                        (partName4.Contains("AEG") || partName4.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName4", partName);
                    }
                }

                string partName5 = (string)sel.GetPropertyValue("PartName5");

                if (partName5 != null)
                {
                    if (partName5.Contains(modelName) &&
                        (partName5.Contains("AEG") || partName5.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName5", partName);
                    }
                }

                string partName6 = (string)sel.GetPropertyValue("PartName6");

                if (partName6 != null)
                {
                    if (partName6.Contains(modelName) &&
                        (partName6.Contains("AEG") || partName6.Contains("aeg")))
                    {
                        sel.SetPropertyValue("PartName6", partName);
                    }
                }
            }
        }
    }

    public static void SetUniqueInstanceID(MapEditorView view, MsbEntity ent, MapContainer m, UniqueIdCache cache = null)
    {
        if (view.Project.Descriptor.ProjectType == ProjectType.ER)
        {
            if (ent.WrappedObject is MSBE.Part msbePart)
            {
                msbePart.InstanceID = GetUniqueInstanceID(m, cache, msbePart.ModelName, msbePart.InstanceID, GetInstanceKey_ER);
            }
        }

        if (view.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            if (ent.WrappedObject is MSB_AC6.Part msbPart)
            {
                msbPart.TypeIndex = GetUniqueInstanceID(m, cache, "", msbPart.TypeIndex, GetInstanceKey_AC6);
            }
        }

        if (view.Project.Descriptor.ProjectType == ProjectType.NR)
        {
            if (ent.WrappedObject is MSB_NR.Part msbNrPart)
            {
                msbNrPart.InstanceID = GetUniqueInstanceID(m, cache, msbNrPart.ModelName, msbNrPart.InstanceID, GetInstanceKey_NR);
            }
        }
    }

    private static (string Group, int ID)? GetInstanceKey_ER(object obj)
    {
        return obj is MSBE.Part part ? (part.ModelName, part.InstanceID) : null;
    }

    private static (string Group, int ID)? GetInstanceKey_AC6(object obj)
    {
        return obj is MSB_AC6.Part part ? ("", part.TypeIndex) : null;
    }

    private static (string Group, int ID)? GetInstanceKey_NR(object obj)
    {
        return obj is MSB_NR.Part part ? (part.ModelName, part.InstanceID) : null;
    }

    /// <summary>
    /// Gets the next ID after the passed one that isn't used by another part of the group.
    /// </summary>
    private static int GetUniqueInstanceID(MapContainer m, UniqueIdCache cache, string group, int currentID,
        Func<object, (string Group, int ID)?> getKey)
    {
        var usedIDs = cache?.InstanceIDs;

        if (usedIDs == null)
        {
            usedIDs = new();

            foreach (Entity tent in m.Objects)
            {
                if (getKey(tent.WrappedObject) is var (partGroup, id))
                {
                    GetInstanceGroup(usedIDs, partGroup).Add(id);
                }
            }

            if (cache != null)
                cache.InstanceIDs = usedIDs;
        }

        var used = GetInstanceGroup(usedIDs, group);

        // A cached part is already on the map, and the cache may not have been built with it, so its own ID is skipped
        var newInstanceID = cache != null ? currentID + 1 : currentID;
        while (used.Contains(newInstanceID))
        {
            newInstanceID++;
        }

        used.Add(newInstanceID);

        return newInstanceID;
    }

    private static HashSet<int> GetInstanceGroup(Dictionary<string, HashSet<int>> usedIDs, string group)
    {
        group ??= "";

        if (!usedIDs.TryGetValue(group, out var used))
        {
            used = new();
            usedIDs[group] = used;
        }

        return used;
    }

    public static void SetSpecificEntityGroupID(MapEditorView view, MsbEntity ent, MapContainer m)
    {
        if (view.Project.Descriptor.ProjectType == ProjectType.NR)
        {
            var newID = (uint)CFG.Current.Prefab_SpecificEntityGroupID;
            var added = false;

            var part = ent.WrappedObject as MSB_NR.Part;

            uint[] newEntityGroupIDs = (uint[])ent.GetPropertyValue("EntityGroupIDs");

            if (newEntityGroupIDs == null)
                return;

            uint[] newEntityGroupIds = new uint[newEntityGroupIDs.Length];

            for (int i = 0; i < newEntityGroupIds.Length; i++)
            {
                var curEntityGroupId = newEntityGroupIds[i];

                if (!added && curEntityGroupId == 0)
                {
                    added = true;
                    newEntityGroupIDs[i] = newID;
                }
            }

            ent.SetPropertyValue("EntityGroupIDs", newEntityGroupIds);
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            var newID = (uint)CFG.Current.Prefab_SpecificEntityGroupID;
            var added = false;

            var part = ent.WrappedObject as MSB_AC6.Part;

            uint[] newEntityGroupIDs = new uint[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];

                if (!added && part.EntityGroupIDs[i] == 0)
                {
                    added = true;
                    newEntityGroupIDs[i] = newID;
                }
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.ER)
        {
            var newID = (uint)CFG.Current.Prefab_SpecificEntityGroupID;
            var added = false;

            var part = ent.WrappedObject as MSBE.Part;

            uint[] newEntityGroupIDs = new uint[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];

                if (!added && part.EntityGroupIDs[i] == 0)
                {
                    added = true;
                    newEntityGroupIDs[i] = newID;
                }
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.DS3)
        {
            var newID = CFG.Current.Prefab_SpecificEntityGroupID;
            var added = false;

            var part = ent.WrappedObject as MSB3.Part;

            int[] newEntityGroupIDs = new int[part.EntityGroups.Length];

            for (int i = 0; i < part.EntityGroups.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroups[i];

                if (!added && part.EntityGroups[i] == -1)
                {
                    added = true;
                    newEntityGroupIDs[i] = newID;
                }
            }

            part.EntityGroups = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.SDT)
        {
            var newID = CFG.Current.Prefab_SpecificEntityGroupID;
            var added = false;

            var part = ent.WrappedObject as MSBS.Part;

            int[] newEntityGroupIDs = new int[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];

                if (!added && part.EntityGroupIDs[i] == -1)
                {
                    added = true;
                    newEntityGroupIDs[i] = newID;
                }
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
    }

    public static void ClearEntityID(MapEditorView view, MsbEntity sel, MapContainer map)
    {
        if (view.Project.Descriptor.ProjectType == ProjectType.DS2S || view.Project.Descriptor.ProjectType == ProjectType.DS2)
            return;

        if (view.Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
        {
            ClearEntityID_UINT(sel, map);
        }
        else
        {
            ClearEntityID_INT(sel, map);
        }
    }

    public static void ClearEntityID_UINT(MsbEntity sel, MapContainer map)
    {
        sel.SetPropertyValue("EntityID", (uint)0);
    }

    public static void ClearEntityID_INT(MsbEntity sel, MapContainer map)
    {
        sel.SetPropertyValue("EntityID", 0);
    }

    public static void ClearEntityGroupID(MapEditorView view, MsbEntity ent, MapContainer map)
    {
        if (view.Project.Descriptor.ProjectType == ProjectType.DS2S || view.Project.Descriptor.ProjectType == ProjectType.DS2)
            return;

        if (view.Project.Descriptor.ProjectType == ProjectType.NR)
        {
            var part = ent.WrappedObject as MSB_NR.Part;

            uint[] newEntityGroupIDs = (uint[])ent.GetPropertyValue("EntityGroupIDs");

            if (newEntityGroupIDs == null)
                return;

            uint[] newEntityGroupIds = new uint[newEntityGroupIDs.Length];

            for (int i = 0; i < newEntityGroupIds.Length; i++)
            {
                newEntityGroupIDs[i] = 0;
            }

            ent.SetPropertyValue("EntityGroupIDs", newEntityGroupIds);
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            var part = ent.WrappedObject as MSB_AC6.Part;

            uint[] newEntityGroupIDs = new uint[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];
                newEntityGroupIDs[i] = 0;
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.ER)
        {
            var part = ent.WrappedObject as MSBE.Part;

            uint[] newEntityGroupIDs = new uint[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];
                newEntityGroupIDs[i] = 0;
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.DS3)
        {
            var part = ent.WrappedObject as MSB3.Part;

            int[] newEntityGroupIDs = new int[part.EntityGroups.Length];

            for (int i = 0; i < part.EntityGroups.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroups[i];
                newEntityGroupIDs[i] = 0;
            }

            part.EntityGroups = newEntityGroupIDs;
        }
        else if (view.Project.Descriptor.ProjectType == ProjectType.SDT)
        {
            var part = ent.WrappedObject as MSBS.Part;

            int[] newEntityGroupIDs = new int[part.EntityGroupIDs.Length];

            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
            {
                newEntityGroupIDs[i] = part.EntityGroupIDs[i];
                newEntityGroupIDs[i] = 0;
            }

            part.EntityGroupIDs = newEntityGroupIDs;
        }
    }

    public static Regex WithIndex = new Regex(@"^(.+)_(\d+)$");
    public static void SetNameHandleDuplicate(MapContainer map, IEnumerable<MsbEntity> entities, MsbEntity target, string name)
    {
        var baseName = name;
        string postfixId = null;

        if (map.GetObjectByName(baseName) is not null)
        {
            var match = WithIndex.Match(baseName);

            if (match.Success)
            {
                baseName = match.Groups[1].Value;
                postfixId = match.Groups[2].Value;
            }

            if (target.WrappedObject.GetType().GetProperty("InstanceID") is PropertyInfo prop)
            {
                postfixId = $"{prop.GetValue(target.WrappedObject)}";
            }
        }

        var newName = baseName;

        if (postfixId is not null)
            newName += $"_{postfixId}";

        if (map.GetObjectByName(newName) is not null)
        {
            try
            {
                var postId = int.Parse(postfixId);

                int count = postId;
                do
                {
                    newName = $"{baseName}_{PadID(count)}";

                    count += 1;
                }
                while (map.GetObjectByName(newName) is not null || entities.Any(ent => ent.Name == newName));
            }
            catch
            {

            }
        }

        if (target.WrappedObject is not BTL.Light)
        {
            MsbUtils.RenameWithRefs(
                entities.Select(ent => ent.WrappedObject as IMsbEntry),
                target.WrappedObject as IMsbEntry,
                newName
            );
        }
        target.Name = newName;
    }

    private static string PadID(int count)
    {
        if (count < 10)
        {
            return $"000{count}";
        }
        else if (count >= 10 && count < 100)
        {
            return $"00{count}";
        }
        else if (count >= 100 && count < 1000)
        {
            return $"0{count}";
        }

        return $"{count}";
    }

    public static void RenameDuplicates(MapContainer map, IEnumerable<MsbEntity> entities, MsbEntity target)
    {
        if (map.GetObjectByName(target.Name) is not null)
        {
            SetNameHandleDuplicate(map, entities, target, target.Name);
        }
    }
}
