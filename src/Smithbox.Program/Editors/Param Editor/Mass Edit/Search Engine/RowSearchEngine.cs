using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class RowSearchEngine : SearchEngine<(ParamBank, Param), Param.Row>
{
    public ParamEditorView CurrentView;

    public RowSearchEngine(ParamEditorView curView)
    {
        CurrentView = curView;
        Setup();
    }

    internal void Setup()
    {
        var meta = CurrentView.GetParamData().ParamMeta;
        var pBank = CurrentView.GetPrimaryBank();
        var vBank = CurrentView.GetVanillaBank();
        var auxBanks = CurrentView.GetParamData().AuxBanks;

        unpacker = param => new List<Param.Row>(param.Item2.Rows);

        filterList.Add("all", newCmd(new string[0],
            "Selects all rows", noArgs(context =>
            {
                return row => true;
            }
            )));

        filterList.Add("modified", newCmd(new string[0],
            "Selects rows which do not match the vanilla version, or are added. Ignores row name", noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                HashSet<int> cache = context.Item1.GetVanillaDiffRows(paramName);
                return row => cache.Contains(row.ID);
            }
            )));

        filterList.Add("named", newCmd(new string[0],
            "Selects rows whose name isn't blank or null", (args, lenient) =>
            {
                return noContext(row => row.Name != null || row.Name != "");
            }));

        filterList.Add("selected", newCmd(new string[0],
            "Selects rows that are already manually selected", (args, lenient) =>
            {
                var selectedRows = CurrentView.Selection.GetSelectedRows();

                return noContext(row => selectedRows.Contains(row));
            }));

        filterList.Add("added", newCmd(new string[0], "Selects rows where the ID is not found in the vanilla param",
            noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                if (!vBank.Params.ContainsKey(paramName))
                {
                    return row => true;
                }

                Param vanilParam = vBank.Params[paramName];
                return row => vanilParam[row.ID] == null;
            }
            )));

        filterList.Add("mergeable", newCmd(new string[0],
            "Selects rows which are not modified in the primary regulation or parambnd and there is exactly one equivalent row in another regulation or parambnd that is modified",
            noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                if (paramName == null)
                {
                    return row => true;
                }

                HashSet<int> pCache = pBank.GetVanillaDiffRows(paramName);
                List<(HashSet<int>, HashSet<int>)> auxCaches = auxBanks.Select(x =>
                    (x.Value.GetPrimaryDiffRows(paramName), x.Value.GetVanillaDiffRows(paramName))).ToList();
                return row =>
                    !pCache.Contains(row.ID) &&
                    auxCaches.Where(x => x.Item2.Contains(row.ID) && x.Item1.Contains(row.ID)).Count() == 1;
            }
            ), () => auxBanks.Count > 0));

        filterList.Add("conflicts", newCmd(new string[0],
            "Selects rows which, among all equivalents in the primary and additional regulations or parambnds, there is more than row 1 which is modified",
            noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                HashSet<int> pCache = pBank.GetVanillaDiffRows(paramName);
                List<(HashSet<int>, HashSet<int>)> auxCaches = auxBanks.Select(x =>
                    (x.Value.GetPrimaryDiffRows(paramName), x.Value.GetVanillaDiffRows(paramName))).ToList();
                return row =>
                    (pCache.Contains(row.ID) ? 1 : 0) + auxCaches
                        .Where(x => x.Item2.Contains(row.ID) && x.Item1.Contains(row.ID)).Count() > 1;
            }
            ), () => auxBanks.Count > 0));

        filterList.Add("id", newCmd(new[] { "row id (regex)" }, "Selects rows whose ID matches the given regex",
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0].ToLower()) : new Regex($@"^{args[0]}$");
                return noContext(row => rx.IsMatch(row.ID.ToString()));
            }));

        filterList.Add("idrange", newCmd(new[] { "row id minimum (inclusive)", "row id maximum (inclusive)" },
            "Selects rows whose ID falls in the given numerical range", (args, lenient) =>
            {
                var floor = double.Parse(args[0]);
                var ceil = double.Parse(args[1]);
                return noContext(row => row.ID >= floor && row.ID <= ceil);
            }));

        filterList.Add("name", newCmd(new[] { "row name (regex)" },
            "Selects rows whose Name matches the given regex", (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(row => rx.IsMatch(row.Name == null ? "" : row.Name));
            }));

        filterList.Add("prop", newCmd(new[] { "field internalName", "field value (regex)" },
            "Selects rows where the specified field has a value that matches the given regex", (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[1], RegexOptions.IgnoreCase) : new Regex($@"^{args[1]}$");
                var field = args[0];
                return noContext(row =>
                {
                    Param.Cell? cq = row[field];
                    if (cq == null)
                    {
                        throw new Exception();
                    }

                    Param.Cell c = cq.Value;
                    var term = c.Value.ToParamEditorString();
                    return rx.IsMatch(term);
                });
            }));

        filterList.Add("proprange", newCmd(
            new[] { "field internalName", "field value minimum (inclusive)", "field value maximum (inclusive)" },
            "Selects rows where the specified field has a value that falls in the given numerical range",
            (args, lenient) =>
            {
                var field = args[0];
                var floor = double.Parse(args[1]);
                var ceil = double.Parse(args[2]);
                return noContext(row =>
                {
                    Param.Cell? c = row[field];
                    if (c == null)
                    {
                        throw new Exception();
                    }

                    return Convert.ToDouble(c.Value.Value) >= floor && Convert.ToDouble(c.Value.Value) <= ceil;
                });
            }));

        filterList.Add("positive", newCmd(
            new[] { "field internalName" },
            "Selects rows where the specified field has a value that is a positive, non-zero number",
            (args, lenient) =>
            {
                var field = args[0];

                var ceil = float.PositiveInfinity;

                return noContext(row =>
                {
                    Param.Cell? c = row[field];
                    if (c == null)
                    {
                        throw new Exception();
                    }

                    return Convert.ToDouble(c.Value.Value) > 0 && Convert.ToDouble(c.Value.Value) <= ceil;
                });
            }));

        filterList.Add("negative", newCmd(
            new[] { "field internalName" },
            "Selects rows where the specified field has a value that is a negative, non-zero number",
            (args, lenient) =>
            {
                var field = args[0];

                var floor = float.NegativeInfinity;

                return noContext(row =>
                {
                    Param.Cell? c = row[field];
                    if (c == null)
                    {
                        throw new Exception();
                    }

                    return Convert.ToDouble(c.Value.Value) < 0 && Convert.ToDouble(c.Value.Value) >= floor;
                });
            }));

        filterList.Add("propref", newCmd(new[] { "field internalName", "referenced row name (regex)" },
            "Selects rows where the specified field that references another param has a value referencing a row whose name matches the given regex",
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[1], RegexOptions.IgnoreCase) : new Regex($@"^{args[1]}$");
                var field = args[0];
                return context =>
                {
                    var paramMeta = meta[context.Item2.AppliedParamdef];

                    List<ParamRef> validFields = paramMeta
                        .GetField(context.Item2.AppliedParamdef.Fields.Find(f => f.InternalName.Equals(field))).RefTypes
                        .FindAll(p => pBank.Params.ContainsKey(p.ParamName));

                    return row =>
                    {
                        Param.Cell? c = row[field];
                        if (c == null)
                        {
                            throw new Exception();
                        }

                        var val = (int)c.Value.Value;
                        foreach (ParamRef rt in validFields)
                        {
                            Param.Row r = pBank.Params[rt.ParamName][val];
                            if (r != null && rx.IsMatch(r.Name ?? ""))
                            {
                                return true;
                            }
                        }

                        return false;
                    };
                };
            }));

        filterList.Add("propwhere", newCmd(new[] { "field internalName", "cell/field selector" },
            "Selects rows where the specified field appears when the given cell/field search is given",
            (args, lenient) =>
            {
                var field = args[0];
                return context =>
                {
                    var paramName = context.Item1.GetKeyForParam(context.Item2);
                    IReadOnlyList<Param.Column> cols = context.Item2.Columns;
                    (ParamEditorPseudoColumn, Param.Column) testCol = context.Item2.GetCol(field);
                    return row =>
                    {
                        (string paramName, Param.Row row) cseSearchContext = (paramName, row);
                        List<(ParamEditorPseudoColumn, Param.Column)> res = CurrentView.MassEdit.CSE.Search(cseSearchContext,
                            new List<(ParamEditorPseudoColumn, Param.Column)> { testCol }, args[1], lenient, false);
                        return res.Contains(testCol);
                    };
                };
            }));

        filterList.Add("fmg", newCmd(new[] { "fmg title (regex)" },
            "Selects rows which have an attached FMG and that FMG's text matches the given regex",
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return context =>
                {
                    var paramName = context.Item1.GetKeyForParam(context.Item2);
                    List<FMG.Entry> fmgEntries = new();

                    fmgEntries = ParamFmgUtils.GetFmgEntriesByAssociatedParam(CurrentView.Editor, paramName);

                    Dictionary<int, FMG.Entry> _cache = new();
                    foreach (FMG.Entry fmgEntry in fmgEntries)
                    {
                        _cache[fmgEntry.ID] = fmgEntry;
                    }

                    return row =>
                    {
                        if (!_cache.ContainsKey(row.ID))
                        {
                            return false;
                        }

                        FMG.Entry e = _cache[row.ID];
                        return e != null && rx.IsMatch(e.Text ?? "");
                    };
                };
            }));

        filterList.Add("vanillaprop", newCmd(new[] { "field internalName", "field value (regex)" },
            "Selects rows where the vanilla equivilent of that row has a value for the given field that matches the given regex",
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[1], RegexOptions.IgnoreCase) : new Regex($@"^{args[1]}$");
                var field = args[0];
                return param =>
                {
                    Param vparam = vBank.GetParamFromName(param.Item1.GetKeyForParam(param.Item2));
                    return row =>
                    {
                        Param.Row vrow = vparam[row.ID];
                        if (vrow == null)
                        {
                            return false;
                        }

                        Param.Cell? cq = vrow[field];
                        if (cq == null)
                        {
                            throw new Exception();
                        }

                        Param.Cell c = cq.Value;
                        var term = c.Value.ToParamEditorString();
                        return rx.IsMatch(term);
                    };
                };
            }));

        filterList.Add("vanillaproprange", newCmd(
            new[] { "field internalName", "field value minimum (inclusive)", "field value maximum (inclusive)" },
            "Selects rows where the vanilla equivilent of that row has a value for the given field that falls in the given numerical range",
            (args, lenient) =>
            {
                var field = args[0];
                var floor = double.Parse(args[1]);
                var ceil = double.Parse(args[2]);
                return param =>
                {
                    Param vparam = vBank.GetParamFromName(param.Item1.GetKeyForParam(param.Item2));
                    return row =>
                    {
                        Param.Row vrow = vparam[row.ID];
                        if (vrow == null)
                        {
                            return false;
                        }

                        Param.Cell? c = vrow[field];
                        if (c == null)
                        {
                            throw new Exception();
                        }

                        return Convert.ToDouble(c.Value.Value) >= floor && Convert.ToDouble(c.Value.Value) <= ceil;
                    };
                };
            }));

        filterList.Add("auxprop", newCmd(new[] { "parambank name", "field internalName", "field value (regex)" },
            "Selects rows where the equivilent of that row in the given regulation or parambnd has a value for the given field that matches the given regex.\nCan be used to determine if an aux row exists.",
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[2], RegexOptions.IgnoreCase) : new Regex($@"^{args[2]}$");
                var field = args[1];
                ParamBank bank;
                if (!auxBanks.TryGetValue(args[0], out bank))
                {
                    throw new Exception("Unable to find auxbank " + args[0]);
                }

                return param =>
                {
                    Param vparam = bank.GetParamFromName(param.Item1.GetKeyForParam(param.Item2));
                    return row =>
                    {
                        Param.Row vrow = vparam[row.ID];
                        if (vrow == null)
                        {
                            return false;
                        }

                        Param.Cell? cq = vrow[field];
                        if (cq == null)
                        {
                            throw new Exception();
                        }

                        Param.Cell c = cq.Value;
                        var term = c.Value.ToParamEditorString();
                        return rx.IsMatch(term);
                    };
                };
            }, () => auxBanks.Count > 0));

        filterList.Add("auxproprange", newCmd(
            new[]
            {
                "parambank name", "field internalName", "field value minimum (inclusive)",
                "field value maximum (inclusive)"
            },
            "Selects rows where the equivilent of that row in the given regulation or parambnd has a value for the given field that falls in the given range",
            (args, lenient) =>
            {
                var field = args[0];
                var floor = double.Parse(args[1]);
                var ceil = double.Parse(args[2]);
                ParamBank bank;
                if (!auxBanks.TryGetValue(args[0], out bank))
                {
                    throw new Exception("Unable to find auxbank " + args[0]);
                }

                return param =>
                {
                    Param vparam = bank.GetParamFromName(param.Item1.GetKeyForParam(param.Item2));
                    return row =>
                    {
                        Param.Row vrow = vparam[row.ID];
                        Param.Cell? c = vrow[field];
                        if (c == null)
                        {
                            throw new Exception();
                        }

                        return Convert.ToDouble(c.Value.Value) >= floor && Convert.ToDouble(c.Value.Value) <= ceil;
                    };
                };
            }, () => auxBanks.Count > 0));

        filterList.Add("semijoin",
            newCmd(
                new[]
                {
                    "this field internalName", "other param", "other param field internalName",
                    "other param row search"
                },
                "Selects all rows where the value of a given field is any of the values in the second given field found in the given param using the given row selector",
                (args, lenient) =>
                {
                    var thisField = args[0];
                    var otherParam = args[1];
                    var otherField = args[2];
                    var otherSearchTerm = args[3];
                    Param otherParamReal;
                    if (!pBank.Params.TryGetValue(otherParam, out otherParamReal))
                    {
                        throw new Exception("Could not find param " + otherParam);
                    }

                    List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, otherParamReal), otherSearchTerm,
                        lenient, false);
                    (ParamEditorPseudoColumn, Param.Column) otherFieldReal = otherParamReal.GetCol(otherField);
                    if (!otherFieldReal.IsColumnValid())
                    {
                        throw new Exception("Could not find field " + otherField);
                    }

                    HashSet<string> possibleValues = rows.Select(x => x.Get(otherFieldReal).ToParamEditorString())
                        .Distinct().ToHashSet();
                    return param =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) thisFieldReal = param.Item2.GetCol(thisField);
                        if (!thisFieldReal.IsColumnValid())
                        {
                            throw new Exception("Could not find field " + thisField);
                        }

                        return row =>
                        {
                            var toFind = row.Get(thisFieldReal).ToParamEditorString();
                            return possibleValues.Contains(toFind);
                        };
                    };
                }));

        filterList.Add("unique", newCmd(new string[] { "field" }, "Selects all rows where the value in the given field is unique", (args, lenient) =>
        {
            string field = args[0].Replace(@"\s", " ");
            return (param) =>
            {
                var col = param.Item2.GetCol(field);
                if (!col.IsColumnValid())
                    throw new Exception("Could not find field " + field);
                var distribution = ParamUtils.GetParamValueDistribution(param.Item2.Rows, col);
                var setOfDuped = distribution.Where((entry, linqi) => entry.Item2 > 1).Select((entry, linqi) => entry.Item1).ToHashSet();
                return (row) =>
                {
                    return !setOfDuped.Contains(row.Get(col));
                };
            };
        }));

        defaultFilter = newCmd(new[] { "row ID or Name (regex)" },
            "Selects rows where either the ID or Name matches the given regex, except in strict/massedit mode",
            (args, lenient) =>
            {
                if (!lenient)
                {
                    return noContext(row => false);
                }

                Regex rx = new(args[0], RegexOptions.IgnoreCase);
                return paramContext =>
                {
                    var paramName = paramContext.Item1.GetKeyForParam(paramContext.Item2);

                    if (CurrentView.Project.Handler.TextEditor != null)
                    {
                        List<FMG.Entry> fmgEntries = new();
                        ParamFmgUtils.GetFmgEntriesByAssociatedParam(CurrentView.Project.Handler.ParamEditor, paramName);

                        if (fmgEntries.Count == 0)
                        {
                            return row => rx.IsMatch(row.Name ?? "") || rx.IsMatch(row.ID.ToString());
                        }

                        Dictionary<int, FMG.Entry> _cache = new();
                        foreach (FMG.Entry fmgEntry in fmgEntries)
                        {
                            _cache[fmgEntry.ID] = fmgEntry;
                        }

                        return row =>
                        {
                            if (rx.IsMatch(row.Name ?? "") || rx.IsMatch(row.ID.ToString()))
                            {
                                return true;
                            }

                            if (!_cache.ContainsKey(row.ID))
                            {
                                return false;
                            }

                            FMG.Entry e = _cache[row.ID];
                            return e != null && rx.IsMatch(e.Text ?? "");
                        };
                    }
                    else
                    {
                        return row => rx.IsMatch(row.Name ?? "") || rx.IsMatch(row.ID.ToString());
                    }
                };
            });
    }
}