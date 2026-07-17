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
            LOC.Get("PARAM_RSE_All_TT"), 
            noArgs(context =>
            {
                return row => true;
            }
            )));

        filterList.Add("modified", newCmd(new string[0],
            LOC.Get("PARAM_RSE_Modified_TT"), 
            noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                HashSet<int> cache = context.Item1.GetVanillaDiffRows(paramName);
                return row => cache.Contains(row.ID);
            }
            )));

        filterList.Add("auxmodified", newCmd(new string[0],
            LOC.Get("PARAM_RSE_AuxModified_TT"), noArgs(context =>
            {
                var paramName = context.Item1.GetKeyForParam(context.Item2);
                HashSet<int> cache = context.Item1.GetPrimaryDiffRows(paramName);
                return row => cache.Contains(row.ID);
            }
            )));

        filterList.Add("named", newCmd(new string[0],
            LOC.Get("PARAM_RSE_Named_TT"), (args, lenient) =>
            {
                return noContext(row => row.Name != null || row.Name != "");
            }));

        filterList.Add("selected", newCmd(new string[0],
            LOC.Get("PARAM_RSE_Selected_TT"), (args, lenient) =>
            {
                var selectedRows = CurrentView.Selection.GetSelectedRows();

                return noContext(row => selectedRows.Contains(row));
            }));

        filterList.Add("added", newCmd(new string[0], 
            LOC.Get("PARAM_RSE_Added_TT"),
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
            LOC.Get("PARAM_RSE_Mergable"),
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
            LOC.Get("PARAM_RSE_Conflicts_TT"),
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

        filterList.Add("id", newCmd(new[] { 
            LOC.Get("PARAM_RSE_ID_Hint_1")}, 
            LOC.Get("PARAM_RSE_ID_Hint"),
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0].ToLower()) : new Regex($@"^{args[0]}$");
                return noContext(row => rx.IsMatch(row.ID.ToString()));
            }));

        filterList.Add("idrange", newCmd(new[] { 
            LOC.Get("PARAM_RSE_IdRange_Hint_1"),
            LOC.Get("PARAM_RSE_IdRange_Hint_2")},
            LOC.Get("PARAM_RSE_IdRange_TT"), 
            (args, lenient) =>
            {
                var floor = double.Parse(args[0]);
                var ceil = double.Parse(args[1]);
                return noContext(row => row.ID >= floor && row.ID <= ceil);
            }));

        filterList.Add("name", newCmd(new[] { 
            LOC.Get("PARAM_RSE_Name_Hint_1")},
            LOC.Get("PARAM_RSE_Name_TT"), 
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(row => rx.IsMatch(row.Name == null ? "" : row.Name));
            }));

        filterList.Add("prop", newCmd(new[] { 
            LOC.Get("PARAM_RSE_Prop_Hint_1"),
            LOC.Get("PARAM_RSE_Prop_Hint_2")},
            LOC.Get("PARAM_RSE_Prop_Hint_TT"), 
            (args, lenient) =>
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

        filterList.Add("proprange", newCmd(new[] {
            LOC.Get("PARAM_RSE_PropRange_Hint_1"),
            LOC.Get("PARAM_RSE_PropRange_Hint_2"),
            LOC.Get("PARAM_RSE_PropRange_Hint_3")},
            LOC.Get("PARAM_RSE_PropRange_TT"),
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

        filterList.Add("positive", newCmd(new[] { 
            LOC.Get("PARAM_RSE_Positive_Hint_1")},
            LOC.Get("PARAM_RSE_Positive_TT"),
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

        filterList.Add("negative", newCmd(new[] {
            LOC.Get("PARAM_RSE_Negative_Hint_1")},
            LOC.Get("PARAM_RSE_Negative_TT"),
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

        filterList.Add("propref", newCmd(new[] { 
            LOC.Get("PARAM_RSE_PropRef_Hint_1"),
            LOC.Get("PARAM_RSE_PropRef_Hint_2")},
            LOC.Get("PARAM_RSE_PropRef_TT"),
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

        filterList.Add("propwhere", newCmd(new[] {
            LOC.Get("PARAM_RSE_PropWhere_Hint_1"),
            LOC.Get("PARAM_RSE_PropWhere_Hint_2")},
            LOC.Get("PARAM_RSE_PropWhere_TT"),
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

        filterList.Add("fmg", newCmd(new[] { 
            LOC.Get("PARAM_RSE_Fmg_Hint_1")},
            LOC.Get("PARAM_RSE_Fmg_TT"),
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return context =>
                {
                    var paramName = context.Item1.GetKeyForParam(context.Item2);
                    List<FMG.Entry> fmgEntries = new();

                    fmgEntries = ParamFmgUtils.GetFmgEntriesByAssociatedParam(CurrentView.Editor, paramName, "Title");

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

        filterList.Add("vanillaprop", newCmd(new[] { 
            LOC.Get("PARAM_RSE_VanillaProp_Hint_1"),
            LOC.Get("PARAM_RSE_VanillaProp_Hint_2")},
            LOC.Get("PARAM_RSE_VanillaProp_TT"),
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

        filterList.Add("vanillaproprange", newCmd(new[] { 
            LOC.Get("PARAM_RSE_VanillaPropRange_Hint_1"),
            LOC.Get("PARAM_RSE_VanillaPropRange_Hint_2"),
            LOC.Get("PARAM_RSE_VanillaPropRange_Hint_3")},
            LOC.Get("PARAM_RSE_VanillaPropRange_TT"),
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

        filterList.Add("auxprop", newCmd(new[] { 
            LOC.Get("PARAM_RSE_AuxProp_Hint_1"),
            LOC.Get("PARAM_RSE_AuxProp_Hint_2"),
            LOC.Get("PARAM_RSE_AuxProp_Hint_3")},
            LOC.Get("PARAM_RSE_AuxProp_TT"),
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[2], RegexOptions.IgnoreCase) : new Regex($@"^{args[2]}$");
                var field = args[1];
                ParamBank bank;
                if (!auxBanks.TryGetValue(args[0], out bank))
                {
                    throw new Exception(
                        LOC.Get("PARAM_RSE_AuxProp_Error_Missing_AuxBank", args[0]));
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

        filterList.Add("auxproprange", newCmd(new[] {
            LOC.Get("PARAM_RSE_AuxPropRange_Hint_1"),
            LOC.Get("PARAM_RSE_AuxPropRange_Hint_2"),
            LOC.Get("PARAM_RSE_AuxPropRange_Hint_3"),
            LOC.Get("PARAM_RSE_AuxPropRange_Hint_4")},
            LOC.Get("PARAM_RSE_AuxPropRange_TT"),
            (args, lenient) =>
            {
                var field = args[0];
                var floor = double.Parse(args[1]);
                var ceil = double.Parse(args[2]);
                ParamBank bank;
                if (!auxBanks.TryGetValue(args[0], out bank))
                {
                    throw new Exception(
                        LOC.Get("PARAM_RSE_AuxProp_Error_Missing_AuxBank", args[0]));
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
            newCmd(new[] {
                LOC.Get("PARAM_RSE_SemiJoin_Hint_1"),
                LOC.Get("PARAM_RSE_SemiJoin_Hint_2"),
                LOC.Get("PARAM_RSE_SemiJoin_Hint_3"),
                LOC.Get("PARAM_RSE_SemiJoin_Hint_4")},
                LOC.Get("PARAM_RSE_SemiJoin_TT"),
                (args, lenient) =>
                {
                    var thisField = args[0];
                    var otherParam = args[1];
                    var otherField = args[2];
                    var otherSearchTerm = args[3];
                    Param otherParamReal;
                    if (!pBank.Params.TryGetValue(otherParam, out otherParamReal))
                    {
                        throw new Exception(
                            LOC.Get("PARAM_RSE_SemiJoin_Error_Cannot_Find_Param", otherParam));
                    }

                    List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, otherParamReal), otherSearchTerm,
                        lenient, false);
                    (ParamEditorPseudoColumn, Param.Column) otherFieldReal = otherParamReal.GetCol(otherField);
                    if (!otherFieldReal.IsColumnValid())
                    {
                        throw new Exception(
                            LOC.Get("PARAM_RSE_SemiJoin_Error_Cannot_Find_Field", otherField));
                    }

                    HashSet<string> possibleValues = rows.Select(x => x.Get(otherFieldReal).ToParamEditorString())
                        .Distinct().ToHashSet();
                    return param =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) thisFieldReal = param.Item2.GetCol(thisField);
                        if (!thisFieldReal.IsColumnValid())
                        {
                            throw new Exception(
                                LOC.Get("PARAM_RSE_SemiJoin_Error_Cannot_Find_Field", thisField));
                        }

                        return row =>
                        {
                            var toFind = row.Get(thisFieldReal).ToParamEditorString();
                            return possibleValues.Contains(toFind);
                        };
                    };
                }));

        filterList.Add("unique", newCmd(new string[] {  
            LOC.Get("PARAM_RSE_Unique_Hint_1")}, 
            LOC.Get("PARAM_RSE_Unique_TT"), 
            (args, lenient) =>
        {
            string field = args[0].Replace(@"\s", " ");
            return (param) =>
            {
                var col = param.Item2.GetCol(field);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_RSE_Unique_Error_Cannot_Find_Field", field));
                }

                var distribution = ParamUtils.GetParamValueDistribution(param.Item2.Rows, col);
                var setOfDuped = distribution.Where((entry, linqi) => entry.Item2 > 1).Select((entry, linqi) => entry.Item1).ToHashSet();
                return (row) =>
                {
                    return !setOfDuped.Contains(row.Get(col));
                };
            };
        }));

        defaultFilter = newCmd(new[] {
            LOC.Get("PARAM_RSE_Default_Hint_1")},
            LOC.Get("PARAM_RSE_Default_TT"),
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

                        fmgEntries = ParamFmgUtils.GetFmgEntriesByAssociatedParam(CurrentView.Editor, paramName, "Title");

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