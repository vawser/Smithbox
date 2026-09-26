using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.HavokEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Developer;

public static class HavokScratchpad
{
    public static void Display()
    {
        if (ImGui.BeginMenu("Havok"))
        {
            if (ImGui.Selectable("Inject Behavior Variables"))
            {
                DS3_HavokVariable_Util.Process();
            }

            ImGui.EndMenu();
        }
    }
}

public static class DS3_HavokVariable_Util
{
    public static HavokPropertyCache HavokPropertyCache = new();
    public static void Process()
    {
        var filePath = @"C:\Users\benja\Programming\Dump\c0000.behbnd.dcx";
        var writefilePath = @"C:\Users\benja\Programming\Dump\new_c0000.behbnd.dcx";
        var internalPath = @"N:\FDP\data\INTERROOT_win64\Action\c0000\Export\Behaviors\c0000.hkx";
        // Load
        var fileData = File.ReadAllBytes(filePath);

        HKX2.hkRootLevelContainer workingHkx = null;
        HKX2.PackFileDeserializer deserializer = new HKX2.PackFileDeserializer();
        HKX2.PackFileSerializer serializer = new HKX2.PackFileSerializer();

        var readBinder = new BND4Reader(fileData);
        foreach (var file in readBinder.Files)
        {
            if (file.Name != internalPath)
                continue;

            var fileBytes = readBinder.ReadFile(file).ToArray();

            using (MemoryStream memoryStream = new MemoryStream(fileBytes))
            {
                try
                {
                    var br = new BinaryReaderEx(false, memoryStream.ToArray());
                    workingHkx = (HKX2.hkRootLevelContainer)deserializer.Deserialize(br);
                }
                catch (InvalidDataException) { }
            }
        }
        readBinder.Dispose();

        InjectVariable(workingHkx);

        // Save
        var writeBinder = BND4.Read(fileData);
        foreach (var file in writeBinder.Files)
        {
            if (file.Name != internalPath)
                continue;

            using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
            {
                if (workingHkx != null)
                {
                    var bw = new BinaryWriterEx(false);
                    serializer.Serialize((HKX2.IHavokObject)workingHkx, bw);

                    file.Bytes = bw.FinishBytes();
                }
            }
        }

        File.WriteAllBytes(writefilePath, writeBinder.Write());
    }

    public static void InjectVariable(HKX2.hkRootLevelContainer root)
    {
        var varIndex = -1;

        // Add the variable setup
        var behaviorGraphs = HavokTreeSearch.FindAll<HKX2.hkbBehaviorGraphData>(
                root, HavokPropertyCache.GetCachedHavokFields);

        var top = behaviorGraphs.FirstOrDefault();
        if (top != null)
        {
            var newVarInfo = new HKX2.hkbVariableInfo()
            {
                m_role = new HKX2.hkbRoleAttribute()
                {
                    m_role = HKX2.Role.ROLE_DEFAULT,
                    m_flags = 0
                },
                m_type = HKX2.VariableType.VARIABLE_TYPE_REAL
            };

            top.m_variableInfos.Add(newVarInfo);

            var newVarBounds = new HKX2.hkbVariableBounds()
            {
                m_min = new HKX2.hkbVariableValue()
                {
                    m_value = 0
                },
                m_max = new HKX2.hkbVariableValue()
                {
                    m_value = 10
                },
            };

            top.m_variableBounds.Add(newVarBounds);

            var newWordVar = new HKX2.hkbVariableValue()
            {
                m_value = 1
            };

            top.m_variableInitialValues.m_wordVariableValues.Add(newWordVar);

            top.m_stringData.m_variableNames.Add("WeaponAnimSpeed");

            // This is the index to use for the clips
            varIndex = top.m_variableInitialValues.m_wordVariableValues.Count - 1;
        }

        if (varIndex != -1)
        {
            // Add variable binding to clips
            var clips = HavokTreeSearch.FindAll<HKX2.hkbClipGenerator>(
                    root, HavokPropertyCache.GetCachedHavokFields);

            var newBinding = new HKX2.hkbVariableBindingSetBinding()
            {
                m_variableIndex = varIndex,
                m_memberPath = "playbackSpeed",
                m_bitIndex = -1,
                m_bindingType = HKX2.BindingType.BINDING_TYPE_VARIABLE
            };

            var set = new HKX2.hkbVariableBindingSet()
            {
                m_bindings = new()
                {
                    newBinding
                },
                m_indexOfBindingToEnable = -1
            };

            foreach (var clip in clips)
            {
                //x000_000000
                var animIDstr = clip.m_name.Substring(5, 6);
                int.TryParse(animIDstr, out int animID);

                if (animID >= 30000 && animID <= 39999)
                {
                    if (clip.m_variableBindingSet == null)
                    {
                        clip.m_variableBindingSet = set;
                    }
                    else
                    {
                        clip.m_variableBindingSet.m_bindings.Add(newBinding);
                    }
                }
            }
        }
    }
}