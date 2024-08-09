using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    public static class MapEditorDecorations
    {
        public static bool GenericEnumRow(PropertyInfo propinfo, object val, ref object newVal)
        {
            if (propinfo.GetCustomAttribute<MSBEnum>() == null)
            {
                return false;
            }

            List<MSBEnum> attributes = propinfo.GetCustomAttributes<MSBEnum>().ToList();
            if (attributes.Any())
            {
                var enumName = attributes[0].EnumType;
                var temp = Smithbox.BankHandler.MSB_Info.Enums.list.Where(x => x.id == enumName).ToList()[0];

                if (temp == null)
                {
                    return false;
                }

                var options = temp.members;


                ImGui.NextColumn();

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
                ImGui.TextUnformatted(@$"   <{enumName}>");
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();

                ImGui.NextColumn();

                string currentEntry = "___";

                var match = options.Where(x => x.id == val.ToString()).FirstOrDefault();

                ImGui.BeginGroup();

                if (match != null)
                {
                    currentEntry = match.name;
                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_ParamRef_Text);
                    ImGui.TextUnformatted(currentEntry);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_ParamRefMissing_Text);
                    ImGui.TextUnformatted(currentEntry);
                    ImGui.PopStyleColor();
                }

                ImGui.EndGroup();

                if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
                {
                    var opened = MsbEnumContextMenu(propinfo, val, ref newVal, options);
                    ImGui.EndPopup();
                    return opened;
                }
            }

            return false;
        }

        private static string enumSearchStr = "";

        public static bool MsbEnumContextMenu(PropertyInfo propinfo, object val, ref object newVal, List<FormatEnumMember> options)
        {
            ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

            if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
            {
                try
                {
                    foreach (var entry in options)
                    {
                        if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.id, " ")
                            || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.name, " ")
                            || enumSearchStr == "")
                        {
                            if (ImGui.Selectable($"{entry.id}: {entry.name}"))
                            {
                                newVal = Convert.ChangeType(entry.id, val.GetType());
                                ImGui.EndChild();
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            ImGui.EndChild();
            return false;
        }

        public static bool MsbReferenceRow(PropertyInfo propInfo, object val, ref object newval, HashSet<Entity> entities)
        {
            var msbRef = propInfo.GetCustomAttribute<MSBReference>();
            if (msbRef == null) return false;

            var name = val as string;
            // Empty values are usually valid, so we just skip
            if (name == null || name == "") return false;

            var refName = msbRef.ReferenceType.Name;

            ImGui.NextColumn();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.TextUnformatted(@$"   <{refName}>");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
            ImGui.NextColumn();


            var maps = entities
                .Select((e) => e.Container)
                .Distinct();
            // We're bailing out on multiple maps
            if (maps.Count() != 1)
            {
                return false;
            }


            var map = maps.First();
            var entity = map.GetObjectByName((string)val);
            if (entity == null)
            {
                ImGui.TextColored(CFG.Current.ImGui_Invalid_Text_Color, "No object by that name");
            }
            else
            {
                var alias = AliasUtils.GetEntityAliasName(entity);
                if (alias is null or "")
                {
                    ImGui.TextColored(CFG.Current.ImGui_ParamRef_Text, $"{entity.PrettyName}");
                }
                else
                {
                    ImGui.TextColored(CFG.Current.ImGui_ParamRef_Text, $"{entity.PrettyName} - {alias}");
                }
            }

            if (ImGui.BeginPopupContextItem($"{msbRef.ReferenceType.Name}RefContextMenu"))
            {
                var changed = PropertyRowMsbRefContextItems(msbRef, val, ref newval, map);
                ImGui.EndPopup();
                return changed;
            }

            return false;
        }

        static string autocomplete = "";
        public static bool PropertyRowMsbRefContextItems(
            MSBReference reference,
            object oldval,
            ref object newval,
            ObjectContainer container
        )
        {
            if (oldval is not string name) return false;

            var entity = container.GetObjectByName(name);

            if (entity != null)
            {
                if (ImGui.Selectable($@"Select {name}"))
                {
                    EditorCommandQueue.AddCommand($@"map/select/{container.Name}/{name}");
                }
            }
            ImGui.InputTextWithHint("##value", "Search...", ref autocomplete, 128);
            if (autocomplete != "")
            {
                var shownList = container.Objects
                    .Where((obj) => reference.ReferenceType.IsInstanceOfType(obj.WrappedObject))
                    .Where((obj) => obj.Name.ToLower().Contains(autocomplete.ToLower()))
                    .Take(15);
                foreach (var shown in shownList)
                {
                    if (ImGui.Selectable($@"{shown.PrettyName}"))
                    {
                        newval = shown.Name;
                        return true;
                    }
                }
            }

            return false;
        }



        public static bool AliasEnumRow(PropertyInfo propinfo, object val, ref object newVal)
        {
            if (propinfo.GetCustomAttribute<MSBAliasEnum>() == null)
            {
                return false;
            }

            List<MSBAliasEnum> attributes = propinfo.GetCustomAttributes<MSBAliasEnum>().ToList();
            if (attributes.Any())
            {
                var enumName = attributes[0].AliasEnumType;
                List<AliasReference> options = null;

                // Particles
                if (enumName == "PARTICLES")
                {
                    if(Smithbox.BankHandler.ParticleAliases.Aliases != null)
                    {
                        options = Smithbox.BankHandler.ParticleAliases.Aliases.list;
                    }
                }
                // Flags
                if (enumName == "FLAGS")
                {
                    if (Smithbox.BankHandler.EventFlagAliases.Aliases != null)
                    {
                        options = Smithbox.BankHandler.EventFlagAliases.Aliases.list;
                    }
                }
                // Sounds
                if (enumName == "SOUNDS")
                {
                    if (Smithbox.BankHandler.SoundAliases.Aliases != null)
                    {
                        options = Smithbox.BankHandler.SoundAliases.Aliases.list;
                    }
                }
                // Cutscenes
                if (enumName == "CUTSCENES")
                {
                    if (Smithbox.BankHandler.CutsceneAliases.Aliases != null)
                    {
                        options = Smithbox.BankHandler.CutsceneAliases.Aliases.list;
                    }
                }
                // Movies
                if (enumName == "MOVIES")
                {
                    if (Smithbox.BankHandler.MovieAliases.Aliases != null)
                    {
                        options = Smithbox.BankHandler.MovieAliases.Aliases.list;
                    }
                }

                if (options == null)
                {
                    return false;
                }

                ImGui.NextColumn();

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
                ImGui.TextUnformatted(@$"   <{enumName}>");
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();

                ImGui.NextColumn();

                string currentEntry = "___";

                var match = options.Where(x => x.id == val.ToString()).FirstOrDefault();

                ImGui.BeginGroup();

                if (match != null)
                {
                    currentEntry = match.name;

                    // Revert if the stored name is empty
                    if(currentEntry == "")
                        currentEntry = "___";

                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_ParamRef_Text);
                    ImGui.TextUnformatted(currentEntry);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_ParamRefMissing_Text);
                    ImGui.TextUnformatted(currentEntry);
                    ImGui.PopStyleColor();
                }

                ImGui.EndGroup();

                if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
                {
                    var opened = MsbAliasEnumContextMenu(propinfo, val, ref newVal, options);
                    ImGui.EndPopup();
                    return opened;
                }
            }

            return false;
        }

        public static bool MsbAliasEnumContextMenu(PropertyInfo propinfo, object val, ref object newVal, List<AliasReference> options)
        {
            ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

            if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
            {
                try
                {
                    foreach (var entry in options)
                    {
                        if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.id, " ")
                            || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.name, " ")
                            || enumSearchStr == "")
                        {
                            if (ImGui.Selectable($"{entry.id}: {entry.name}"))
                            {
                                newVal = Convert.ChangeType(entry.id, val.GetType());
                                ImGui.EndChild();
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            ImGui.EndChild();
            return false;
        }

        // Elden Ring Asset Mask and Anim property
        public static bool EldenRingAssetMaskAndAnimRow(PropertyInfo propinfo, object oldValue, ref object newValue, ViewportSelection selection)
        {
            if(Smithbox.BankHandler.MSB_Info.Masks.list == null)
            {
                return false;
            }

            bool changedValue = false;

            Entity ent = selection.GetFilteredSelection<Entity>().First();

            if (ent.WrappedObject is MSBE.Part.Asset assetEnt)
            {
                FormatMaskEntry targetEntry = null;

                // Get the entry for the current model
                foreach(var entry in Smithbox.BankHandler.MSB_Info.Masks.list)
                {
                    if(assetEnt.ModelName == entry.model)
                    {
                        targetEntry = entry;
                    }
                }

                if (targetEntry != null)
                {
                    var mask = oldValue.ToString();

                    string sectionOne = "";
                    string sectionTwo = "";
                    string sectionThree = "";

                    bool hasSectionOne = false;
                    bool hasSectionTwo = false;
                    bool hasSectionThree = false;

                    if (mask.Length == 1)
                    {
                        mask = $"0{mask}";

                        hasSectionThree = true;

                        sectionThree = mask.Substring(0, 2);
                    }
                    else if (mask.Length == 2)
                    {
                        hasSectionThree = true;

                        sectionThree = mask.Substring(0, 2);
                    }
                    else if (mask.Length == 4)
                    {
                        hasSectionTwo = true;
                        hasSectionThree = true;

                        sectionTwo = mask.Substring(0, 2);
                        sectionThree = mask.Substring(2, 2);
                    }
                    else if (mask.Length == 6)
                    {
                        hasSectionOne = true;
                        hasSectionTwo = true;
                        hasSectionThree = true;

                        sectionOne = mask.Substring(0, 2);
                        sectionTwo = mask.Substring(2, 2);
                        sectionThree = mask.Substring(4, 2);
                    }

                    ImGui.NextColumn();

                    // Text
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_EnumName_Text);

                    ImGui.TextUnformatted($"<Form>");
                    ImGui.TextUnformatted($"<Equipment>");
                    ImGui.TextUnformatted($"<Position>");

                    ImGui.PopStyleColor();
                    ImGui.PopStyleVar();

                    ImGui.NextColumn();

                    // Section 1
                    var curSectionOne = targetEntry.section_one.Find(x => x.mask == sectionOne);

                    var sectionOneName = "None";
                    if (curSectionOne != null)
                    {
                        sectionOneName = curSectionOne.name;
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_EnumName_Text);
                    ImGui.TextUnformatted($"{sectionOneName}");
                    ImGui.PopStyleColor();

                    if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu"))
                    {
                        if (ImGui.Selectable($"None"))
                        {
                            var newMask = $"{sectionTwo}{sectionThree}";

                            if(!hasSectionTwo)
                            {
                                newMask = $"00{sectionThree}";
                            }

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }

                        foreach (var entry in targetEntry.section_one)
                        {
                            if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                            {
                                var newMask = $"{entry.mask}{sectionTwo}{sectionThree}";

                                if (!hasSectionTwo)
                                {
                                    newMask = $"00{sectionThree}";
                                }

                                newValue = Convert.ChangeType(newMask, oldValue.GetType());
                                changedValue = true;
                            }
                        }

                        ImGui.EndPopup();
                    }

                    // Section 2
                    var curSectionTwo = targetEntry.section_two.Find(x => x.mask == sectionTwo);

                    var sectionTwoName = "None";
                    if (curSectionTwo != null)
                    {
                        sectionTwoName = curSectionTwo.name;
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_EnumName_Text);
                    ImGui.TextUnformatted($"{sectionTwoName}");
                    ImGui.PopStyleColor();

                    if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu2"))
                    {
                        if (ImGui.Selectable($"None"))
                        {
                            var newMask = $"00{sectionThree}";

                            if (hasSectionOne)
                            {
                                newMask = $"{sectionOne}00{sectionThree}";
                            }

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }

                        foreach (var entry in targetEntry.section_two)
                        {
                            if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                            {
                                var newMask = $"{entry.mask}{sectionThree}";

                                if (hasSectionOne)
                                {
                                    newMask = $"{sectionOne}{entry.mask}{sectionThree}";
                                }

                                newValue = Convert.ChangeType(newMask, oldValue.GetType());
                                changedValue = true;
                            }
                        }

                        ImGui.EndPopup();
                    }

                    var curSectionThree = targetEntry.section_three.Find(x => x.mask == sectionThree);

                    var sectionThreeName = "None";
                    if (curSectionThree != null)
                    {
                        sectionThreeName = curSectionThree.name;
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_EnumName_Text);
                    ImGui.TextUnformatted($"{sectionThreeName}");
                    ImGui.PopStyleColor();

                    if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu3"))
                    {
                        if (ImGui.Selectable($"None"))
                        {
                            var newMask = $"-1";

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }

                        foreach (var entry in targetEntry.section_three)
                        {
                            if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                            {
                                var newMask = $"{entry.mask}";

                                if (hasSectionOne)
                                {
                                    newMask = $"{sectionTwo}{entry.mask}";
                                }
                                if (hasSectionTwo)
                                {
                                    newMask = $"{sectionOne}{sectionTwo}{entry.mask}";
                                }

                                newValue = Convert.ChangeType(newMask, oldValue.GetType());
                                changedValue = true;
                            }
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            return changedValue;
        }
    }
}

