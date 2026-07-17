using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MEOperationArgument
{
    public ParamEditorView CurrentView;

    public MEOperationArgument(ParamEditorView curView)
    {
        CurrentView = curView;
        argumentGetters.Clear();
        Setup();
    }

    private readonly Dictionary<string, OperationArgumentGetter> argumentGetters = new();
    private OperationArgumentGetter defaultGetter;

    private OperationArgumentGetter newGetter(string[] args, string wiki,
        Func<string[], Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>>
            func, Func<bool> shouldShow = null)
    {
        return new OperationArgumentGetter(args, wiki, func, shouldShow);
    }

    private void Setup()
    {
        // <.>
        defaultGetter = newGetter(new string[0], 
            LOC.Get("PARAM_OpArg_Default_TT"),
            value => (i, param) => (j, row) => (k, col) => value[0]);

        // self
        argumentGetters.Add("self", newGetter(new string[0], 
            LOC.Get("PARAM_OpArg_Self_TT"),
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return row.Get(col).ToParamEditorString();
            }));

        // field <field name>
        argumentGetters.Add("field", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Field_Hint")},
            LOC.Get("PARAM_OpArg_Field_TT"), 
            args =>
                (i, param) =>
                {
                    var fieldName = args[0];

                    (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(fieldName);
                    if (!col.IsColumnValid())
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_Field_Error_Locate_Field", fieldName.ToString()));
                    }

                    return (j, row) =>
                    {
                        var v = row.Get(col).ToParamEditorString();
                        return (k, c) => v;
                    };
                }));

        // vanilla
        argumentGetters.Add("vanilla", newGetter(new string[0],
            LOC.Get("PARAM_OpArg_Vanilla_TT"),
            empty =>
            {
                var vBank = CurrentView.GetVanillaBank();
                var pBank = CurrentView.GetPrimaryBank();

                return (i, param) =>
                {

                    var paramName = pBank.GetKeyForParam(param);
                    if (!vBank.Params.ContainsKey(paramName))
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_Vanilla_Error_Locate_Vanilla_Param", param.ParamType));
                    }

                    Param vParam = vBank.Params[paramName];
                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];
                        if (vRow == null)
                        {
                            throw new Exception(
                                LOC.Get("PARAM_OpArg_Vanilla_Error_Locate_Vanilla_Row", row.ID.ToString()));
                        }

                        return (k, col) =>
                        {
                            if (col.Item1 == ParamEditorPseudoColumn.None && col.Item2 == null)
                            {
                                throw new Exception(
                                    LOC.Get("PARAM_OpArg_Vanilla_Error_Locate_Property"));
                            }

                            return vRow.Get(col).ToParamEditorString();
                        };
                    };
                };
            }));

        // aux <parambank name>
        argumentGetters.Add("aux", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Aux_Hint")},
            LOC.Get("PARAM_OpArg_Aux_TT"),
            args =>
            {
                var pBank = CurrentView.GetPrimaryBank();
                var auxBanks = CurrentView.Editor.Project.Handler.ParamData.AuxBanks;

                var parambankName = args[0];

                if (!auxBanks.ContainsKey(parambankName))
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Aux_Error_Locate_Param_Bank", parambankName.ToString()));
                }

                ParamBank bank = auxBanks[parambankName];
                return (i, param) =>
                {
                    var paramName = pBank.GetKeyForParam(param);
                    if (!bank.Params.ContainsKey(paramName))
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_Aux_Error_Locate_Aux_Param", param.ParamType));
                    }

                    Param vParam = bank.Params[paramName];
                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];
                        if (vRow == null)
                        {
                            throw new Exception(
                                LOC.Get("PARAM_OpArg_Aux_Error_Locate_Aux_Row", row.ID.ToString()));
                        }

                        return (k, col) =>
                        {
                            if (!col.IsColumnValid())
                            {
                                throw new Exception(
                                    LOC.Get("PARAM_OpArg_Aux_Error_Locate_Property"));
                            }

                            return vRow.Get(col).ToParamEditorString();
                        };
                    };
                };
            }, () => CurrentView.Editor.Project.Handler.ParamData.AuxBanks.Count > 0));

        // vanillafield <field name>
        argumentGetters.Add("vanillafield", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_VanillaField_Hint")},
            LOC.Get("PARAM_OpArg_VanillaField_TT"),
            args => (i, param) =>
            {
                var vBank = CurrentView.GetVanillaBank();
                var pBank = CurrentView.GetPrimaryBank();

                var fieldName = args[0];

                var paramName = pBank.GetKeyForParam(param);
                Param vParam = vBank.GetParamFromName(paramName);
                if (vParam == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_VanillaField_Error_Locate_Vanilla_Param", param.ParamType));
                }

                (ParamEditorPseudoColumn, Param.Column) col = vParam.GetCol(fieldName);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_VanillaField_Error_Locate_Field", fieldName.ToString()));
                }

                return (j, row) =>
                {
                    Param.Row vRow = vParam?[row.ID];
                    if (vRow == null)
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_VanillaField_Error_Locate_Vanilla_Row", row.ID.ToString()));
                    }

                    var v = vRow.Get(col).ToParamEditorString();
                    return (k, c) => v;
                };
            }));

        // auxfield <parambank name> <field name>
        argumentGetters.Add("auxfield", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_AuxField_Hint_1"),
            LOC.Get("PARAM_OpArg_AuxField_Hint_2")},
            LOC.Get("PARAM_OpArg_AuxField_TT"),
            args =>
            {
                var pBank = CurrentView.GetPrimaryBank();
                var auxBanks = CurrentView.Editor.Project.Handler.ParamData.AuxBanks;

                var parambankName = args[0];
                var fieldName = args[1];

                if (!auxBanks.ContainsKey(parambankName))
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_AuxField_Error_Locate_Param_Bank", parambankName.ToString()));
                }

                ParamBank bank = auxBanks[parambankName];
                return (i, param) =>
                {
                    var paramName = pBank.GetKeyForParam(param);
                    if (!bank.Params.ContainsKey(paramName))
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_AuxField_Error_Locate_Aux_Param", param.ParamType));
                    }

                    Param vParam = bank.Params[paramName];
                    (ParamEditorPseudoColumn, Param.Column) col = vParam.GetCol(fieldName);

                    if (!col.IsColumnValid())
                    {
                        throw new Exception(
                            LOC.Get("PARAM_OpArg_AuxField_Error_Locate_Field", fieldName.ToString()));
                    }

                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];

                        if (vRow == null)
                        {
                            throw new Exception(
                                LOC.Get("PARAM_OpArg_AuxField_Error_Locate_Aux_Row", row.ID.ToString()));
                        }

                        var v = vRow.Get(col).ToParamEditorString();
                        return (k, c) => v;
                    };
                };
            }, () => CurrentView.Editor.Project.Handler.ParamData.AuxBanks.Count > 0));

        // paramlookup <param name> <row id> <field name>
        argumentGetters.Add("paramlookup", newGetter(new[] {
            LOC.Get("PARAM_OpArg_ParamLookup_Hint_1"),
            LOC.Get("PARAM_OpArg_ParamLookup_Hint_2"),
            LOC.Get("PARAM_OpArg_ParamLookup_Hint_3")},
            LOC.Get("PARAM_OpArg_ParamLookup_TT"), 
            args =>
            {
                var pBank = CurrentView.GetPrimaryBank();
                var paramName = args[0];
                var rowId = args[1];
                var fieldName = args[2];

                Param param = pBank.Params[paramName];

                if (param == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_ParamLookup_Error_Locate_Param", paramName));
                }

                var id = int.Parse(rowId);
                (ParamEditorPseudoColumn, Param.Column) field = param.GetCol(fieldName);

                if (!field.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_ParamLookup_Error_Locate_Field", fieldName, paramName));
                }

                var row = param[id];

                if (row == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_ParamLookup_Error_Locate_Row", id, paramName));
                }

                var value = row.Get(field).ToParamEditorString();
                return (i, param) => (j, row) => (k, col) => value;
            }));

        // average <field name> <row selector>
        argumentGetters.Add("average", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Average_Hint_1"),
            LOC.Get("PARAM_OpArg_Average_Hint_2")},
            LOC.Get("PARAM_OpArg_Average_TT"),
            args => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                var fieldName = args[0];
                var rowSelector = args[1];

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(fieldName);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Average_Error_Locate_Field", fieldName));
                }

                Type colType = col.GetColumnType();
                if (colType == typeof(string) || colType == typeof(byte[]))
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Average_Error_Invalid_Type", fieldName));
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), rowSelector, false, false);

                IEnumerable<object> vals = rows.Select((row, i) => row.Get(col));

                var avg = vals.Average(val => Convert.ToDouble(val));

                return (j, row) => (k, c) => avg.ToString();

            }));

        // median <field name> <row selector>
        argumentGetters.Add("median", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Median_Hint_1"),
            LOC.Get("PARAM_OpArg_Median_Hint_2")},
            LOC.Get("PARAM_OpArg_Median_TT"),
            args => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                var fieldName = args[0];
                var rowSelector = args[1];

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(fieldName);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Median_Error_Locate_Field", fieldName));
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), rowSelector, false, false);

                IEnumerable<object> vals = rows.Select((row, i) => row.Get(col));

                var avg = vals.OrderBy(val => Convert.ToDouble(val)).ElementAt(vals.Count() / 2);

                return (j, row) => (k, c) => avg.ToParamEditorString();
            }));

        // mode <field name> <row selector>
        argumentGetters.Add("mode", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Mode_Hint_1"),
            LOC.Get("PARAM_OpArg_Mode_Hint_2")},
            LOC.Get("PARAM_OpArg_Mode_TT"),
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Mode_Error_Locate_Field", field[0]));
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var avg = ParamUtils.GetParamValueDistribution(rows, col).OrderByDescending(g => g.Item2)
                    .First().Item1;

                return (j, row) => (k, c) => avg.ToParamEditorString();
            }));

        argumentGetters.Add("min", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Min_Hint_1"),
            LOC.Get("PARAM_OpArg_Min_Hint_2")},
            LOC.Get("PARAM_OpArg_Min_TT"),
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Min_Error_Locate_Field", field[0]));
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var min = rows.Min(r => r[field[0]].Value.Value);

                return (j, row) => (k, c) => min.ToParamEditorString();
            }));

        argumentGetters.Add("max", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Max_Hint_1"),
            LOC.Get("PARAM_OpArg_Max_Hint_2")},
            LOC.Get("PARAM_OpArg_Max_TT"),
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Max_Error_Locate_Field", field[0]));
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var max = rows.Max(r => r[field[0]].Value.Value);

                return (j, row) => (k, c) => max.ToParamEditorString();
            }));

        argumentGetters.Add("random", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_Random_Hint_1"),
            LOC.Get("PARAM_OpArg_Random_Hint_2")},
            LOC.Get("PARAM_OpArg_Random_Hint_TT"), minAndMax =>
            {
                double min;
                double max;
                if (!double.TryParse(minAndMax[0], out min) || !double.TryParse(minAndMax[1], out max))
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Random_Error_Parse_Min_Max_Values"));
                }

                if (max <= min)
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_Random_Error_Max_Higher_Than_Min"));
                }

                var range = max - min;
                return (i, param) => (j, row) => (k, c) => ((Random.Shared.NextDouble() * range) + min).ToString();
            }));

        argumentGetters.Add("randint", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_RandInt_Hint_1"),
            LOC.Get("PARAM_OpArg_RandInt_Hint_2")},
            LOC.Get("PARAM_OpArg_RandInt_TT"), minAndMax =>
            {
                int min;
                int max;
                if (!int.TryParse(minAndMax[0], out min) || !int.TryParse(minAndMax[1], out max))
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_RandInt_Error_Parse_Min_Max_Values"));
                }

                if (max <= min)
                {
                    throw new Exception(
                        LOC.Get("PARAM_OpArg_RandInt_Error_Max_Higher_Than_Min"));
                }

                return (i, param) => (j, row) => (k, c) => Random.Shared.NextInt64(min, max + 1).ToString();
            }));

        argumentGetters.Add("randFrom", newGetter(new[] { 
            LOC.Get("PARAM_OpArg_RandFrom_Hint_1"),
            LOC.Get("PARAM_OpArg_RandFrom_Hint_2"),
            LOC.Get("PARAM_OpArg_RandFrom_Hint_3")},
            LOC.Get("PARAM_OpArg_RandFrom_TT"),
            paramFieldRowSelector =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                Param srcParam = pBank.Params[paramFieldRowSelector[0]];
                List<Param.Row> srcRows = CurrentView.MassEdit.RSE.Search((pBank, srcParam),
                    paramFieldRowSelector[2], false, false);
                var values = srcRows.Select((r, i) => r[paramFieldRowSelector[1]].Value.Value).ToArray();
                return (i, param) =>
                    (j, row) => (k, c) => values[Random.Shared.NextInt64(values.Length)].ToString();
            }));

        argumentGetters.Add("paramIndex", newGetter(new string[0],
            LOC.Get("PARAM_OpArg_ParamIndex_TT"),
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return i.ToParamEditorString();
            }));

        argumentGetters.Add("rowIndex", newGetter(new string[0],
            LOC.Get("PARAM_OpArg_RowIndex_TT"),
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return j.ToParamEditorString();
            }));

        argumentGetters.Add("fieldIndex", newGetter(new string[0],
            LOC.Get("PARAM_OpArg_FieldIndex_TT"),
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return k.ToParamEditorString();
            }));
    }

    public List<(string, string[])> AllArguments()
    {
        List<(string, string[])> options = new();
        foreach (var op in argumentGetters.Keys)
        {
            options.Add((op, argumentGetters[op].args));
        }

        return options;
    }

    public List<(string, string, string[])> VisibleArguments()
    {
        List<(string, string, string[])> options = new();
        foreach (var op in argumentGetters.Keys)
        {
            OperationArgumentGetter oag = argumentGetters[op];
            if (oag.shouldShow == null || oag.shouldShow())
            {
                options.Add((op, oag.wiki, oag.args));
            }
        }

        return options;
    }

    internal Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>[]
        getContextualArguments(int argumentCount, string opData)
    {
        var opArgs = opData == null ? new string[0] : opData.Split(':', argumentCount);
        var contextualArgs =
            new Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>[opArgs
                .Length];
        for (var i = 0; i < opArgs.Length; i++)
        {
            contextualArgs[i] = getContextualArgument(opArgs[i]);
        }

        return contextualArgs;
    }

    internal Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>
        getContextualArgument(string opArg)
    {
        if (opArg.StartsWith('"') && opArg.EndsWith('"'))
        {
            return (i, p) => (j, r) => (k, c) => opArg.Substring(1, opArg.Length - 2);
        }

        if (opArg.StartsWith('$'))
        {
            opArg = MassParamEdit.massEditVars[opArg.Substring(1)].ToString();
        }

        var arg = opArg.Split(" ", 2);
        if (argumentGetters.ContainsKey(arg[0].Trim()))
        {
            OperationArgumentGetter getter = argumentGetters[arg[0]];
            var opArgArgs = arg.Length > 1 ? arg[1].Split(" ", getter.args.Length) : new string[0];
            if (opArgArgs.Length != getter.args.Length)
            {
                throw new Exception(
                    LOC.Get("PARAM_OpArg_ContextualArg_Wrong_Arg_Count", arg[0], opArgArgs.Length));
            }

            for (var i = 0; i < opArgArgs.Length; i++)
            {
                if (opArgArgs[i].StartsWith('$'))
                {
                    opArgArgs[i] = MassParamEdit.massEditVars[opArgArgs[i].Substring(1)].ToString();
                }
            }

            return getter.func(opArgArgs);
        }

        return defaultGetter.func(new[] { opArg });
    }
}
