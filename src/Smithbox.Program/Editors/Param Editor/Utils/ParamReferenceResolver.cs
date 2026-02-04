using Andre.Formats;
using Octokit;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public static class ParamReferenceResolver
{
    public static List<(string, Param.Row, string)> ResolveParamReferences(ParamEditorView curView, List<ParamRef> paramRefs, Param.Row context, dynamic oldval)
    {
        List<(string, Param.Row, string)> rows = new();
        if (curView.GetPrimaryBank().Params == null)
        {
            return rows;
        }

        if (paramRefs == null)
            return rows;

        var originalValue = -1;
        var success = int.TryParse($"{oldval}", out originalValue);

        if (!success)
            return rows;

        foreach (ParamRef rf in paramRefs)
        {
            Param.Cell? c = context?[rf.ConditionField];

            var inactiveRef = false;

            if (c != null && context != null)
            {
                var fieldValue = c.Value.Value;
                int intValue = 0;
                var valueConvertSuccess = int.TryParse($"{fieldValue}", out intValue);

                // Only check if field value is valid uint
                if (valueConvertSuccess)
                {
                    inactiveRef = intValue != rf.ConditionValue;
                }
            }

            if (inactiveRef)
            {
                continue;
            }

            var rt = rf.ParamName;
            var hint = "";
            if (curView.GetPrimaryBank().Params.ContainsKey(rt))
            {
                var altval = originalValue;
                if (rf.Offset != 0)
                {
                    altval += rf.Offset;
                    hint += rf.Offset > 0 ? "+" + rf.Offset : rf.Offset.ToString();
                }

                Param param = curView.GetPrimaryBank().Params[rt];
                var meta = curView.GetParamData().GetParamMeta(curView.GetPrimaryBank().Params[rt].AppliedParamdef);
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

                    r = curView.GetPrimaryBank().Params[rt][altval];
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

    public static List<TextResult> ResolveTextReferences(ParamEditorView curView, List<FMGRef> fmgRefs, Param.Row context, dynamic oldval)
    {
        List<TextResult> newTextResults = new();

        if (fmgRefs == null)
            return newTextResults;

        foreach (var entry in fmgRefs)
        {
            Param.Cell? c = context?[entry.conditionField];

            bool cont = true;

            if (context != null && c != null)
            {
                if (Convert.ToInt32(c.Value.Value) != entry.conditionValue)
                {
                    cont = false;
                }
            }

            if (cont)
            {
                uint tempVal = (uint)oldval;

                if (curView.Project.Handler.TextEditor != null)
                {
                    var activeView = curView.Project.Handler.TextEditor.ViewHandler.ActiveView;

                    if (activeView != null)
                    {
                        TextResult result = TextFinder.GetTextResult(activeView, entry.fmg, (int)tempVal, entry.offset);

                        if (result != null)
                        {
                            newTextResults.Add(result);
                        }
                    }
                }
            }
        }

        return newTextResults;
    }

    public static string ResolveExternalReferences(List<string> matchedExtRefPath, string baseDir)
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
}
