using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using StudioCore.Core;
using Google.Protobuf.WellKnownTypes;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Editors.TextEditor.Utils;
using Silk.NET.OpenGL;
using SoapstoneLib.Proto.Internal;
using StudioCore.Tasks;

namespace StudioCore.Editor;

public class EditorDecorations
{
    private static string _refContextCurrentAutoComplete = "";

    public static bool HelpIcon(string id, ref string hint, bool canEdit)
    {
        if (hint == null)
        {
            return false;
        }

        return UIHints.AddImGuiHintButton(id, ref hint, canEdit, true); //presently a hack, move code here
    }

    public static void ParamFieldOffsetText(string activeParam, Param.Row context, string index)
    {
        // This feature is purely for AC6 MenuPropertySpecParam.
        if (activeParam == "MenuPropertySpecParam")
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.TextUnformatted(@"   <PARAM>");
            ImGui.TextUnformatted(@"   <FIELD>");
            if (CFG.Current.Param_MakeMetaNamesPrimary)
            {
                ImGui.TextUnformatted(@"   <NAME>");
            }
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }

    public static void ParamFieldOffsetValueText(string activeParam, Param.Row context, string index)
    {
        // This feature is purely for AC6 MenuPropertySpecParam.
        if (activeParam == "MenuPropertySpecParam")
        {
            string target = "";
            string primitiveType = "";
            string operationType = "";
            string fieldOffset = "";

            if (index == "0")
            {
                target = context["extract0_Target"].Value.Value.ToString();
                primitiveType = context["extract0_MemberType"].Value.Value.ToString();
                operationType = context["extract0_Operation"].Value.Value.ToString();
                fieldOffset = context["extract0_MemberTailOffset"].Value.Value.ToString();
            }
            else if (index == "1")
            {
                target = context["extract1_Target"].Value.Value.ToString();
                primitiveType = context["extract1_MemberType"].Value.Value.ToString();
                operationType = context["extract1_Operation"].Value.Value.ToString();
                fieldOffset = context["extract1_MemberTailOffset"].Value.Value.ToString();
            }
            else
            {
                return;
            }

            var decimalOffset = int.Parse($"{fieldOffset}");

            switch (primitiveType)
            {
                case "0": return;

                case "1": // s8
                case "2": // u8
                    decimalOffset = decimalOffset - 1;
                    break;

                case "3": // s16
                case "4": // u16
                    decimalOffset = decimalOffset - 2;
                    break;


                case "5": // s32
                case "6": // u32
                case "7": // f
                    decimalOffset = decimalOffset - 4;
                    break;
            }
            
            var paramString = "";

            switch(target)
            {
                case "0": return;

                case "1": // Weapon
                    paramString = "EquipParamWeapon";
                    break;
                case "2": // Armor
                    paramString = "EquipParamProtector";
                    break;
                case "3": // Booster
                    paramString = "EquipParamBooster";
                    break;
                case "4": // FCS
                    paramString = "EquipParamFcs";
                    break;
                case "5": // Generator
                    paramString = "EquipParamGenerator";
                    break;
                case "6": // Behavior Paramter
                    paramString = "BehaviorParam_PC";
                    break;
                case "7": // Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "8": // Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "100": // Child Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "101": // Child Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "110": // Parent Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "111": // Parent Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
            }

            var firstRow = ParamBank.PrimaryBank.Params[paramString].Rows.First();
            var internalName = "";
            var displayName = "";

            foreach (var col in firstRow.Columns)
            {
                var offset = (int)col.GetByteOffset();

                if (offset == decimalOffset)
                {
                    internalName = col.Def.InternalName;

                    var cellmeta = FieldMetaData.Get(col.Def);
                    displayName = cellmeta.AltName;
                }
            }

            ImGui.BeginGroup();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);

            ImGui.TextUnformatted($"{paramString}:");
            ImGui.TextUnformatted($"{internalName}");
            if (CFG.Current.Param_MakeMetaNamesPrimary)
            {
                ImGui.TextUnformatted($"{displayName}");
            }

            ImGui.PopStyleColor();

            ImGui.EndGroup();
        }
    }

    public static void ParamRefText(List<ParamRef> paramRefs, Param.Row context)
    {
        if (paramRefs == null || paramRefs.Count == 0)
        {
            return;
        }

        if(!CFG.Current.Param_ShowFieldParamLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.TextUnformatted(@"   <");
        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in paramRefs)
        {
            Param.Cell? c = context?[r.ConditionField];
            var inactiveRef = context != null && c != null && Convert.ToUInt32(c.Value.Value) != r.ConditionValue;
            if (inactiveRef)
            {
                inactiveRefs.Add(r.ParamName);
            }
            else
            {
                if (first)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted(r.ParamName);
                }
                else
                {
                    ImGui.TextUnformatted("    " + r.ParamName);
                }

                first = false;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefInactive_Text);
        foreach (var inactive in inactiveRefs)
        {
            ImGui.SameLine();
            if (first)
            {
                ImGui.TextUnformatted("!" + inactive);
            }
            else
            {
                ImGui.TextUnformatted("!" + inactive);
            }

            first = false;
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.TextUnformatted(">");
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    public static void FmgRefText(List<FMGRef> fmgRef, Param.Row context, string overrideName = "")
    {
        if (fmgRef == null)
        {
            return;
        }

        if (!CFG.Current.Param_ShowFieldFmgLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

        if (overrideName == "")
        {
            ImGui.TextUnformatted(@"   [");

            List<string> inactiveRefs = new();
            var first = true;
            foreach (FMGRef r in fmgRef)
            {
                Param.Cell? c = context?[r.conditionField];
                var inactiveRef = context != null && c != null && Convert.ToInt32(c.Value.Value) != r.conditionValue;

                if (inactiveRef)
                {
                    inactiveRefs.Add(r.fmg);
                }
                else
                {
                    if (first)
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted(r.fmg);
                    }
                    else
                    {
                        ImGui.TextUnformatted("    " + r.fmg);
                    }

                    first = false;
                }
            }

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRefInactive_Text);
            foreach (var inactive in inactiveRefs)
            {
                ImGui.SameLine();
                if (first)
                {
                    ImGui.TextUnformatted("!" + inactive);
                }
                else
                {
                    ImGui.TextUnformatted("!" + inactive);
                }

                first = false;
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.TextUnformatted("]");
        }
        else
        {
            ImGui.TextUnformatted($@"   [{overrideName}]");
        }

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }


    public static void TextureRefText(List<TexRef> textureRef, Param.Row context)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (textureRef == null)
        {
            return;
        }

        if (!CFG.Current.Param_ShowFieldTextureLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

        ImGui.TextUnformatted(@"   [Image]");

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    public static void ParamRefsSelectables(ParamBank bank, List<ParamRef> paramRefs, Param.Row context,
        dynamic oldval)
    {
        if (paramRefs == null)
        {
            return;
        }

        // Add named row and context menu
        // Lists located params
        // May span lines
        List<(string, Param.Row, string)> matches = resolveRefs(bank, paramRefs, context, oldval);
        var entryFound = matches.Count > 0;
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
        ImGui.BeginGroup();
        foreach ((var param, Param.Row row, var adjName) in matches)
        {
            ImGui.TextUnformatted(adjName);
        }

        ImGui.PopStyleColor();
        if (!entryFound)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
            ImGui.TextUnformatted("___");
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();
    }

    private static List<(string, Param.Row, string)> resolveRefs(ParamBank bank, List<ParamRef> paramRefs,
        Param.Row context, dynamic oldval)
    {
        List<(string, Param.Row, string)> rows = new();
        if (bank.Params == null)
        {
            return rows;
        }

        var originalValue =
            (int)oldval; //make sure to explicitly cast from dynamic or C# complains. Object or Convert.ToInt32 fail.
        foreach (ParamRef rf in paramRefs)
        {
            Param.Cell? c = context?[rf.ConditionField];
            var inactiveRef = context != null && c != null && Convert.ToUInt32(c.Value.Value) != rf.ConditionValue;
            if (inactiveRef)
            {
                continue;
            }

            var rt = rf.ParamName;
            var hint = "";
            if (bank.Params.ContainsKey(rt))
            {
                var altval = originalValue;
                if (rf.Offset != 0)
                {
                    altval += rf.Offset;
                    hint += rf.Offset > 0 ? "+" + rf.Offset : rf.Offset.ToString();
                }

                Param param = bank.Params[rt];
                ParamMetaData meta = ParamMetaData.Get(bank.Params[rt].AppliedParamdef);
                if (meta != null && meta.Row0Dummy && altval == 0)
                {
                    continue;
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

                    r = bank.Params[rt][altval];
                }

                if (r == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.Name))
                {
                    rows.Add((rf.ParamName, r, "Unnamed Row" + hint));
                }
                else
                {
                    rows.Add((rf.ParamName, r, r.Name + hint));
                }
            }
        }

        return rows;
    }

    private static List<TextResult> resolveFMGRefs(List<FMGRef> fmgRefs, Param.Row context,
        dynamic oldval)
    {
        if (!TextBank.PrimaryBankLoaded)
        {
            return new List<TextResult>();
        }

        List<TextResult> newTextResults = new();

        foreach(var entry in fmgRefs)
        {
            Param.Cell? c = context?[entry.conditionField];

            bool cont = true;

            if (context != null && c != null)
            {
                if(Convert.ToInt32(c.Value.Value) != entry.conditionValue)
                {
                    cont = false;
                }
            }

            if(cont)
            {
                uint tempVal = (uint)oldval;

                TextResult result = TextFinder.GetTextResult(entry.fmg, (int)tempVal, entry.offset);

                if (result != null)
                {
                    newTextResults.Add(result);
                }
            }
        }

        return newTextResults;
    }

    public static void FmgRefSelectable(EditorScreen ownerScreen, List<FMGRef> fmgNames, Param.Row context,
        dynamic oldval)
    {
        List<string> textsToPrint = new List<string>();

        textsToPrint = UICache.GetCached(ownerScreen, (int)oldval, "PARAM META FMGREF", () =>
        {
            List<TextResult> refs = resolveFMGRefs(fmgNames, context, oldval);
            return refs.Where(x => x.Entry != null)
                .Select(x =>
                {
                    return $"{x.Entry.Text}".TrimStart();
                }).ToList();
        });

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);
        foreach (var text in textsToPrint)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ImGui.TextUnformatted("%null%");
            }
            else
            {
                ImGui.TextUnformatted(text);
            }
        }

        ImGui.PopStyleColor();
    }

    public static void TextureRefSelectable(EditorScreen ownerScreen, List<TexRef> texRefs, Param.Row context,
        dynamic oldval)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (Smithbox.EditorHandler.TextureViewer.ImagePreview == null)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);

        ImGui.TextUnformatted("View Source Image");

        foreach (var texRef in texRefs)
        {
            if (CFG.Current.EnableViewer_TEXTURE && CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn)
            {
                var imageDisplayed = Smithbox.EditorHandler.TextureViewer.ImagePreview.DisplayImagePreview(context, texRef);

                // If an image has been displayed, exit the loop so we don't show multiple images
                if (imageDisplayed)
                    break;
            }
        }

        ImGui.PopStyleColor();
    }

    public static void EnumNameText(ParamEnum pEnum)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        if (pEnum != null && pEnum.Name != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {pEnum.Name}");
            ImGui.PopStyleColor();
        }
    }

    public static void EnumValueText(Dictionary<string, string> enumValues, string value)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
        ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
        ImGui.PopStyleColor();
    }

    public static void AliasEnumNameText(string name)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        var inactiveEnum = false;

        if(!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {name}");
            ImGui.PopStyleColor();
        }
    }

    public static void ConditionalAliasEnumNameText(string name, Param.Row row, string limitField, string limitValue)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        var inactiveEnum = false;

        if (limitField != "")
        {
            Param.Cell? c = row?[limitField];
            inactiveEnum = row != null && c != null && Convert.ToInt32(c.Value.Value) != Convert.ToInt32(limitValue);
        }

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {name}");
            ImGui.PopStyleColor();
        }
    }

    public static void AliasEnumValueText(Dictionary<string, string> enumValues, string value)
    {
        var inactiveEnum = false;

        if (!CFG.Current.Param_HideEnums) //Move preference
        {
            if (!inactiveEnum)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
                if (value == "0" || value == "-1")
                {
                    ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "None"));
                }
                else
                {
                    ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
                }
                ImGui.PopStyleColor();
            }
        }
    }
    public static void ConditionalAliasEnumValueText(Dictionary<string, string> enumValues, string value, Param.Row row, string conditionalField, string conditionalValue)
    {
        var inactiveEnum = false;

        if (conditionalField != "")
        {
            Param.Cell? c = row?[conditionalField];
            inactiveEnum = row != null && c != null && Convert.ToInt32(c.Value.Value) != Convert.ToInt32(conditionalValue);
        }

        if (CFG.Current.Param_HideEnums == false) //Move preference
        {
            if (!inactiveEnum)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
                if(value == "0" || value == "-1")
                {
                    ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "None"));
                }
                else
                {
                    ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
                }
                ImGui.PopStyleColor();
            }
        }
    }

    public static bool ProjectEnumsLoaded()
    {
        var bank = Smithbox.BankHandler.ProjectEnums;

        if(bank == null)
            return false;

        if(bank.Enums == null)
            return false;

        if (bank.Enums.List == null)
            return false;

        if(bank.Enums.List.Count == 0)
            return false;

        return true;
    }

    public static void ProjectEnumNameText(string enumType)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        var bank = Smithbox.BankHandler.ProjectEnums;
        if (ProjectEnumsLoaded())
        {
            var enumEntry = bank.Enums.List.Where(e => e.Name == enumType).FirstOrDefault();

            if (enumEntry != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
                ImGui.TextUnformatted($@"   {enumEntry.DisplayName}");
                ImGui.PopStyleColor();

                if (enumEntry.Description != "")
                {
                    UIHelper.ShowHoverTooltip($"{enumEntry.Description}");
                }
            }
        }
    }

    public static void ProjectEnumValueText(string enumType, string value)
    {
        if (CFG.Current.Param_HideEnums == false) //Move preference
        {
            var bank = Smithbox.BankHandler.ProjectEnums;

            if (ProjectEnumsLoaded())
            {
                var enumEntry = bank.Enums.List.Where(e => e.Name == enumType).FirstOrDefault();

                if(enumEntry != null)
                {
                    var enumValueName = "";
                    var enumValue = enumEntry.Options.Where(e => e.ID == value).FirstOrDefault();

                    if (enumValue != null)
                    {
                        enumValueName = enumValue.Name;
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
                    ImGui.TextUnformatted(enumValueName);
                    ImGui.PopStyleColor();
                }
            }
        }
    }

    public static void VirtualParamRefSelectables(ParamBank bank, string virtualRefName, object searchValue,
        Param.Row context, string fieldName, List<ExtRef> ExtRefs, EditorScreen cacheOwner)
    {
        // Add Goto statements
        if (bank.Params != null)
        {
            foreach (KeyValuePair<string, Param> param in bank.Params)
            {
                foreach (PARAMDEF.Field f in param.Value.AppliedParamdef.Fields)
                {
                    if (FieldMetaData.Get(f).VirtualRef != null &&
                        FieldMetaData.Get(f).VirtualRef.Equals(virtualRefName))
                    {
                        if (ImGui.Selectable($@"Search in {param.Key} ({f.InternalName})"))
                        {
                            EditorCommandQueue.AddCommand($@"param/select/-1/{param.Key}");
                            EditorCommandQueue.AddCommand(
                                $@"param/search/prop {f.InternalName} ^{searchValue.ToParamEditorString()}$");
                        }
                    }
                }
            }
        }

        if (ExtRefs != null)
        {
            foreach (ExtRef currentRef in ExtRefs)
            {
                List<string> matchedExtRefPath =
                    currentRef.paths.Select(x => string.Format(x, searchValue)).ToList();
                ExtRefItem(context, fieldName, $"modded {currentRef.name}", matchedExtRefPath, Smithbox.ProjectRoot,
                    cacheOwner);
                ExtRefItem(context, fieldName, $"vanilla {currentRef.name}", matchedExtRefPath,
                    Smithbox.GameRoot, cacheOwner);
            }
        }
    }

    private static void ExtRefItem(Param.Row keyRow, string fieldKey, string menuText,
        List<string> matchedExtRefPath, string dir, EditorScreen cacheOwner)
    {
        var exist = UICache.GetCached(cacheOwner, keyRow, $"extRef{menuText}{fieldKey}",
            () => Path.Exists(Path.Join(dir, matchedExtRefPath[0])));
        if (exist && ImGui.Selectable($"Go to {menuText} file..."))
        {
            var path = ResolveExtRefPath(matchedExtRefPath, dir);
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                TaskLogs.AddLog($"\"{path}\" could not be found. It may be map or chr specific",
                    LogLevel.Warning);
                UICache.ClearCaches();
            }
        }
    }

    private static string ResolveExtRefPath(List<string> matchedExtRefPath, string baseDir)
    {
        var currentPath = baseDir;
        foreach (var nextStage in matchedExtRefPath)
        {
            var thisPathF = Path.Join(currentPath, nextStage);
            var thisPathD = Path.Join(currentPath, nextStage.Replace('.', '-'));
            if (Directory.Exists(thisPathD))
            {
                currentPath = thisPathD;
                continue;
            }

            if (File.Exists(thisPathF))
            {
                currentPath = thisPathF;
            }

            break;
        }

        if (currentPath == baseDir)
        {
            return null;
        }

        return currentPath;
    }

    public static void ParamRefEnumQuickLink(ParamBank bank, object oldval, List<ParamRef> RefTypes,
        Param.Row context, List<FMGRef> fmgRefs, ParamEnum Enum, List<TexRef> textureRefs)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) &&
            (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight)))
        {
            if (RefTypes != null)
            {
                (string, Param.Row, string)? primaryRef =
                    resolveRefs(bank, RefTypes, context, oldval)?.FirstOrDefault();
                if (primaryRef?.Item2 != null)
                {
                    if (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight))
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/new/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                    else
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/-1/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                }
            }
            else if (fmgRefs != null)
            {
                TextResult? primaryRef = resolveFMGRefs(fmgRefs, context, oldval)?.FirstOrDefault();
                if (primaryRef != null)
                {
                    EditorCommandQueue.AddCommand($@"text/select/{primaryRef.ContainerWrapper.Filename}/{primaryRef.FmgName}/{primaryRef.Entry.ID}");
                }
            }
            else if (textureRefs != null)
            {
                TexRef primaryRef = textureRefs.FirstOrDefault();
                if (primaryRef?.TextureContainer != null && primaryRef?.TextureFile != null)
                {
                    EditorCommandQueue.AddCommand($@"texture/view/{primaryRef?.TextureContainer}/{primaryRef?.TextureFile}");
                }
            }
        }
    }

    public static bool ParamRefEnumShortcutItems(ParamBank bank, FieldMetaData cellMeta, object oldval, ref object newval, List<ParamRef> RefTypes, Param.Row context, List<FMGRef> fmgRefs, List<FMGRef> mapFmgRefs, List<TexRef> textureRefs, ParamEnum Enum, ActionManager executor)
    {
        var result = false;

        if (!ImGui.IsAnyItemActive())
        {
            if (RefTypes != null)
            {
                if (bank.Params == null)
                {
                    return false;
                }

                if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_InheritReferencedRowName))
                {
                    List<(string, Param.Row, string)> refs = resolveRefs(bank, RefTypes, context, oldval);

                    foreach ((string, Param.Row, string) rf in refs)
                    {
                        if (context == null || executor == null)
                        {
                            continue;
                        }

                        executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                                rf.Item2.Name));
                    }

                    result = true;
                }
            }
        }

        return result;
    }

    public static bool ParamRefEnumContextMenuItems(ParamBank bank, FieldMetaData cellMeta, object oldval, ref object newval, List<ParamRef> RefTypes, Param.Row context, List<FMGRef> fmgRefs, List<FMGRef> mapFmgRefs, List<TexRef> textureRefs, ParamEnum Enum, ActionManager executor)
    {
        var result = false;
        if (RefTypes != null)
        {
            result |= PropertyRowRefsContextItems(bank, RefTypes, context, oldval, ref newval, executor);
        }

        if (fmgRefs != null)
        {
            PropertyRowFMGRefsContextItems(fmgRefs, context, oldval, executor);
        }

        if (mapFmgRefs != null)
        {
            PropertyRowFMGRefsContextItems(mapFmgRefs, context, oldval, executor);
        }

        if (textureRefs != null)
        {
            PropertyRowTextureRefsContextItems(textureRefs, context, executor);
        }

        if (Enum != null)
        {
            result |= PropertyRowEnumContextItems(Enum, oldval, ref newval);
        }

        if(cellMeta != null)
        {
            if (cellMeta.ShowParticleEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.ParticleAliases.GetEnumDictionary(), oldval, ref newval);
            }

            if (cellMeta.ShowSoundEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.SoundAliases.GetEnumDictionary(), oldval, ref newval);
            }

            if (cellMeta.ShowFlagEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.EventFlagAliases.GetEnumDictionary(), oldval, ref newval);
            }

            if (cellMeta.ShowCutsceneEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.CutsceneAliases.GetEnumDictionary(), oldval, ref newval);
            }

            if (cellMeta.ShowMovieEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.MovieAliases.GetEnumDictionary(), oldval, ref newval);
            }

            if (cellMeta.ShowProjectEnumList)
            {
                result |= PropertyRowAliasEnumContextItems(Smithbox.BankHandler.ProjectEnums.GetEnumDictionary(cellMeta.ProjectEnumType), oldval, ref newval);
            }
        }

        return result;
    }

    public static bool PropertyRowRefsContextItems(ParamBank bank, List<ParamRef> reftypes, Param.Row context,
        object oldval, ref object newval, ActionManager executor)
    {
        if (bank.Params == null)
        {
            return false;
        }

        // Add Goto statements
        List<(string, Param.Row, string)> refs = resolveRefs(bank, reftypes, context, oldval);
        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);
        foreach ((string, Param.Row, string) rf in refs)
        {
            if (ImGui.Selectable($@"Go to {rf.Item3}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/-1/{rf.Item1}/{rf.Item2.ID}");
            }

            if (ImGui.Selectable($@"Go to {rf.Item3} in new view"))
            {
                EditorCommandQueue.AddCommand($@"param/select/new/{rf.Item1}/{rf.Item2.ID}");
            }

            if (context == null || executor == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(rf.Item2.Name) &&
                (ctrlDown || string.IsNullOrWhiteSpace(context.Name)) &&
                ImGui.Selectable($@"Inherit referenced row's name ({rf.Item2.Name})"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                    rf.Item2.Name));
            }
            else if ((ctrlDown || string.IsNullOrWhiteSpace(rf.Item2.Name)) &&
                     !string.IsNullOrWhiteSpace(context.Name) &&
                     ImGui.Selectable($@"Proliferate name to referenced row ({rf.Item1})"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(rf.Item2.GetType().GetProperty("Name"), rf.Item2,
                    context.Name));
            }
        }

        // Add searchbar for named editing
        ImGui.InputTextWithHint("##value", "Search...", ref _refContextCurrentAutoComplete, 128);
        // This should be replaced by a proper search box with a scroll and everything
        if (_refContextCurrentAutoComplete != "")
        {
            foreach (ParamRef rf in reftypes)
            {
                var rt = rf.ParamName;
                if (!bank.Params.ContainsKey(rt))
                {
                    continue;
                }

                ParamMetaData meta = ParamMetaData.Get(bank.Params[rt].AppliedParamdef);
                var maxResultsPerRefType = 15 / reftypes.Count;
                List<Param.Row> rows = RowSearchEngine.rse.Search((bank, bank.Params[rt]),
                    _refContextCurrentAutoComplete, true, true);
                foreach (Param.Row r in rows)
                {
                    if (maxResultsPerRefType <= 0)
                    {
                        break;
                    }

                    if (ImGui.Selectable($@"({rt}){r.ID}: {r.Name}"))
                    {
                        try
                        {
                            if (meta != null && meta.FixedOffset != 0)
                            {
                                newval = Convert.ChangeType(r.ID - meta.FixedOffset - rf.Offset, oldval.GetType());
                            }
                            else
                            {
                                newval = Convert.ChangeType(r.ID - rf.Offset, oldval.GetType());
                            }

                            _refContextCurrentAutoComplete = "";
                            return true;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog("Unable to convert value into param field's type'", LogLevel.Warning,
                                LogPriority.Normal, e);
                        }
                    }

                    maxResultsPerRefType--;
                }
            }
        }

        return false;
    }

    public static void PropertyRowFMGRefsContextItems(List<FMGRef> reftypes, Param.Row context, dynamic oldval,
        ActionManager executor)
    {
        // Add Goto statements
        List<TextResult> refs = resolveFMGRefs(reftypes, context, oldval);

        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);

        foreach (var result in refs)
        {
            if (result != null && result.Entry != null)
            {
                if (ImGui.Selectable($@"Go to FMG entry text"))
                {
                    EditorCommandQueue.AddCommand($@"text/select/{result.ContainerWrapper.ContainerDisplayCategory}/{result.ContainerWrapper.Filename}/{result.FmgName}/{result.Entry.ID}");
                }

                if (context == null || executor == null)
                {
                    continue;
                }

                // Set Row Name to X
                if (!string.IsNullOrWhiteSpace(result.Entry.Text))
                {
                    if(ImGui.Selectable($@"Replace row name with referenced FMG entry text"))
                    {
                        executor.ExecuteAction(
                            new PropertiesChangedAction(
                                context.GetType().GetProperty("Name"),
                                context, 
                                result.Entry.Text));
                    }
                }

                // Apply Row Name to X
                if (!string.IsNullOrWhiteSpace(context.Name))
                {
                    if (ImGui.Selectable($@"Replace FMG entry text with current row name"))
                    {
                        executor.ExecuteAction(
                            new PropertiesChangedAction(
                                result.Entry.GetType().GetProperty("Text"), 
                                result.Entry,
                                context.Name));
                    }
                }
            }
        }
    }

    public static void PropertyRowTextureRefsContextItems(List<TexRef> reftypes, Param.Row context, ActionManager executor)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);

        foreach(var textureRef in reftypes)
        {
            bool displayedImage = Smithbox.EditorHandler.TextureViewer.ImagePreview.DisplayImagePreview(context, textureRef, false);

            if (displayedImage)
            {
                if (ImGui.Selectable($@"View {textureRef.TextureFile}"))
                {
                    EditorCommandQueue.AddCommand($@"texture/view/{textureRef.TextureContainer}/{textureRef.TextureFile}");
                }
            }

            if (context == null || executor == null)
            {
                continue;
            }
        }
    }

    public static string enumSearchStr = "";

    public static bool PropertyRowAliasEnumContextItems(Dictionary<string, string> en, object oldval, ref object newval)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, en.Count))))
        {
            try
            {
                foreach (KeyValuePair<string, string> option in en)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                        {
                            newval = Convert.ChangeType(option.Key, oldval.GetType());
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

    public static bool PropertyRowEnumContextItems(ParamEnum en, object oldval, ref object newval)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, en.Values.Count))))
        {
            try
            {
                foreach (KeyValuePair<string, string> option in en.Values)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                        {
                            newval = Convert.ChangeType(option.Key, oldval.GetType());
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

    public static void ParamRefReverseLookupSelectables(EditorScreen screen, ParamBank bank, string currentParam,
        int currentID)
    {
        if (ImGui.BeginMenu("Search for references"))
        {
            Dictionary<string, List<(string, ParamRef)>> items = UICache.GetCached(screen, (bank, currentParam),
                () => ParamRefReverseLookupFieldItems(bank, currentParam));
            foreach (KeyValuePair<string, List<(string, ParamRef)>> paramitems in items)
            {
                if (ImGui.BeginMenu($@"in {paramitems.Key}..."))
                {
                    foreach ((var fieldName, ParamRef pref) in paramitems.Value)
                    {
                        if (ImGui.BeginMenu($@"in {fieldName}"))
                        {
                            List<Param.Row> rows = UICache.GetCached(screen, (bank, currentParam, currentID, paramitems.Key, fieldName),
                                () => ParamRefReverseLookupRowItems(bank, paramitems.Key, fieldName, currentID,
                                    pref));
                            foreach (Param.Row row in rows)
                            {
                                var nameToPrint = string.IsNullOrEmpty(row.Name) ? "Unnamed Row" : row.Name;
                                if (ImGui.Selectable($@"{row.ID} {nameToPrint}"))
                                {
                                    EditorCommandQueue.AddCommand($@"param/select/-1/{paramitems.Key}/{row.ID}");
                                }
                            }

                            if (rows.Count == 0)
                            {
                                ImGui.TextUnformatted("No rows found");
                            }

                            ImGui.EndMenu();
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            if (items.Count == 0)
            {
                ImGui.TextUnformatted("This param is not referenced");
            }

            ImGui.EndMenu();
        }
    }

    public static void DrawCalcCorrectGraph(EditorScreen screen, ParamMetaData meta, Param.Row row)
    {
        try
        {
            bool draw = true;

            // Prevent draw for any rows that use inheritanceFcsParamId
            if (row["inheritanceFcsParamId"] != null)
            {
                if(row["inheritanceFcsParamId"].Value.Value.ToString() != "-1")
                {
                    draw = false;
                }
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Indent();
            CalcCorrectDefinition ccd = meta.CalcCorrectDef;
            SoulCostDefinition scd = meta.SoulCostDef;
            float[] values;
            int xOffset;
            float minY;
            float maxY;

            if (draw)
            {
                if (scd != null && scd.cost_row == row.ID)
                {
                    (values, maxY) = UICache.GetCached(screen, row, "soulCostData", () => ParamUtils.getSoulCostData(scd, row));

                    ImGui.PlotLines("##graph", ref values[0], values.Length, 0, "", 0, maxY, new Vector2(ImGui.GetColumnWidth(-1) - 30.0f, (ImGui.GetColumnWidth(-1) * 0.5625f) - 30.0f));
                }
                else if (ccd != null)
                {
                    (values, xOffset, minY, maxY) = UICache.GetCached(screen, row, "calcCorrectData",
                        () => ParamUtils.getCalcCorrectedData(ccd, row));
                    ImGui.PlotLines("##graph", ref values[0], values.Length, 0,
                        xOffset == 0 ? "" : $@"Note: add {xOffset} to x coordinate", minY, maxY,
                        new Vector2(ImGui.GetColumnWidth(-1) - 30f, (ImGui.GetColumnWidth(-1) * 0.5625f) - 30f));
                }
            }
        }
        catch (Exception e)
        {
            ImGui.TextUnformatted("Unable to draw graph");
        }

        ImGui.NewLine();
    }

    private static Dictionary<string, List<(string, ParamRef)>> ParamRefReverseLookupFieldItems(ParamBank bank,
        string currentParam)
    {
        Dictionary<string, List<(string, ParamRef)>> items = new();
        foreach (KeyValuePair<string, Param> param in bank.Params)
        {
            List<(string, ParamRef)> paramitems = new();
            //get field
            foreach (PARAMDEF.Field f in param.Value.AppliedParamdef.Fields)
            {
                FieldMetaData meta = FieldMetaData.Get(f);
                if (meta.RefTypes == null)
                {
                    continue;
                }

                // get hilariously deep in loops
                foreach (ParamRef pref in meta.RefTypes)
                {
                    if (!pref.ParamName.Equals(currentParam))
                    {
                        continue;
                    }

                    paramitems.Add((f.InternalName, pref));
                }
            }

            if (paramitems.Count > 0)
            {
                items[param.Key] = paramitems;
            }
        }

        return items;
    }

    private static List<Param.Row> ParamRefReverseLookupRowItems(ParamBank bank, string paramName, string fieldName,
        int currentID, ParamRef pref)
    {
        var searchTerm = pref.ConditionField != null
            ? $@"prop {fieldName} ^{currentID}$ && prop {pref.ConditionField} ^{pref.ConditionValue}$"
            : $@"prop {fieldName} ^{currentID}$";
        return RowSearchEngine.rse.Search((bank, bank.Params[paramName]), searchTerm, false, false);
    }

    public static bool ImguiTableSeparator()
    {
        var lastCol = false;
        var cols = ImGui.TableGetColumnCount();
        ImGui.TableNextRow();
        for (var i = 0; i < cols; i++)
        {
            if (ImGui.TableNextColumn())
            {
                ImGui.Separator();
                lastCol = true;
            }
        }

        return lastCol;
    }

    public static bool ImGuiTableStdColumns(string id, int cols, bool fixVerticalPadding)
    {
        Vector2 oldPad = ImGui.GetStyle().CellPadding;
        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = new Vector2(oldPad.X, 0);
        }

        var v = ImGui.BeginTable(id, cols,
            ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchSame |
            ImGuiTableFlags.ScrollY);
        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = oldPad;
        }

        return v;
    }

    public static void PinListReorderOptions<T>(List<T> sourceListToModify, T currentElement)
    {
        int indexOfPin = sourceListToModify.IndexOf(currentElement);
        if (indexOfPin > 0 && ImGui.Selectable("Move pin up"))
        {
            T prevKey = sourceListToModify[indexOfPin - 1];
            sourceListToModify[indexOfPin] = prevKey;
            sourceListToModify[indexOfPin - 1] = currentElement;
        }
        if (indexOfPin >= 0 && indexOfPin < sourceListToModify.Count - 1 && ImGui.Selectable("Move pin down"))
        {
            T nextKey = sourceListToModify[indexOfPin + 1];
            sourceListToModify[indexOfPin] = nextKey;
            sourceListToModify[indexOfPin + 1] = currentElement;
        }
    }

    /// <summary>
    ///     Displays information about the provided property.
    /// </summary>
    public static void ImGui_DisplayPropertyInfo(PropertyInfo prop)
    {
        ImGui_DisplayPropertyInfo(prop.PropertyType, prop.Name, true, true);
    }

    /// <summary>
    ///     Displays information about the provided property.
    /// </summary>
    public static void ImGui_DisplayPropertyInfo(System.Type propType, string fieldName, bool printName, bool printType, string altName = null, int arrayLength = -1, int bitSize = -1)
    {
        if (!string.IsNullOrWhiteSpace(altName))
        {
            fieldName += $"  /  {altName}";
        }

        if (CFG.Current.Param_FieldContextMenu_Name && printName)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(fieldName, "", true));
        }

        if (CFG.Current.Param_FieldContextMenu_Split && !printType)
        {
            return;
        }

        if (bitSize != -1)
        {
            var str = $"Bitfield Type within: {fieldName}";
            var min = 0;
            var max = (2ul << (bitSize - 1)) - 1;
            str += $" (Min {min}, Max {max})";
            ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
        }
        else
        {
            if (propType.IsArray)
            {
                var str = $"Array Type: {propType.Name}";
                if (arrayLength > 0)
                {
                    str += $" (Length: {arrayLength})";
                }

                propType = propType.GetElementType();

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }

            if (propType.IsValueType)
            {
                var str = $"Value Type: {propType.Name}";
                var min = propType.GetField("MinValue")?.GetValue(propType);
                var max = propType.GetField("MaxValue")?.GetValue(propType);
                if (min != null && max != null)
                {
                    str += $" (Min {min}, Max {max})";
                }

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }
            else if (propType == typeof(string))
            {
                var str = $"String Type: {propType.Name}";
                if (arrayLength > 0)
                {
                    str += $" (Length: {arrayLength})";
                }

                ImGui.TextColored(new Vector4(.4f, 1f, .7f, 1f), str);
            }
        }
    }
}
