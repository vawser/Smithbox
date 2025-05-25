using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Actions.Viewport;

/// <summary>
/// For functions that multiple EntityActions make use of.
/// </summary>
public static class ViewportActionCommon
{
    public static void SetUniqueEntityID(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        if (sel.WrappedObject is BTL.Light)
            return;

        if (editor.Project.ProjectType == ProjectType.DS2S || editor.Project.ProjectType == ProjectType.DS2)
            return;

        if (editor.Project.ProjectType == ProjectType.AC6)
        {
            SetUniqueEntityID_AC6(editor, sel, map);
        }
        else if (editor.Project.ProjectType == ProjectType.ER)
        {
            SetUniqueEntityID_ER(editor, sel, map);
        }
        else
        {
            SetUniqueEntityID_Int(editor, sel, map);
        }
    }

    public static void SetUniqueEntityID_AC6(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        uint originalID = (uint)sel.GetPropertyValue("EntityID");

        if (originalID == 0 )
            return;

        sel.SetPropertyValue("EntityID", (uint)0);

        HashSet<uint> vals = new();

        foreach (var e in map?.Objects)
        {
            var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
            if (val == null)
                continue;

            uint entUint;
            if (val is int entInt)
                entUint = (uint)entInt;
            else
                entUint = (uint)val;

            if (entUint == 0 || entUint == uint.MaxValue)
                continue;

            vals.Add(entUint);
        }

        var mapIdParts = map.Name.Replace("m", "").Split("_");

        uint minId = 100;
        uint maxId = 9999;

        // Build base entity ID list
        var baseVals = new HashSet<uint>();
        for (uint i = minId; i < maxId; i++)
        {
            baseVals.Add(i);
        }

        // Remove elements that are present in both hashsets, to get the list of unique IDs
        baseVals.SymmetricExceptWith(vals);

        bool hasMatch = false;
        uint newID = 0;

        // Prefer IDs after the original ID first
        foreach (var entry in baseVals)
        {
            if (!hasMatch)
            {
                if (entry > originalID)
                {
                    newID = entry;
                    hasMatch = true;
                }
            }
            else
            {
                break;
            }
        }

        // No match in preferred range, get first of possible values.
        if (!hasMatch)
        {
            newID = baseVals.First();
        }

        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetUniqueEntityID_ER(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        uint originalID = (uint)sel.GetPropertyValue("EntityID");

        // Don't set entity ID if the original entity doesn't have one
        if (originalID == 0)
            return;
        
        sel.SetPropertyValue("EntityID", (uint)0);

        HashSet<uint> vals = new();

        // For enemies, only fill vals with other enemy IDs, as ER enemies use 7 digits, not 8 like the other map objects
        foreach (var e in map?.Objects)
        {
            var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
            if (val == null)
                continue;

            uint entUint;
            if (val is int entInt)
                entUint = (uint)entInt;
            else
                entUint = (uint)val;

            if (entUint == 0 || entUint == uint.MaxValue)
                continue;

            vals.Add(entUint);
        }

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

        // Build base entity ID list
        var baseVals = new HashSet<uint>();
        for (uint i = minId; i < maxId; i++)
        {
            baseVals.Add(i);
        }

        // Remove elements that are present in both hashsets, to get the list of unique IDs
        baseVals.SymmetricExceptWith(vals);

        bool hasMatch = false;
        uint newID = 0;

        // Prefer IDs after the original ID first
        foreach (var entry in baseVals)
        {
            if (!hasMatch)
            {
                if (entry > originalID)
                {
                    newID = entry;
                    hasMatch = true;
                }
            }
            else
            {
                break;
            }
        }

        // No match in preferred range, get first of possible values.
        if (!hasMatch)
        {
            newID = baseVals.First();
        }

        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetUniqueEntityID_Int(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        int originalID = (int)sel.GetPropertyValue("EntityID");

        if (originalID == 0 || originalID == -1)
            return;

        sel.SetPropertyValue("EntityID", -1);

        HashSet<int> vals = new();

        // Get currently used Entity IDs
        foreach (var e in map?.Objects)
        {
            var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
            if (val == null)
                continue;

            int entInt = (int)val;

            if (entInt == 0 || entInt == int.MaxValue)
                continue;

            vals.Add(entInt);
        }

        // Build set of all 'valid' Entity IDs
        var mapIdParts = map.Name.Replace("m", "").Split("_");

        int minId = 0;
        int maxId = 9999;

        // Get the first non-zero digit from mapIdParts[1]
        var part = mapIdParts[1][1];

        minId = int.Parse($"{mapIdParts[0]}{part}0000");
        maxId = int.Parse($"{mapIdParts[0]}{part}9999");

        var baseVals = new HashSet<int>();
        for (int i = minId; i < maxId; i++)
        {
            baseVals.Add(i);
        }

        baseVals.SymmetricExceptWith(vals);

        bool hasMatch = false;
        int newID = 0;

        // Prefer IDs after the original ID first
        foreach (var entry in baseVals)
        {
            if (!hasMatch)
            {
                if (entry > originalID)
                {
                    newID = entry;
                    hasMatch = true;

                    // This is to ignore the 4 digit Entity IDs used in some DS1 maps
                    if (editor.Project.ProjectType == ProjectType.DS1 || editor.Project.ProjectType == ProjectType.DS1R)
                    {
                        if (newID < 10000)
                        {
                            hasMatch = false;
                        }
                    }
                }
            }
            else
            {
                break;
            }
        }

        // No match in preferred range, get first of possible values.
        if (!hasMatch)
        {
            newID = baseVals.First();
        }

        sel.SetPropertyValue("EntityID", newID);
    }

    public static void SetSelfPartNames(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        if (editor.Project.ProjectType == ProjectType.ER)
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
                    if (name.Contains(modelName) && name.Contains("AEG"))
                    {
                        name = partName;
                    }

                    newNames[i] = name;
                }

                sel.SetPropertyValue("PartNames", newNames);
            }
        }

        if (editor.Project.ProjectType == ProjectType.AC6)
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
                    if (name.Contains(modelName) && name.Contains("AEG"))
                    {
                        name = partName;
                    }

                    newNames[i] = name;
                }

                sel.SetPropertyValue("PartNames", newNames);
            }
        }
    }

    public static void SetUniqueInstanceID(MapEditorScreen editor, MsbEntity ent, MapContainer m)
    {
        if (editor.Project.ProjectType == ProjectType.ER)
        {
            Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

            if (ent.WrappedObject is MSBE.Part msbePart)
            {
                if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
                {
                    foreach (Entity tent in m.Objects)
                    {
                        if (ent.WrappedObject != null && tent.WrappedObject is MSBE.Part)
                        {
                            mapPartEntities[m].Add((MsbEntity)tent);
                        }
                    }
                }

                var newInstanceID = msbePart.InstanceID;
                while (mapPartEntities[m].FirstOrDefault(e =>
                           ((MSBE.Part)e.WrappedObject).ModelName == msbePart.ModelName
                           && ((MSBE.Part)e.WrappedObject).InstanceID == newInstanceID) != null)
                {
                    newInstanceID++;
                }

                msbePart.InstanceID = newInstanceID;
                mapPartEntities[m].Add(ent);
            }
        }

        if (editor.Project.ProjectType == ProjectType.AC6)
        {
            Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

            if (ent.WrappedObject is MSB_AC6.Part msbPart)
            {
                if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
                {
                    foreach (Entity tent in m.Objects)
                    {
                        if (ent.WrappedObject != null && tent.WrappedObject is MSB_AC6.Part)
                        {
                            mapPartEntities[m].Add((MsbEntity)tent);
                        }
                    }
                }

                var newInstanceID = msbPart.TypeIndex;
                while (mapPartEntities[m].FirstOrDefault(e => ((MSB_AC6.Part)e.WrappedObject).TypeIndex == newInstanceID) != null)
                {
                    newInstanceID++;
                }

                msbPart.TypeIndex = newInstanceID;
                mapPartEntities[m].Add(ent);
            }
        }
    }

    public static void SetSpecificEntityGroupID(MapEditorScreen editor, MsbEntity ent, MapContainer m)
    {
        if (editor.Project.ProjectType == ProjectType.AC6)
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
        else if (editor.Project.ProjectType == ProjectType.ER)
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
        else if (editor.Project.ProjectType == ProjectType.DS3)
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
        else if (editor.Project.ProjectType == ProjectType.SDT)
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

    public static void ClearEntityID(MapEditorScreen editor, MsbEntity sel, MapContainer map)
    {
        if (editor.Project.ProjectType == ProjectType.DS2S || editor.Project.ProjectType == ProjectType.DS2)
            return;

        if (editor.Project.ProjectType is ProjectType.AC6 or ProjectType.ER)
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

    public static void ClearEntityGroupID(MapEditorScreen editor, MsbEntity ent, MapContainer map)
    {
        if (editor.Project.ProjectType == ProjectType.DS2S || editor.Project.ProjectType == ProjectType.DS2)
            return;

        if (editor.Project.ProjectType == ProjectType.AC6)
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
        else if (editor.Project.ProjectType == ProjectType.ER)
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
        else if (editor.Project.ProjectType == ProjectType.DS3)
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
        else if (editor.Project.ProjectType == ProjectType.SDT)
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
