using Andre.Formats;
using ImGuiNET;
using Octokit;
using Org.BouncyCastle.Utilities;
using SoapstoneLib.Proto.Internal;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.EmevdEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSkinnedMeshShape;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static SoulsFormats.TAE;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public class EmevdInstructionHandler
{
    private EmevdEditorScreen Screen;

    public EmevdInstructionHandler(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public List<ArgDataObject> Arguments { get; set; }

    public static string enumSearchStr = "";

    public void Display()
    {
        if (Screen._selectedEvent != null && Screen._selectedInstruction != null)
        {
            var instruction = Screen._selectedInstruction;

            Arguments = BuildArgumentList(instruction);

            ImGui.Columns(2);

            // Names
            for(int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i];

                ImGui.Text($"{arg.ArgDoc.Name}");

                if(arg.ArgDoc.EnumName != null)
                {
                    ImGui.Text("");
                }

                if(HasParamReference(arg.ArgDoc.Name))
                {
                    DetermineParamReferenceSpacing(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }
            }

            ImGui.NextColumn();

            // Properties
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i];

                // TODO: add property edit
                ImGui.Text($"{arg.ArgObject}");

                // Enums
                if (arg.ArgDoc.EnumName != null)
                {
                    DisplayEnumReference(arg, i);
                    
                }

                // Param Reference
                if (HasParamReference(arg.ArgDoc.Name))
                {
                    DetermineParamReference(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }

                // TODO: Particle Alias
            }


            ImGui.Columns(1);
        }
    }

    private List<ArgDataObject> BuildArgumentList(Instruction ins)
    {
        var argList = new List<ArgDataObject>();

        var instructionDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault()[ins.ID];

        var data = ins.ArgData;

        List<ArgType> argTypes = instructionDoc.Arguments.Select(arg => arg.Type == 8 ? ArgType.UInt32 : (ArgType)arg.Type).ToList();

        var argObjects = UnpackArguments(ins, argTypes);

        for (int i = 0; i < instructionDoc.Arguments.Length; i++)
        {
            var entry = instructionDoc.Arguments[i];
            var obj = argObjects[i];

            argList.Add(new ArgDataObject(entry, obj));
        }

        return argList;
    }

    private List<object> UnpackArguments(Instruction ins, IEnumerable<ArgType> argStruct, bool bigEndian = false)
    {
        var result = new List<object>();
        using (var ms = new MemoryStream(ins.ArgData))
        {
            byte[] bytes = ms.ToArray();

            var br = new BinaryReaderEx(bigEndian, bytes);
            foreach (ArgType arg in argStruct)
            {
                switch (arg)
                {
                    case ArgType.Byte:
                        result.Add(br.ReadByte()); break;
                    case ArgType.UInt16:
                        AssertZeroPad(br, 2);
                        result.Add(br.ReadUInt16()); break;
                    case ArgType.UInt32:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadUInt32()); break;
                    case ArgType.SByte:
                        result.Add(br.ReadSByte()); break;
                    case ArgType.Int16:
                        AssertZeroPad(br, 2);
                        result.Add(br.ReadInt16()); break;
                    case ArgType.Int32:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadInt32()); break;
                    case ArgType.Single:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadSingle()); break;

                    default:
                        throw new NotImplementedException($"Unimplemented argument type: {arg}");
                }
            }
            AssertZeroPad(br, 4);
        }
        return result;
    }

    private void AssertZeroPad(BinaryReaderEx br, int align)
    {
        if (br.Position % align > 0)
        {
            br.AssertPattern(align - (int)(br.Position % align), 0);
        }
    }

    private void DisplayEnumReference(ArgDataObject arg, int i)
    {
        var enumDoc = EmevdBank.InfoBank.Enums.Where(e => e.Name == arg.ArgDoc.EnumName).FirstOrDefault();
        var alias = enumDoc.Values.Where(e => e.Key == $"{arg.ArgObject}").FirstOrDefault();

        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{alias.Value}");

        // Context Menu for enum
        if (ImGui.BeginPopupContextItem($"EnumContextMenu{i}"))
        {
            ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

            if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, enumDoc.Values.Count))))
            {
                try
                {
                    foreach (KeyValuePair<string, string> option in enumDoc.Values)
                    {
                        if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                            || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
                            || enumSearchStr == "")
                        {
                            if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                            {
                                var newval = Convert.ChangeType(option.Key, arg.ArgObject.GetType());
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    private bool HasParamReference(string parameterName)
    {
        // TODO: Particle Alias
        if ( parameterName == "Bullet ID" ||
            parameterName == "DamageParam ID" ||
            parameterName == "ChrFullBodySFXParam ID" ||
            parameterName == "Head Armor ID" || 
            parameterName == "Chest Armor ID" || 
            parameterName == "Arm Armor ID" || 
            parameterName == "Leg Armor ID")
        {
            return true;
        }

        return false;
    }

    private void DetermineParamReferenceSpacing(string parameterName, string value, int i)
    {
        if (parameterName == "Bullet ID")
        {
            ImGui.Text("");
            ImGui.Text("");
        }

        if (parameterName == "DamageParam ID")
        {
            ImGui.Text("");
            ImGui.Text("");
        }

        if (parameterName == "ChrFullBodySFXParam ID")
        {
            ImGui.Text("");
        }

        if (parameterName == "Head Armor ID" || parameterName == "Chest Armor ID" || parameterName == "Arm Armor ID" || parameterName == "Leg Armor ID")
        {
            ImGui.Text("");
        }

        // TODO: Particle Alias
    }

    // TODO: the data side should really be part of the EMEDF
    private void DetermineParamReference(string parameterName, string value, int i)
    {
        void ConstructParamReference(string paramName)
        {
            var refValue = int.Parse(value);

            (string, Param.Row, string) match = ResolveParamRef(ParamBank.PrimaryBank, paramName, refValue);

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{match.Item3}");

            // Context Menu for param ref
            if (ImGui.BeginPopupContextItem($"ParamContextMenu_{paramName}_{i}"))
            {
                if (ImGui.Selectable($"Go to {match.Item2.ID} ({match.Item3})"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{match.Item1}/{match.Item2.ID}");
                }

                ImGui.EndPopup();
            }
        }

        if (parameterName == "Bullet ID")
        {
            ConstructParamReference("BulletParam");
            ConstructParamReference("EnemyBulletParam");
        }

        if (parameterName == "DamageParam ID")
        {
            ConstructParamReference("PlayerDamageParam");
            ConstructParamReference("EnemyDamageParam");
        }

        if (parameterName == "ChrFullBodySFXParam ID")
        {
            ConstructParamReference("ChrFullBodySfxParam");
        }

        if (parameterName == "Head Armor ID" || parameterName == "Chest Armor ID" || parameterName == "Arm Armor ID" || parameterName == "Leg Armor ID")
        {
            ConstructParamReference("ItemParam");
        }
    }

    private static (string, Param.Row, string) ResolveParamRef(ParamBank bank, string paramRef, dynamic oldval)
    {
        (string, Param.Row, string) row = new();
        if (bank.Params == null)
        {
            return row;
        }

        var originalValue = (int)oldval; //make sure to explicitly cast from dynamic or C# complains. Object or Convert.ToInt32 fail.

        var hint = "";
        if (bank.Params.ContainsKey(paramRef))
        {
            var altval = originalValue;

            Param param = bank.Params[paramRef];
            ParamMetaData meta = ParamMetaData.Get(bank.Params[paramRef].AppliedParamdef);
            if (meta != null && meta.Row0Dummy && altval == 0)
            {
                return row;
            }

            Param.Row r = param[altval];
            if (r == null && altval > 0 && meta != null)
            {
                if (meta.FixedOffset != 0)
                {
                    altval = originalValue + meta.FixedOffset;
                    hint += meta.FixedOffset > 0 ? "+" + meta.FixedOffset : meta.FixedOffset.ToString();
                }

                if (meta.OffsetSize > 0)
                {
                    altval = altval - (altval % meta.OffsetSize);
                    hint += "+" + (originalValue % meta.OffsetSize);
                }

                r = bank.Params[paramRef][altval];
            }

            if (r == null)
            {
                return row;
            }

            if (string.IsNullOrWhiteSpace(r.Name))
            {
                row = ((paramRef, r, "Unnamed Row" + hint));
            }
            else
            {
                row = ((paramRef, r, r.Name + hint));
            }
        }

        return row;
    }
}

public class ArgDataObject
{
    public ArgDoc ArgDoc { get; set; }

    public object ArgObject { get; set; }

    public ArgDataObject(ArgDoc argDoc, object argObj)
    {
        ArgDoc = argDoc;
        ArgObject = argObj;
    }
}