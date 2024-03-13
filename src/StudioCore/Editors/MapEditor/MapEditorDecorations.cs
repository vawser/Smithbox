using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Editors.ParamEditor;
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
        // Elden Ring Asset Mask and Anim property
        public static bool EldenRingAssetMaskAndAnimRow(PropertyInfo propinfo, object oldValue, ref object newValue, ViewportSelection selection)
        {
            if(MsbFormatBank.Bank.Masks.list == null)
            {
                return false;
            }

            bool changedValue = false;

            Entity ent = selection.GetFilteredSelection<Entity>().First();

            if (ent.WrappedObject is MSBE.Part.Asset assetEnt)
            {
                FormatMaskEntry targetEntry = null;

                // Get the entry for the current model
                foreach(var entry in MsbFormatBank.Bank.Masks.list)
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

