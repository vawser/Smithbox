using Google.Protobuf.WellKnownTypes;
using HKX2;
using ImGuiNET;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor
{
    public static class GparamQuickEdit
    {
        public enum EditEffectType
        {
            Set,
            Add,
            Subtract,
            Multiply,
            SetByRow
        }

        public static string filterArguments = "";

        public static string commandArguments = "";

        public static GPARAM.IField SelectedParamField;

        private static string _filterString = "";
        private static string _commandString = "";

        private static bool[] filterTruth = null;

        public static void OnGui()
        {
            filterArguments = "" +
            "\n-----------------------------------" +
            $"\nFilters can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character." +
            "\n-----------------------------------" +
            "\n*" +
            "\nTargets all rows." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_ID}:<x>" +
            "\nTargets all rows with <x> ID." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_TimeOfDay}:<x>" +
            "\nTargets all rows with <x> Time of Day." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_Value}:[<x>]" +
            "\nTargets all rows with <x> Value." +
            "\nFor multi-values split them like so: [<x>,<x>]" +
            "\n-----------------------------------";

            commandArguments = "" +
            "\n-----------------------------------" +
            $"\nCommands can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_Set}:[<x>]" +
            "\nSets target rows to <x> Value." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_Add}:[<x>]" +
            "\nAdds <x> to the Value of the target rows." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_Subtract}:[<x>]" +
            "\nSubtracts <x> from the Value of the target rows." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_Multiply}:[<x>]" +
            "\nMultiplies the Value of the target rows by <x>." +
            "\n-----------------------------------" +
            $"\n{CFG.Current.Gparam_QuickEdit_SetByRow}:<x>" +
            "\nSets target rows to the Value of row ID <x>." +
            "\n-----------------------------------";

            ImGui.Text("Filter: ");
            ImguiUtils.ShowWideHoverTooltip($"Filter arguments:{filterArguments}");
            ImGui.SameLine();

            ImGui.InputText("##filterString", ref _filterString, 255);

            ImGui.Text("Command:");
            ImguiUtils.ShowWideHoverTooltip($"Command arguments:{commandArguments}");
            ImGui.SameLine();

            ImGui.InputText("##commandString", ref _commandString, 255);

            if(ImGui.Button("Execute"))
            {
                filterTruth = new bool[SelectedParamField.Values.Count];
                ExecuteQuickEdit();
            }
        }

        private static void ExecuteQuickEdit()
        {
            for (int i = 0; i < SelectedParamField.Values.Count; i++)
            {
                filterTruth[i] = false;
            }

            // Filter arguments
            var filters = _filterString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach(var filter in filters) 
            {
                FilterAll(filter);
                FilterId(filter);
                FilterTimeOfDay(filter);
                FilterValue(filter);
            }

            // Command arguments
            var commands = _commandString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach (var command in commands)
            {
                CommandAdjust(command, EditEffectType.Set);
                CommandAdjust(command, EditEffectType.Add);
                CommandAdjust(command, EditEffectType.Subtract);
                CommandAdjust(command, EditEffectType.Multiply);
                CommandAdjust(command, EditEffectType.SetByRow);
            }
        }

        private static void CommandAdjust(string commandArg, EditEffectType effectType)
        {
            //TaskLogs.AddLog(commandArg);

            Match setValueMatch = null;

            if(effectType == EditEffectType.Set)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Set}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Add)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Add}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Subtract)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Subtract}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Multiply)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Multiply}:\[(.*)\]");
            }
            if (effectType == EditEffectType.SetByRow)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_SetByRow}:\[(.*)\]");
            }

            if (setValueMatch == null)
            {
                return;
            }

            if (setValueMatch.Success && setValueMatch.Groups.Count >= 2)
            {
                string setValue = setValueMatch.Groups[1].Value;

                int rowsetId = -1;
                int.TryParse(setValue, out rowsetId);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    // Only change if it matches filter truth
                    if (filterTruth[i] == true)
                    {
                        GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                        //TaskLogs.AddLog($"entry: {entry.Id} {entry.Value}");

                        // INT
                        if (SelectedParamField is GPARAM.IntField intField)
                        {
                            int commandValue = -1;
                            var valid = int.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    intField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value INT {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = intField.Values[i].Value + commandValue;

                                    if (addValue > int.MaxValue)
                                        addValue = int.MaxValue;

                                    intField.Values[i].Value = addValue;

                                    //TaskLogs.AddLog($"Command: Add Value INT {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = intField.Values[i].Value - commandValue;

                                    if (subValue < int.MinValue)
                                        subValue = int.MinValue;

                                    intField.Values[i].Value = subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value INT {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = intField.Values[i].Value * commandValue;

                                    if (multValue > int.MaxValue)
                                        multValue = int.MaxValue;

                                    if (multValue < int.MinValue)
                                        multValue = int.MinValue;

                                    intField.Values[i].Value = multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value INT {multValue} - {entry.Id}");
                                }
                                if(effectType == EditEffectType.SetByRow)
                                {
                                    if (intField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        intField.Values[i].Value = intField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set INT {intField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // UINT
                        else if (SelectedParamField is GPARAM.UintField uintField)
                        {
                            uint commandValue = 0;
                            var valid = uint.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    uintField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value UINT {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = uintField.Values[i].Value + commandValue;

                                    if (addValue > uint.MaxValue)
                                        addValue = uint.MaxValue;

                                    uintField.Values[i].Value = addValue;

                                    //TaskLogs.AddLog($"Command: Add Value UINT {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = uintField.Values[i].Value - commandValue;

                                    if (subValue < uint.MinValue)
                                        subValue = uint.MinValue;

                                    uintField.Values[i].Value = subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value UINT {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = uintField.Values[i].Value * commandValue;

                                    if (multValue > uint.MaxValue)
                                        multValue = uint.MaxValue;

                                    if (multValue < uint.MinValue)
                                        multValue = uint.MinValue;

                                    uintField.Values[i].Value = multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value UINT {multValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (uintField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        uintField.Values[i].Value = uintField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set UINT {uintField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // SHORT
                        else if (SelectedParamField is GPARAM.ShortField shortField)
                        {
                            short commandValue = 0;
                            var valid = short.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    shortField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value SHORT {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = shortField.Values[i].Value + commandValue;

                                    if (addValue > short.MaxValue)
                                        addValue = short.MaxValue;

                                    shortField.Values[i].Value = (short)addValue;

                                    //TaskLogs.AddLog($"Command: Add Value SHORT {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = shortField.Values[i].Value - commandValue;

                                    if (subValue < short.MinValue)
                                        subValue = short.MinValue;

                                    shortField.Values[i].Value = (short)subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value SHORT {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = shortField.Values[i].Value * commandValue;

                                    if (multValue > short.MaxValue)
                                        multValue = short.MaxValue;

                                    if (multValue < short.MinValue)
                                        multValue = short.MinValue;

                                    shortField.Values[i].Value = (short)multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value SHORT {multValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (shortField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        shortField.Values[i].Value = shortField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set SHORT {shortField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // SBYTE
                        else if (SelectedParamField is GPARAM.SbyteField sbyteField)
                        {
                            sbyte commandValue = 0;
                            var valid = sbyte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    sbyteField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value SBYTE {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = sbyteField.Values[i].Value + commandValue;

                                    if (addValue > sbyte.MaxValue)
                                        addValue = sbyte.MaxValue;

                                    sbyteField.Values[i].Value = (sbyte)addValue;

                                    //TaskLogs.AddLog($"Command: Add Value SBYTE {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = sbyteField.Values[i].Value - commandValue;

                                    if (subValue < sbyte.MinValue)
                                        subValue = sbyte.MinValue;

                                    sbyteField.Values[i].Value = (sbyte)subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value SBYTE {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = sbyteField.Values[i].Value * commandValue;

                                    if (multValue > sbyte.MaxValue)
                                        multValue = sbyte.MaxValue;

                                    if (multValue < sbyte.MinValue)
                                        multValue = sbyte.MinValue;

                                    sbyteField.Values[i].Value = (sbyte)multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value SBYTE {multValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (sbyteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        sbyteField.Values[i].Value = sbyteField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set SBYTE {sbyteField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // BYTE
                        else if (SelectedParamField is GPARAM.ByteField byteField)
                        {
                            byte commandValue = 0;
                            var valid = byte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    byteField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value BYTE {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = byteField.Values[i].Value + commandValue;

                                    if (addValue > byte.MaxValue)
                                        addValue = byte.MaxValue;

                                    byteField.Values[i].Value = (byte)addValue;

                                    //TaskLogs.AddLog($"Command: Add Value BYTE {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = byteField.Values[i].Value - commandValue;

                                    if (subValue < byte.MinValue)
                                        subValue = byte.MinValue;

                                    byteField.Values[i].Value = (byte)subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value BYTE {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = byteField.Values[i].Value * commandValue;

                                    if (multValue > byte.MaxValue)
                                        multValue = byte.MaxValue;

                                    if (multValue < byte.MinValue)
                                        multValue = byte.MinValue;

                                    byteField.Values[i].Value = (byte)multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value BYTE {multValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (byteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        byteField.Values[i].Value = byteField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set BYTE {byteField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // BOOL
                        else if (SelectedParamField is GPARAM.BoolField boolField)
                        {
                            int commandValue = 0;
                            var valid = int.TryParse(setValue, out commandValue);
                            var boolean = false;

                            if (commandValue == 1)
                                boolean = true;

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    boolField.Values[i].Value = boolean;

                                    //TaskLogs.AddLog($"Command: Set Value BOOL {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (boolField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        boolField.Values[i].Value = boolField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set BOOL {boolField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // FLOAT
                        else if (SelectedParamField is GPARAM.FloatField floatField)
                        {
                            float commandValue = 0.0f;
                            var valid = float.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    floatField.Values[i].Value = commandValue;

                                    //TaskLogs.AddLog($"Command: Set Value FLOAT {commandValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    var addValue = floatField.Values[i].Value + commandValue;

                                    if (addValue > float.MaxValue)
                                        addValue = float.MaxValue;

                                    floatField.Values[i].Value = addValue;

                                    //TaskLogs.AddLog($"Command: Add Value FLOAT {addValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    var subValue = floatField.Values[i].Value - commandValue;

                                    if (subValue < float.MinValue)
                                        subValue = float.MinValue;

                                    floatField.Values[i].Value = subValue;

                                    //TaskLogs.AddLog($"Command: Subtract Value FLOAT {subValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    var multValue = floatField.Values[i].Value * commandValue;

                                    if (multValue > float.MaxValue)
                                        multValue = float.MaxValue;

                                    if (multValue < float.MinValue)
                                        multValue = float.MinValue;

                                    floatField.Values[i].Value = multValue;

                                    //TaskLogs.AddLog($"Command: Multiply Value FLOAT {multValue} - {entry.Id}");
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (floatField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        floatField.Values[i].Value = floatField.Values.Find(x => x.Id == rowsetId).Value;
                                        //TaskLogs.AddLog($"Command: Row Set FLOAT {floatField.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // VECTOR2
                        else if (SelectedParamField is GPARAM.Vector2Field vector2Field)
                        {
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector2Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    vector2Field.Values[i].Value = vector2Field.Values.Find(x => x.Id == rowsetId).Value;
                                    //TaskLogs.AddLog($"Command: Row Set VECTOR2 {vector2Field.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 1)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                if(valid1 && valid2)
                                {
                                    if (effectType == EditEffectType.Set)
                                    {
                                        Vector2 commandVector = new Vector2(commandValue1, commandValue2);
                                        vector2Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Set Value VECTOR2 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        var addValue1 = vector2Field.Values[i].Value.X + commandValue1;
                                        var addValue2 = vector2Field.Values[i].Value.Y + commandValue2;

                                        if (addValue1 > float.MaxValue)
                                            addValue1 = float.MaxValue;
                                        
                                        if (addValue2 > float.MaxValue)
                                            addValue2 = float.MaxValue;

                                        Vector2 commandVector = new Vector2(addValue1, addValue2);
                                        vector2Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Add Value VECTOR2 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        var subValue1 = vector2Field.Values[i].Value.X - commandValue1;
                                        var subValue2 = vector2Field.Values[i].Value.Y - commandValue2;

                                        if (subValue1 < float.MinValue)
                                            subValue1 = float.MinValue;

                                        if (subValue2 < float.MinValue)
                                            subValue2 = float.MinValue;

                                        Vector2 commandVector = new Vector2(subValue1, subValue2);
                                        vector2Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Subtract Value VECTOR2 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        var multValue1 = vector2Field.Values[i].Value.X * commandValue1;
                                        var multValue2 = vector2Field.Values[i].Value.Y * commandValue2;

                                        if (multValue1 > float.MaxValue)
                                            multValue1 = float.MaxValue;

                                        if (multValue2 > float.MaxValue)
                                            multValue2 = float.MaxValue;

                                        if (multValue1 < float.MinValue)
                                            multValue1 = float.MinValue;

                                        if (multValue2 < float.MinValue)
                                            multValue2 = float.MinValue;

                                        Vector2 commandVector = new Vector2(multValue1, multValue2);
                                        vector2Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Multiply Value VECTOR2 {commandVector} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // VECTOR3
                        else if (SelectedParamField is GPARAM.Vector3Field vector3Field)
                        {
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector3Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    vector3Field.Values[i].Value = vector3Field.Values.Find(x => x.Id == rowsetId).Value;
                                    //TaskLogs.AddLog($"Command: Row Set VECTOR3 {vector3Field.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 2)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                float commandValue3 = 0.0f;
                                var valid3 = float.TryParse(parts[2], out commandValue3);

                                if (valid1 && valid2 && valid3)
                                {
                                    if (effectType == EditEffectType.Set)
                                    {
                                        Vector3 commandVector = new Vector3(commandValue1, commandValue2, commandValue3);
                                        vector3Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Set Value VECTOR3 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        var addValue1 = vector3Field.Values[i].Value.X + commandValue1;
                                        var addValue2 = vector3Field.Values[i].Value.Y + commandValue2;
                                        var addValue3 = vector3Field.Values[i].Value.Z + commandValue3;

                                        if (addValue1 > float.MaxValue)
                                            addValue1 = float.MaxValue;

                                        if (addValue2 > float.MaxValue)
                                            addValue2 = float.MaxValue;

                                        if (addValue3 > float.MaxValue)
                                            addValue3 = float.MaxValue;

                                        Vector3 commandVector = new Vector3(addValue1, addValue2, addValue3);
                                        vector3Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Add Value VECTOR3 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        var subValue1 = vector3Field.Values[i].Value.X - commandValue1;
                                        var subValue2 = vector3Field.Values[i].Value.Y - commandValue2;
                                        var subValue3 = vector3Field.Values[i].Value.Z - commandValue3;

                                        if (subValue1 < float.MinValue)
                                            subValue1 = float.MinValue;

                                        if (subValue2 < float.MinValue)
                                            subValue2 = float.MinValue;

                                        if (subValue3 < float.MinValue)
                                            subValue3 = float.MinValue;

                                        Vector3 commandVector = new Vector3(subValue1, subValue2, subValue3);
                                        vector3Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Subtract Value VECTOR3 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        var multValue1 = vector3Field.Values[i].Value.X * commandValue1;
                                        var multValue2 = vector3Field.Values[i].Value.Y * commandValue2;
                                        var multValue3 = vector3Field.Values[i].Value.Z * commandValue3;

                                        if (multValue1 > float.MaxValue)
                                            multValue1 = float.MaxValue;

                                        if (multValue2 > float.MaxValue)
                                            multValue2 = float.MaxValue;

                                        if (multValue3 > float.MaxValue)
                                            multValue3 = float.MaxValue;

                                        if (multValue1 < float.MinValue)
                                            multValue1 = float.MinValue;

                                        if (multValue2 < float.MinValue)
                                            multValue2 = float.MinValue;

                                        if (multValue3 < float.MinValue)
                                            multValue3 = float.MinValue;

                                        Vector3 commandVector = new Vector3(multValue1, multValue2, multValue2);
                                        vector3Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Multiply Value VECTOR3 {commandVector} - {entry.Id}");
                                    }
                                }
                            }
                        }
                        // VECTOR4
                        else if (SelectedParamField is GPARAM.Vector4Field vector4Field)
                        {
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector4Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    vector4Field.Values[i].Value = vector4Field.Values.Find(x => x.Id == rowsetId).Value;
                                    //TaskLogs.AddLog($"Command: Row Set VECTOR4 {vector4Field.Values.Find(x => x.Id == rowsetId).Value} - {entry.Id}");
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 3)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                float commandValue3 = 0.0f;
                                var valid3 = float.TryParse(parts[2], out commandValue3);

                                float commandValue4 = 0.0f;
                                var valid4 = float.TryParse(parts[3], out commandValue4);

                                if (valid1 && valid2 && valid3 && valid4)
                                {
                                    if (effectType == EditEffectType.Set)
                                    {
                                        Vector4 commandVector = new Vector4(commandValue1, commandValue2, commandValue3, commandValue4);
                                        vector4Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Set Value VECTOR4 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        var addValue1 = vector4Field.Values[i].Value.X + commandValue1;
                                        var addValue2 = vector4Field.Values[i].Value.Y + commandValue2;
                                        var addValue3 = vector4Field.Values[i].Value.Z + commandValue3;
                                        var addValue4 = vector4Field.Values[i].Value.W + commandValue4;

                                        if (addValue1 > float.MaxValue)
                                            addValue1 = float.MaxValue;

                                        if (addValue2 > float.MaxValue)
                                            addValue2 = float.MaxValue;

                                        if (addValue3 > float.MaxValue)
                                            addValue3 = float.MaxValue;

                                        if (addValue4 > float.MaxValue)
                                            addValue4 = float.MaxValue;

                                        Vector4 commandVector = new Vector4(addValue1, addValue2, addValue3, addValue4);
                                        vector4Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Add Value VECTOR4 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        var subValue1 = vector4Field.Values[i].Value.X - commandValue1;
                                        var subValue2 = vector4Field.Values[i].Value.Y - commandValue2;
                                        var subValue3 = vector4Field.Values[i].Value.Z - commandValue3;
                                        var subValue4 = vector4Field.Values[i].Value.W - commandValue4;

                                        if (subValue1 < float.MinValue)
                                            subValue1 = float.MinValue;

                                        if (subValue2 < float.MinValue)
                                            subValue2 = float.MinValue;

                                        if (subValue3 < float.MinValue)
                                            subValue3 = float.MinValue;

                                        if (subValue4 < float.MinValue)
                                            subValue4 = float.MinValue;

                                        Vector4 commandVector = new Vector4(subValue1, subValue2, subValue3, subValue4);
                                        vector4Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Subtract Value VECTOR4 {commandVector} - {entry.Id}");
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        var multValue1 = vector4Field.Values[i].Value.X * commandValue1;
                                        var multValue2 = vector4Field.Values[i].Value.Y * commandValue2;
                                        var multValue3 = vector4Field.Values[i].Value.Z * commandValue3;
                                        var multValue4 = vector4Field.Values[i].Value.W * commandValue4;

                                        if (multValue1 > float.MaxValue)
                                            multValue1 = float.MaxValue;

                                        if (multValue2 > float.MaxValue)
                                            multValue2 = float.MaxValue;

                                        if (multValue3 > float.MaxValue)
                                            multValue3 = float.MaxValue;

                                        if (multValue4 > float.MaxValue)
                                            multValue4 = float.MaxValue;

                                        if (multValue1 < float.MinValue)
                                            multValue1 = float.MinValue;

                                        if (multValue2 < float.MinValue)
                                            multValue2 = float.MinValue;

                                        if (multValue3 < float.MinValue)
                                            multValue3 = float.MinValue;

                                        if (multValue4 < float.MinValue)
                                            multValue4 = float.MinValue;

                                        Vector4 commandVector = new Vector4(multValue1, multValue2, multValue2, multValue4);
                                        vector4Field.Values[i].Value = commandVector;

                                        //TaskLogs.AddLog($"Command: Multiply Value VECTOR4 {commandVector} - {entry.Id}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FilterAll(string filterArg)
        {
            if (filterArg == "*")
            {
                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    filterTruth[i] = true;
                    //TaskLogs.AddLog($"Filter All: {entry.Id}");
                }
            }
        }

        private static void FilterId(string filterArg)
        {
            var idMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_ID}:([0-9]+)");

            if (idMatch.Success && idMatch.Groups.Count >= 2)
            {
                int targetId = -1;
                int.TryParse(idMatch.Groups[1].Value, out targetId);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    if (entry.Id == targetId)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched ID {targetId} - {entry.Id}");
                    }
                }
            }
        }

        private static void FilterTimeOfDay(string filterArg)
        {
            var todMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:([0-9]+)");

            if (todMatch.Success && todMatch.Groups.Count >= 2)
            {
                float targetTod = -1;
                float.TryParse(todMatch.Groups[1].Value, out targetTod);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    if (entry.Unk04 == targetTod)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched Time of Day {targetTod} - {entry.Id}");
                    }
                }
            }
        }

        private static void FilterValue(string filterArg)
        {
            var valueMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_Value}:\[(.*)\]");

            if (valueMatch.Success && valueMatch.Groups.Count >= 2)
            {
                string targetValue = valueMatch.Groups[1].Value;

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    // INT
                    if (SelectedParamField is GPARAM.IntField intField)
                    {
                        int fieldValue = intField.Values[i].Value;

                        int commandValue = -1;
                        var valid = int.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value INT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // UINT
                    else if (SelectedParamField is GPARAM.UintField uintField)
                    {
                        uint fieldValue = uintField.Values[i].Value;

                        uint commandValue = 0;
                        var valid = uint.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value UINT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // SHORT
                    else if (SelectedParamField is GPARAM.ShortField shortField)
                    {
                        short fieldValue = shortField.Values[i].Value;

                        short commandValue = 0;
                        var valid = short.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value SHORT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // SBYTE
                    else if (SelectedParamField is GPARAM.SbyteField sbyteField)
                    {
                        sbyte fieldValue = sbyteField.Values[i].Value;

                        sbyte commandValue = 0;
                        var valid = sbyte.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value SBYTE {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // BYTE
                    else if (SelectedParamField is GPARAM.ByteField byteField)
                    {
                        byte fieldValue = byteField.Values[i].Value;

                        byte commandValue = 0;
                        var valid = byte.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value BYTE {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // BOOL
                    else if (SelectedParamField is GPARAM.BoolField boolField)
                    {
                        bool fieldValue = boolField.Values[i].Value;

                        bool commandValue = false;
                        var valid = bool.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value BOOL {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // FLOAT
                    else if (SelectedParamField is GPARAM.FloatField floatField)
                    {
                        float fieldValue = floatField.Values[i].Value;

                        float commandValue = 0.0f;
                        var valid = float.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value FLOAT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // VECTOR2
                    else if (SelectedParamField is GPARAM.Vector2Field vector2Field)
                    {
                        Vector2 fieldValue = vector2Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 1)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            if (valid1 && valid2)
                            {
                                Vector2 commandVector = new Vector2(commandValue1, commandValue2);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR2 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                    // VECTOR3
                    else if (SelectedParamField is GPARAM.Vector3Field vector3Field)
                    {
                        Vector3 fieldValue = vector3Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 2)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            float commandValue3 = 0.0f;
                            var valid3 = float.TryParse(parts[2], out commandValue3);

                            if (valid1 && valid2 && valid3)
                            {
                                Vector3 commandVector = new Vector3(commandValue1, commandValue2, commandValue3);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR3 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                    // VECTOR4
                    else if (SelectedParamField is GPARAM.Vector4Field vector4Field)
                    {
                        Vector4 fieldValue = vector4Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 3)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            float commandValue3 = 0.0f;
                            var valid3 = float.TryParse(parts[2], out commandValue3);

                            float commandValue4 = 0.0f;
                            var valid4 = float.TryParse(parts[3], out commandValue4);


                            if (valid1 && valid2 && valid3 && valid4)
                            {
                                Vector4 commandVector = new Vector4(commandValue1, commandValue2, commandValue3, commandValue4);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR4 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
