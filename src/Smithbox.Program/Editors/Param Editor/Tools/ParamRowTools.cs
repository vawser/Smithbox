using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamRowTools
{
    #region Quick Search
    public static void ParamQuickSearch(ParamView curView, string currentParam, int currentID)
    {
        if (ImGui.MenuItem("Search for references in tool"))
        {
            curView.Editor.ToolMenu.FieldValueFinder.SearchText = $"{currentID}";
            curView.Editor.ToolMenu.FieldValueFinder.CachedSearchText = curView.Editor.ToolMenu.FieldValueFinder.SearchText;

            curView.Editor.ToolMenu.FieldValueFinder.Results = curView.Editor.ToolMenu.FieldValueFinder.ConstructResults();
            curView.Editor.ToolMenu.FieldValueFinder.Results.Sort();
        }
        UIHelper.Tooltip("Quick use action for searching in 'Find Field Value Instances' tool with this row ID.");
    }
    #endregion

    #region Param Reverse Lookup
    public static void ParamReverseLookup_Value(ParamView curView, string currentParam,
        int currentID)
    {
        if (ImGui.BeginMenu("Search for references"))
        {
            Dictionary<string, List<(string, ParamRef)>> items = CacheBank.GetCached(curView.Editor, (curView.GetPrimaryBank(), currentParam),
                () => ParamRefReverseLookupFieldItems(curView, currentParam));

            foreach (KeyValuePair<string, List<(string, ParamRef)>> paramitems in items)
            {
                if (ImGui.BeginMenu($@"in {paramitems.Key}..."))
                {
                    foreach ((var fieldName, ParamRef pref) in paramitems.Value)
                    {
                        if (ImGui.BeginMenu($@"in {fieldName}"))
                        {
                            List<Param.Row> rows = CacheBank.GetCached(curView.Editor, (curView.GetPrimaryBank(), currentParam, currentID, paramitems.Key, fieldName),
                                () => ParamRefReverseLookupRowItems(curView, paramitems.Key, fieldName, currentID,
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

    private static Dictionary<string, List<(string, ParamRef)>> ParamRefReverseLookupFieldItems(ParamView curView, string currentParam)
    {
        Dictionary<string, List<(string, ParamRef)>> items = new();
        foreach (KeyValuePair<string, Param> param in curView.GetPrimaryBank().Params)
        {
            List<(string, ParamRef)> paramitems = new();

            var curMeta = curView.GetParamData().GetParamMeta(param.Value.AppliedParamdef);

            if (param.Value.AppliedParamdef == null)
                continue;

            //get field
            foreach (PARAMDEF.Field f in param.Value.AppliedParamdef.Fields)
            {
                var meta = curView.GetParamData().GetParamFieldMeta(curMeta, f);
                if (meta == null || meta.RefTypes == null)
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

    private static List<Param.Row> ParamRefReverseLookupRowItems(ParamView curView, string paramName, string fieldName,
        int currentID, ParamRef pref)
    {
        var searchTerm = pref.ConditionField != null
            ? $@"prop {fieldName} ^{currentID}$ && prop {pref.ConditionField} ^{pref.ConditionValue}$"
            : $@"prop {fieldName} ^{currentID}$";

        return curView.MassEdit.RSE.Search((curView.GetPrimaryBank(), curView.GetPrimaryBank().Params[paramName]), searchTerm, false, false);
    }

    #endregion

    #region Sort Rows
    public static void SortRows(ParamView activeView)
    {
        if (activeView.Selection.ActiveParamExists())
        {
            var action = MassParamEditOther.SortRows(
                activeView,
                activeView.GetPrimaryBank(),
                activeView.Selection.GetActiveParam());

            activeView.Editor.ActionManager.ExecuteAction(action);

            TaskLogs.AddLog($"Param rows sorted for " +
                $"{activeView.Selection.GetActiveParam()}");
        }
    }
    #endregion
}
