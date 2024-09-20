using Andre.Formats;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.Editors.EmevdEditor
{
    public class EmevdDecorator
    {
        private EmevdEditorScreen Screen;

        private Instruction Instruction;
        private List<ArgDataObject> Arguments;

        public EmevdDecorator(EmevdEditorScreen screen)
        {
            Screen = screen;
        }

        public void OnProjectChanged()
        {

        }

        // Store this for the current instruction loop so we can determine stuff such as Item Type
        public void StoreInstructionInfo(Instruction instruction, List<ArgDataObject> arguments)
        {
            Instruction = instruction;
            Arguments = arguments;
        }

        #region Param Reference
        // HasParamReference, DetermineParamReferenceSpacing, DetermineParamReference
        // These three functions handle the cross-referencing, emulating the ParamMETA functionality used in the Param Editor.
        // This is done in-code rather than within the EMEDF for now

        public bool HasParamReference(string parameterName)
        {
            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (parameterName == "Bullet ID" ||
                    parameterName == "DamageParam ID" ||
                    parameterName == "ChrFullBodySFXParam ID" ||
                    parameterName == "Head Armor ID" ||
                    parameterName == "Chest Armor ID" ||
                    parameterName == "Arm Armor ID" ||
                    parameterName == "Leg Armor ID")
                {
                    return true;
                }
            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "SpEffect ID")
                {
                    return true;
                }

                if (parameterName == "Action Button Parameter ID")
                {
                    return true;
                }

                if (parameterName == "Item ID" )
                {
                    return true;
                }
            }

            return false;
        }

        public void DetermineParamReferenceSpacing(string parameterName, string value, int i)
        {
            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (parameterName == "Bullet ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }

                if (parameterName == "DamageParam ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }

                if (parameterName == "ChrFullBodySFXParam ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }

                if (parameterName == "Head Armor ID" || parameterName == "Chest Armor ID" || parameterName == "Arm Armor ID" || parameterName == "Leg Armor ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }
            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "SpEffect ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }

                if (parameterName == "Action Button Parameter ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }

                if (parameterName == "Item ID")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");
                }
            }
        }

        public void DetermineParamReference(string parameterName, string value, int i)
        {
            var currentEventID = EmevdUtils.GetCurrentEventID(); // For cross-ESD SpEffect stuff that AC6 does, which aligns with EventID

            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (parameterName == "Bullet ID")
                {
                    ConstructParamReference("BulletParam", value, i);
                    ConstructParamReference("EnemyBulletParam", value, i);
                }

                if (parameterName == "DamageParam ID")
                {
                    ConstructParamReference("PlayerDamageParam", value, i);
                    ConstructParamReference("EnemyDamageParam", value, i);
                }

                if (parameterName == "ChrFullBodySFXParam ID")
                {
                    ConstructParamReference("ChrFullBodySfxParam", value, i);
                }

                if (parameterName == "Head Armor ID" || 
                    parameterName == "Chest Armor ID" || 
                    parameterName == "Arm Armor ID" || 
                    parameterName == "Leg Armor ID")
                {
                    ConstructParamReference("ItemParam", value, i);
                }
            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "SpEffect ID")
                {
                    // TODO: AC6 seems to get the actual SpEffect via ESD shenanigans when this is set to 0

                    if (value == "0")
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"ESD");
                    }
                    else
                    {
                        ConstructParamReference("SpEffectParam", value, i);
                    }
                }

                if (parameterName == "Action Button Parameter ID")
                {
                    ConstructParamReference("ActionButtonParam", value, i);
                }

                if (parameterName == "Item ID")
                {
                    foreach(var entry in Arguments)
                    {
                        if(entry.ArgDoc.DisplayName == "Item Type")
                        {
                            string typeValue = entry.ArgObject as string;
                            switch(typeValue)
                            {
                                case "0": ConstructParamReference("EquipParamWeapon", value, i); break;
                                case "1": ConstructParamReference("EquipParamProtector", value, i); break;
                                case "2": ConstructParamReference("EquipParamAccessory", value, i); break;
                                case "3": ConstructParamReference("EquipParamGoods", value, i); break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Text Reference
        public bool HasTextReference(string parameterName)
        {
            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {

            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "Name ID")
                {
                    return true;
                }
            }

            return false;
        }

        public void DetermineTextReferenceSpacing(string parameterName, string value, int i)
        {
            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "Name ID")
                {
                    ImGui.Text("");
                }
            }
        }

        public void DetermineTextReference(string parameterName, string value, int i)
        {
            // DS1
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
            }

            // DS2
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
            }

            // DS3
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
            }

            // BB
            if (Smithbox.ProjectType is ProjectType.BB)
            {
            }

            // SDT
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
            }

            // ER
            if (Smithbox.ProjectType is ProjectType.ER)
            {
            }

            // AC6
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                if (parameterName == "Name ID")
                {
                    ConstructTextReference("TitleCharacters", value, i);
                }
            }
        }
        #endregion

        #region Alias Reference
        public bool HasAliasReference(string parameterName)
        {
            if (IsFlagParameter(parameterName))
            {
                return true;
            }
            if (IsParticleParameter(parameterName))
            {
                return true;
            }
            if (IsSoundParameter(parameterName))
            {
                return true;
            }

            return false;
        }

        public void DetermineAliasReferenceSpacing(string parameterName, string value, int i)
        {
            if (IsFlagParameter(parameterName))
            {
                ImGui.Text("");
            }
            if (IsParticleParameter(parameterName))
            {
                ImGui.Text("");
            }
            if (IsSoundParameter(parameterName))
            {
                ImGui.Text("");
            }
        }

        public void DetermineAliasReference(string parameterName, string value, int i)
        {
            if (IsFlagParameter(parameterName))
            {
                var entries = Smithbox.BankHandler.EventFlagAliases.Aliases.list;
                if (entries != null)
                {
                    foreach(var entry in entries)
                    {
                        if(entry.id == value)
                        {
                            ImGui.AlignTextToFramePadding();
                            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.name}");
                        }
                        else
                        {
                            //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                        }
                    }
                }
            }

            if (IsParticleParameter(parameterName))
            {
                var entries = Smithbox.BankHandler.ParticleAliases.Aliases.list;
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.id == value)
                        {
                            ImGui.AlignTextToFramePadding();
                            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.name}");
                        }
                        else
                        {
                            //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                        }
                    }
                }
            }

            if (IsSoundParameter(parameterName))
            {
                var entries = Smithbox.BankHandler.SoundAliases.Aliases.list;
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.id == value)
                        {
                            ImGui.AlignTextToFramePadding();
                            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.name}");
                        }
                        else
                        {
                            //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                        }
                    }
                }
            }

        }
        #endregion

        #region Entity Reference
        public bool HasEntityReference(string parameterName)
        {
            if (IsEntityParameter(parameterName))
            {
                return true;
            }

            return false;
        }

        public void DetermineEntityReferenceSpacing(string parameterName, string value, int i)
        {
            if (IsEntityParameter(parameterName))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }
        }


        public void DetermineEntityReference(string parameterName, string value, int i)
        {
            var mapID = EmevdUtils.GetCurrentFileID(); // To determine map ID

            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                // Referring to base map from mission EMEVD
                if (value.Length > 4)
                {
                    var id = value.Substring(0, 2);
                    var block = value.Substring(2, 2);

                    mapID = $"m{id}_{block}_00_00";
                }

                // Referring to base map
                if (value.Length == 10)
                {
                    // Ignore the start: 21
                    var id = value.Substring(2, 2);
                    var block = value.Substring(4, 2);

                    mapID = $"m{id}_{block}_00_00";

                }
            }

            if (IsEntityParameter(parameterName))
            {
                if (!IsSpecialEntityID(value))
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "View in Map");

                    // Context Menu for param ref
                    if (ImGui.BeginPopupContextItem($"EntityContextMenu_{parameterName}_{i}"))
                    {
                        if (ImGui.Selectable($"View"))
                        {
                            EditorCommandQueue.AddCommand($"map/load/{mapID}");
                            EditorCommandQueue.AddCommand($"map/emevd_select/{mapID}/{value}");
                        }

                        ImGui.EndPopup();
                    }
                }
                else if(value == "10000" || value == "10010" || value == "0")
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Self");
                }
                else if (value == "10002" || value == "10012")
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Host Player");
                }
                else if (value == "10003" || value == "10013" ||
                         value == "10004" || value == "10014" ||
                         value == "10005" || value == "10015")
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Client Player");
                }
                else if (value == "20000")
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Any Player");
                }
            }
        }
        #endregion

        #region Enum Reference
        public string enumSearchStr = "";

        public void DisplayEnumReference(ArgDataObject arg, int i)
        {
            var enumDoc = EmevdBank.InfoBank.Enums.Where(e => e.Name == arg.ArgDoc.EnumName).FirstOrDefault();
            var alias = enumDoc.Values.Where(e => e.Key == $"{arg.ArgObject}").FirstOrDefault();

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{alias.Value}");

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
        #endregion

        #region Utils
        private bool IsSpecialEntityID(string value)
        {
            if (value == "0" || 
                value == "10000" ||
                value == "10002" ||
                value == "10003" ||
                value == "10004" ||
                value == "10005" ||
                value == "10010" ||
                value == "10012" ||
                value == "10013" ||
                value == "10014" ||
                value == "10015" ||
                value == "20000")
            {
                return true;
            }

            return false;
        }

        private bool IsSoundParameter(string parameterName)
        {
            if (parameterName == "Sound ID")
            {
                return true;
            }

            return false;
        }

        private bool IsParticleParameter(string parameterName)
        {
            if (parameterName == "SFX ID")
            {
                return true;
            }

            return false;
        }

        private bool IsFlagParameter(string parameterName)
        {
            if (parameterName == "Target Event Flag ID" ||
                parameterName == "Starting Target Event Flag ID" ||
                parameterName == "Ending Target Event Flag ID" ||
                parameterName == "Base Event Flag ID" ||
                parameterName == "Right-side Base Event Flag ID" ||
                parameterName == "Left-side Base Event Flag ID")
            {
                return true;
            }

            return false;
        }

        private bool IsEntityParameter(string parameterName)
        {
            if (parameterName == "Entity ID" ||
                parameterName == "Character Entity ID" ||
                parameterName == "Target Asset Entity ID" ||
                parameterName == "Hit Entity ID" ||
                parameterName == "Target Entity ID" ||
                parameterName == "Area Entity ID" ||
                parameterName == "Target Entity ID A" ||
                parameterName == "Target Entity ID B" ||
                parameterName == "Attacker Entity ID" ||
                parameterName == "Warp Destination Entity ID")
            {
                return true;
            }

            return false;
        }

        private void ConstructParamReference(string paramName, string value, int i)
        {
            var refValue = int.Parse(value);

            (string, Param.Row, string) match = ResolveParamRef(ParamBank.PrimaryBank, paramName, refValue);

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{match.Item3}");

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

        private void ConstructTextReference(string fmgName, string value, int i)
        {
            if (!Smithbox.BankHandler.FMGBank.IsLoaded)
            {
                return;
            }

            var refValue = int.Parse(value);

            (string, FMGEntryGroup) match = resolveFMGRef(fmgName, refValue);

            if(match.Item2 != null)
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{match.Item2.Title.Text}");

                if (ImGui.BeginPopupContextItem($"TextContextMenu_{fmgName}_{i}"))
                {
                    if (ImGui.Selectable($"Go to {match.Item2.ID} ({match.Item2.Title})"))
                    {
                        EditorCommandQueue.AddCommand($@"text/select/{fmgName}/{match.Item2.ID}");
                    }

                    ImGui.EndPopup();
                }
            }


        }

        private (string, Param.Row, string) ResolveParamRef(ParamBank bank, string paramRef, dynamic oldval)
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

        private static (string, FMGEntryGroup) resolveFMGRef(string fmgName, int value, int offset = 0)
        {
            if (!Smithbox.BankHandler.FMGBank.IsLoaded)
            {
                return ("", null);
            }

            var matchingFmgInfo = Smithbox.BankHandler.FMGBank.FmgInfoBank.ToList().Find(x => x.Name == fmgName);

            if (matchingFmgInfo != null)
            {
                var entryGroupId = Math.Abs(value) + offset;

                var newFmgInfo = (matchingFmgInfo.Name, Smithbox.BankHandler.FMGBank.GenerateEntryGroup(entryGroupId, matchingFmgInfo));

                return newFmgInfo;
            }

            return ("", null);
        }

        #endregion
    }
}
