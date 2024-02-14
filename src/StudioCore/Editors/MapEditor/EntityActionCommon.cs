using SoulsFormats;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    /// <summary>
    /// For functions that multiple EntityActions make use of.
    /// </summary>
    public static class EntityActionCommon
    {
        public static void SetUniqueEntityID(MsbEntity sel, Map map)
        {
            if (Project.Type == ProjectType.DS2S)
                return;

            if (Project.Type == ProjectType.AC6)
                return;

            if (Project.Type == ProjectType.ER)
            {
                SetUniqueEntityID_Uint(sel, map);
            }
            else
            {
                SetUniqueEntityID_Int(sel, map);
            }
        }

        // 10_01_101
        // 10_01_0011
        public static void SetUniqueEntityID_Uint(MsbEntity sel, Map map)
        {
            uint originalID = (uint)sel.GetPropertyValue("EntityID");
            sel.SetPropertyValue("EntityID", (uint)0);

            HashSet<uint> vals = new();

            // For enemies, only fill vals with other enemy IDs, as ER enemies use 7 digits, not 8 like the other map objects
            if (sel.WrappedObject is MSBE.Part.Enemy)
            {
                foreach (var e in map?.Objects)
                {
                    if (e.WrappedObject is MSBE.Part.Enemy)
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
                }
            }
            // Default val behavior
            else
            {
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

            // Enemies themselves don't use the 60 -> 10 substitution, and only have 7 digits
            if (sel.WrappedObject is MSBE.Part.Enemy)
            {
                minId = uint.Parse($"{mapIdParts[0]}{mapIdParts[1]}000");
                maxId = uint.Parse($"{mapIdParts[0]}{mapIdParts[1]}999");
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

        public static void SetUniqueEntityID_Int(MsbEntity sel, Map map)
        {
            int originalID = (int)sel.GetPropertyValue("EntityID");
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
                        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
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

        public static void SetSelfPartNames(MsbEntity sel, Map map)
        {
            if (Project.Type != ProjectType.ER)
                return;

            if (Project.Type == ProjectType.ER)
            {
                if (sel.WrappedObject is MSBE.Part.Asset)
                {
                    string partName = (string)sel.GetPropertyValue("Name");
                    string modelName = (string)sel.GetPropertyValue("ModelName");
                    string[] names = (string[])sel.GetPropertyValue("UnkPartNames");
                    string[] newNames = new string[names.Length];

                    for (int i = 0; i < names.Length; i++)
                    {
                        var name = names[i];

                        if (name != null)
                        {
                            // Name is a AEG reference
                            if (name.Contains(modelName) && name.Contains("AEG"))
                            {
                                TaskLogs.AddLog($"{name}");

                                name = partName;
                            }
                        }

                        newNames[i] = name;
                    }

                    sel.SetPropertyValue("UnkPartNames", newNames);
                }
            }
        }

        public static void SetUniqueInstanceID(MsbEntity ent, Map m)
        {
            if (Project.Type != ProjectType.ER)
                return;

            if (Project.Type == ProjectType.ER)
            {
                Dictionary<Map, HashSet<MsbEntity>> mapPartEntities = new();

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
        }

        public static void SetSpecificEntityID(MsbEntity ent, Map m)
        {

        }

        public static void SetSpecificEntityGroupID(MsbEntity ent, Map m)
        {

        }
    }
}
