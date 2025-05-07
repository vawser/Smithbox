using Andre.Formats;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Editors.TextEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Decorators;

public class ReferenceResolver
{
    /// <summary>
    /// Resolve the param references for the current row
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="paramRefs"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    /// <returns></returns>
    public static List<(string, Param.Row, string)> ResolveParamReferences(ParamEditorScreen editor, ParamBank bank, List<ParamRef> paramRefs,
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
            if (bank.Params.ContainsKey(rt))
            {
                var altval = originalValue;
                if (rf.Offset != 0)
                {
                    altval += rf.Offset;
                    hint += rf.Offset > 0 ? "+" + rf.Offset : rf.Offset.ToString();
                }

                Param param = bank.Params[rt];
                var meta = editor.Project.ParamData.GetParamMeta(bank.Params[rt].AppliedParamdef);
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

    /// <summary>
    /// Resolve the FMG references for the current row
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="fmgRefs"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    /// <returns></returns>
    public static List<TextResult> ResolveTextReferences(ParamEditorScreen editor, List<FMGRef> fmgRefs, Param.Row context,
        dynamic oldval)
    {
        List<TextResult> newTextResults = new();

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

                if (editor.BaseEditor.ProjectManager.SelectedProject.TextEditor != null)
                {
                    var textEditor = editor.BaseEditor.ProjectManager.SelectedProject.TextEditor;

                    TextResult result = TextFinder.GetTextResult(textEditor, entry.fmg, (int)tempVal, entry.offset);

                    if (result != null)
                    {
                        newTextResults.Add(result);
                    }
                }

            }
        }

        return newTextResults;
    }

    /// <summary>
    /// Resolve the external reference path
    /// </summary>
    /// <param name="matchedExtRefPath"></param>
    /// <param name="baseDir"></param>
    /// <returns></returns>
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
