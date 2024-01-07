using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;

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

    public static void ParamRefText(List<ParamRef> paramRefs, Param.Row context)
    {
        if (paramRefs == null || paramRefs.Count == 0)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
        ImGui.TextUnformatted(@"   <");
        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in paramRefs)
        {
            Param.Cell? c = context?[r.conditionField];
            var inactiveRef = context != null && c != null && Convert.ToInt32(c.Value.Value) != r.conditionValue;
            if (inactiveRef)
            {
                inactiveRefs.Add(r.param);
            }
            else
            {
                if (first)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted(r.param);
                }
                else
                {
                    ImGui.TextUnformatted("    " + r.param);
                }

                first = false;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
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

    public static void FmgRefText(List<FMGRef> fmgRef, Param.Row context)
    {
        if (fmgRef == null)
        {
            return;
        }

        if (CFG.Current.Param_HideReferenceRows == false) //Move preference
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
            ImGui.TextUnformatted(@"   [");
            List<string> inactiveRefs = new();
            var first = true;
            foreach (FMGRef r in fmgRef)
            {
                Param.Cell? c = context?[r.conditionField];
                var inactiveRef = context != null && c != null &&
                                  Convert.ToInt32(c.Value.Value) != r.conditionValue;
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

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
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
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
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
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.5f, 0.5f, 1.0f));
        ImGui.BeginGroup();
        foreach ((var param, Param.Row row, var adjName) in matches)
        {
            ImGui.TextUnformatted(adjName);
        }

        ImGui.PopStyleColor();
        if (!entryFound)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
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
            Param.Cell? c = context?[rf.conditionField];
            var inactiveRef = context != null && c != null && Convert.ToInt32(c.Value.Value) != rf.conditionValue;
            if (inactiveRef)
            {
                continue;
            }

            var rt = rf.param;
            var hint = "";
            if (bank.Params.ContainsKey(rt))
            {
                var altval = originalValue;
                if (rf.offset != 0)
                {
                    altval += rf.offset;
                    hint += rf.offset > 0 ? "+" + rf.offset : rf.offset.ToString();
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
                    rows.Add((rf.param, r, "Unnamed Row" + hint));
                }
                else
                {
                    rows.Add((rf.param, r, r.Name + hint));
                }
            }
        }

        return rows;
    }

    private static List<(string, FMGBank.EntryGroup)> resolveFMGRefs(List<FMGRef> fmgRefs, Param.Row context,
        dynamic oldval)
    {
        if (!FMGBank.IsLoaded)
        {
            return new List<(string, FMGBank.EntryGroup)>();
        }

        return fmgRefs.Where(rf =>
            {
                Param.Cell? c = context?[rf.conditionField];
                return context == null || c == null || Convert.ToInt32(c.Value.Value) == rf.conditionValue;
            }).Select(rf => FMGBank.FmgInfoBank.Find(x => x.Name == rf.fmg))
            .Where(fmgi => fmgi != null)
            .Select(fmgi => (fmgi.Name, FMGBank.GenerateEntryGroup((int)oldval, fmgi)))
            .ToList();
    }

    public static void FmgRefSelectable(EditorScreen ownerScreen, List<FMGRef> fmgNames, Param.Row context,
        dynamic oldval)
    {
        List<string> textsToPrint = UICache.GetCached(ownerScreen, (int)oldval, "PARAM META FMGREF", () =>
        {
            List<(string, FMGBank.EntryGroup)> refs = resolveFMGRefs(fmgNames, context, oldval);
            return refs.Where(x => x.Item2 != null)
                .Select(x =>
                {
                    FMGBank.EntryGroup group = x.Item2;
                    var toPrint = "";
                    if (!string.IsNullOrWhiteSpace(group.Title?.Text))
                    {
                        toPrint += '\n' + group.Title.Text;
                    }

                    if (!string.IsNullOrWhiteSpace(group.Summary?.Text))
                    {
                        toPrint += '\n' + group.Summary.Text;
                    }

                    if (!string.IsNullOrWhiteSpace(group.Description?.Text))
                    {
                        toPrint += '\n' + group.Description.Text;
                    }

                    if (!string.IsNullOrWhiteSpace(group.TextBody?.Text))
                    {
                        toPrint += '\n' + group.TextBody.Text;
                    }

                    if (!string.IsNullOrWhiteSpace(group.ExtraText?.Text))
                    {
                        toPrint += '\n' + group.ExtraText.Text;
                    }

                    return toPrint.TrimStart();
                }).ToList();
        });
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.5f, 0.5f, 1.0f));
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

    public static void EnumNameText(ParamEnum pEnum)
    {
        if (pEnum != null && pEnum.name != null && CFG.Current.Param_HideEnums == false) //Move preference
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
            ImGui.TextUnformatted($@"   {pEnum.name}");
            ImGui.PopStyleColor();
        }
    }

    public static void EnumValueText(Dictionary<string, string> enumValues, string value)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.5f, 0.5f, 1.0f));
        ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
        ImGui.PopStyleColor();
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
                AssetLocator al = ParamBank.PrimaryBank.AssetLocator;
                ExtRefItem(context, fieldName, $"modded {currentRef.name}", matchedExtRefPath, al.GameModDirectory,
                    cacheOwner);
                ExtRefItem(context, fieldName, $"vanilla {currentRef.name}", matchedExtRefPath,
                    al.GameRootDirectory, cacheOwner);
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
        Param.Row context, List<FMGRef> fmgRefs, ParamEnum Enum)
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
                (string, FMGBank.EntryGroup)? primaryRef =
                    resolveFMGRefs(fmgRefs, context, oldval)?.FirstOrDefault();
                if (primaryRef?.Item2 != null)
                {
                    EditorCommandQueue.AddCommand($@"text/select/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                }
            }
        }
    }

    public static bool ParamRefEnumContextMenuItems(ParamBank bank, object oldval, ref object newval,
        List<ParamRef> RefTypes, Param.Row context, List<FMGRef> fmgRefs, ParamEnum Enum, ActionManager executor)
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

        if (Enum != null)
        {
            result |= PropertyRowEnumContextItems(Enum, oldval, ref newval);
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
                var rt = rf.param;
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
                                newval = Convert.ChangeType(r.ID - meta.FixedOffset - rf.offset, oldval.GetType());
                            }
                            else
                            {
                                newval = Convert.ChangeType(r.ID - rf.offset, oldval.GetType());
                            }

                            _refContextCurrentAutoComplete = "";
                            return true;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog("Unable to convert value into param field's type'", LogLevel.Warning,
                                TaskLogs.LogPriority.Normal, e);
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
        List<(string, FMGBank.EntryGroup)> refs = resolveFMGRefs(reftypes, context, oldval);
        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);
        foreach ((var name, FMGBank.EntryGroup group) in refs)
        {
            if (ImGui.Selectable($@"Goto {name} Text"))
            {
                EditorCommandQueue.AddCommand($@"text/select/{name}/{group.ID}");
            }

            if (context == null || executor == null)
            {
                continue;
            }

            foreach (FieldInfo field in group.GetType().GetFields()
                         .Where(propinfo => propinfo.FieldType == typeof(FMG.Entry)))
            {
                var entry = (FMG.Entry)field.GetValue(group);
                if (!string.IsNullOrWhiteSpace(entry?.Text) &&
                    (ctrlDown || string.IsNullOrWhiteSpace(context.Name)) &&
                    ImGui.Selectable($@"Inherit referenced fmg {field.Name} ({entry?.Text})"))
                {
                    executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"),
                        context, entry?.Text));
                }

                if (entry != null && (ctrlDown || string.IsNullOrWhiteSpace(entry?.Text)) &&
                    !string.IsNullOrWhiteSpace(context.Name) &&
                    ImGui.Selectable($@"Proliferate name to referenced fmg {field.Name} ({name})"))
                {
                    executor.ExecuteAction(new PropertiesChangedAction(entry.GetType().GetProperty("Text"), entry,
                        context.Name));
                }
            }
        }
    }

    public static bool PropertyRowEnumContextItems(ParamEnum en, object oldval, ref object newval)
    {
        if (ImGui.BeginChild("EnumList",
                new Vector2(0, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, en.values.Count))))
        {
            try
            {
                foreach (KeyValuePair<string, string> option in en.values)
                {
                    if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                    {
                        newval = Convert.ChangeType(option.Key, oldval.GetType());
                        ImGui.EndChild();
                        return true;
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
        if (ImGui.BeginMenu("Search for references..."))
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
                            List<Param.Row> rows = UICache.GetCached(screen, (bank, currentParam, currentID, pref),
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
            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Indent();
            CalcCorrectDefinition ccd = meta.CalcCorrectDef;
            SoulCostDefinition scd = meta.SoulCostDef;
            float[] values;
            int xOffset;
            float minY;
            float maxY;
            if (scd != null && scd.cost_row == row.ID)
            {
                (values, maxY) = UICache.GetCached(screen, row, "soulCostData",
                    () => ParamUtils.getSoulCostData(scd, row));
                ImGui.PlotLines("##graph", ref values[0], values.Length, 0, "", 0, maxY,
                    new Vector2(ImGui.GetColumnWidth(-1) - 30.0f, (ImGui.GetColumnWidth(-1) * 0.5625f) - 30.0f));
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
                    if (!pref.param.Equals(currentParam))
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
        var searchTerm = pref.conditionField != null
            ? $@"prop {fieldName} ^{currentID}$ && prop {pref.conditionField} ^{pref.conditionValue}$"
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
        ImGui_DisplayPropertyInfo(prop.PropertyType, prop.Name);
    }

    /// <summary>
    ///     Displays information about the provided property.
    /// </summary>
    public static void ImGui_DisplayPropertyInfo(Type propType, string fieldName, string altName = null, int arrayLength = -1, int bitSize = -1)
    {
        if (!string.IsNullOrWhiteSpace(altName))
        {
            fieldName += $"  /  {altName}";
        }

        ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(fieldName, "", true));
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

        ImGui.Separator();
    }
}
