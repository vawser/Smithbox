using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MEOperationArgument
{
    public ParamView CurrentView;

    public MEOperationArgument(ParamView curView)
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
        defaultGetter = newGetter(new string[0], "Gives the specified value",
            value => (i, param) => (j, row) => (k, col) => value[0]);

        argumentGetters.Add("self", newGetter(new string[0], "Gives the value of the currently selected value",
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return row.Get(col).ToParamEditorString();
            }));

        argumentGetters.Add("field", newGetter(new[] { "field internalName" },
            "Gives the value of the given cell/field for the currently selected row and param", field =>
                (i, param) =>
                {
                    (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                    if (!col.IsColumnValid())
                    {
                        throw new Exception($@"Could not locate field {field[0]}");
                    }

                    return (j, row) =>
                    {
                        var v = row.Get(col).ToParamEditorString();
                        return (k, c) => v;
                    };
                }));

        argumentGetters.Add("vanilla", newGetter(new string[0],
            "Gives the value of the equivalent cell/field in the vanilla regulation or parambnd for the currently selected cell/field, row and param.\nWill fail if a row does not have a vanilla equivilent. Consider using && !added",
            empty =>
            {
                var vBank = CurrentView.GetVanillaBank();
                var pBank = CurrentView.GetPrimaryBank();

                return (i, param) =>
                {

                    var paramName = pBank.GetKeyForParam(param);
                    if (!vBank.Params.ContainsKey(paramName))
                    {
                        throw new Exception($@"Could not locate vanilla param for {param.ParamType}");
                    }

                    Param vParam = vBank.Params[paramName];
                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];
                        if (vRow == null)
                        {
                            throw new Exception($@"Could not locate vanilla row {row.ID}");
                        }

                        return (k, col) =>
                        {
                            if (col.Item1 == ParamEditorPseudoColumn.None && col.Item2 == null)
                            {
                                throw new Exception(@"Could not locate given field or property");
                            }

                            return vRow.Get(col).ToParamEditorString();
                        };
                    };
                };
            }));

        argumentGetters.Add("aux", newGetter(new[] { "parambank name" },
            "Gives the value of the equivalent cell/field in the specified regulation or parambnd for the currently selected cell/field, row and param.\nWill fail if a row does not have an aux equivilent. Consider using && auxprop ID .*",
            bankName =>
            {
                var pBank = CurrentView.GetPrimaryBank();
                var auxBanks = CurrentView.Editor.Project.Handler.ParamData.AuxBanks;

                if (!auxBanks.ContainsKey(bankName[0]))
                {
                    throw new Exception($@"Could not locate paramBank {bankName[0]}");
                }

                ParamBank bank = auxBanks[bankName[0]];
                return (i, param) =>
                {
                    var paramName = pBank.GetKeyForParam(param);
                    if (!bank.Params.ContainsKey(paramName))
                    {
                        throw new Exception($@"Could not locate aux param for {param.ParamType}");
                    }

                    Param vParam = bank.Params[paramName];
                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];
                        if (vRow == null)
                        {
                            throw new Exception($@"Could not locate aux row {row.ID}");
                        }

                        return (k, col) =>
                        {
                            if (!col.IsColumnValid())
                            {
                                throw new Exception(@"Could not locate given field or property");
                            }

                            return vRow.Get(col).ToParamEditorString();
                        };
                    };
                };
            }, () => CurrentView.Editor.Project.Handler.ParamData.AuxBanks.Count > 0));

        argumentGetters.Add("vanillafield", newGetter(new[] { "field internalName" },
            "Gives the value of the specified cell/field in the vanilla regulation or parambnd for the currently selected row and param.\nWill fail if a row does not have a vanilla equivilent. Consider using && !added",
            field => (i, param) =>
            {
                var vBank = CurrentView.GetVanillaBank();
                var pBank = CurrentView.GetPrimaryBank();

                var paramName = pBank.GetKeyForParam(param);
                Param vParam = vBank.GetParamFromName(paramName);
                if (vParam == null)
                {
                    throw new Exception($@"Could not locate vanilla param for {param.ParamType}");
                }

                (ParamEditorPseudoColumn, Param.Column) col = vParam.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                return (j, row) =>
                {
                    Param.Row vRow = vParam?[row.ID];
                    if (vRow == null)
                    {
                        throw new Exception($@"Could not locate vanilla row {row.ID}");
                    }

                    var v = vRow.Get(col).ToParamEditorString();
                    return (k, c) => v;
                };
            }));

        argumentGetters.Add("auxfield", newGetter(new[] { "parambank name", "field internalName" },
            "Gives the value of the specified cell/field in the specified regulation or parambnd for the currently selected row and param.\nWill fail if a row does not have an aux equivilent. Consider using && auxprop ID .*",
            bankAndField =>
            {
                var pBank = CurrentView.GetPrimaryBank();
                var auxBanks = CurrentView.Editor.Project.Handler.ParamData.AuxBanks;

                if (!auxBanks.ContainsKey(bankAndField[0]))
                {
                    throw new Exception($@"Could not locate paramBank {bankAndField[0]}");
                }

                ParamBank bank = auxBanks[bankAndField[0]];
                return (i, param) =>
                {
                    var paramName = pBank.GetKeyForParam(param);
                    if (!bank.Params.ContainsKey(paramName))
                    {
                        throw new Exception($@"Could not locate aux param for {param.ParamType}");
                    }

                    Param vParam = bank.Params[paramName];
                    (ParamEditorPseudoColumn, Param.Column) col = vParam.GetCol(bankAndField[1]);
                    if (!col.IsColumnValid())
                    {
                        throw new Exception($@"Could not locate field {bankAndField[1]}");
                    }

                    return (j, row) =>
                    {
                        Param.Row vRow = vParam?[row.ID];
                        if (vRow == null)
                        {
                            throw new Exception($@"Could not locate aux row {row.ID}");
                        }

                        var v = vRow.Get(col).ToParamEditorString();
                        return (k, c) => v;
                    };
                };
            }, () => CurrentView.Editor.Project.Handler.ParamData.AuxBanks.Count > 0));

        argumentGetters.Add("paramlookup", newGetter(new[] { "param name", "row id", "field name" },
            "Returns the specific value specified by the exact param, row and field.", address =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                Param param = pBank.Params[address[0]];
                if (param == null)
                    throw new Exception($@"Could not find param {address[0]}");
                var id = int.Parse(address[1]);
                (ParamEditorPseudoColumn, Param.Column) field = param.GetCol(address[2]);
                if (!field.IsColumnValid())
                    throw new Exception($@"Could not find field {address[2]} in param {address[0]}");
                var row = param[id];
                if (row == null)
                    throw new Exception($@"Could not find row {id} in param {address[0]}");
                var value = row.Get(field).ToParamEditorString();
                return (i, param) => (j, row) => (k, col) => value;
            }));

        argumentGetters.Add("average", newGetter(new[] { "field internalName", "row selector" },
            "Gives the mean value of the cells/fields found using the given selector, for the currently selected param",
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                Type colType = col.GetColumnType();
                if (colType == typeof(string) || colType == typeof(byte[]))
                {
                    throw new Exception($@"Cannot average field {field[0]}");
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                IEnumerable<object> vals = rows.Select((row, i) => row.Get(col));

                var avg = vals.Average(val => Convert.ToDouble(val));

                return (j, row) => (k, c) => avg.ToString();

            }));

        argumentGetters.Add("median", newGetter(new[] { "field internalName", "row selector" },
            "Gives the median value of the cells/fields found using the given selector, for the currently selected param",
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                IEnumerable<object> vals = rows.Select((row, i) => row.Get(col));

                var avg = vals.OrderBy(val => Convert.ToDouble(val)).ElementAt(vals.Count() / 2);

                return (j, row) => (k, c) => avg.ToParamEditorString();
            }));

        argumentGetters.Add("mode", newGetter(new[] { "field internalName", "row selector" },
            "Gives the most common value of the cells/fields found using the given selector, for the currently selected param",
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var avg = ParamUtils.GetParamValueDistribution(rows, col).OrderByDescending(g => g.Item2)
                    .First().Item1;

                return (j, row) => (k, c) => avg.ToParamEditorString();
            }));

        argumentGetters.Add("min", newGetter(new[] { "field internalName", "row selector" },
            "Gives the smallest value from the cells/fields found using the given param, row selector and field",
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var min = rows.Min(r => r[field[0]].Value.Value);

                return (j, row) => (k, c) => min.ToParamEditorString();
            }));

        argumentGetters.Add("max", newGetter(new[] { "field internalName", "row selector" },
            "Gives the largest value from the cells/fields found using the given param, row selector and field",
            field => (i, param) =>
            {
                var pBank = CurrentView.GetPrimaryBank();

                (ParamEditorPseudoColumn, Param.Column) col = param.GetCol(field[0]);
                if (!col.IsColumnValid())
                {
                    throw new Exception($@"Could not locate field {field[0]}");
                }

                List<Param.Row> rows = CurrentView.MassEdit.RSE.Search((pBank, param), field[1], false, false);

                var max = rows.Max(r => r[field[0]].Value.Value);

                return (j, row) => (k, c) => max.ToParamEditorString();
            }));

        argumentGetters.Add("random", newGetter(
            new[] { "minimum number (inclusive)", "maximum number (exclusive)" },
            "Gives a random decimal number between the given values for each selected value", minAndMax =>
            {
                double min;
                double max;
                if (!double.TryParse(minAndMax[0], out min) || !double.TryParse(minAndMax[1], out max))
                {
                    throw new Exception(@"Could not parse min and max random values");
                }

                if (max <= min)
                {
                    throw new Exception(@"Random max must be greater than min");
                }

                var range = max - min;
                return (i, param) => (j, row) => (k, c) => ((Random.Shared.NextDouble() * range) + min).ToString();
            }));

        argumentGetters.Add("randint", newGetter(
            new[] { "minimum integer (inclusive)", "maximum integer (inclusive)" },
            "Gives a random integer between the given values for each selected value", minAndMax =>
            {
                int min;
                int max;
                if (!int.TryParse(minAndMax[0], out min) || !int.TryParse(minAndMax[1], out max))
                {
                    throw new Exception(@"Could not parse min and max randint values");
                }

                if (max <= min)
                {
                    throw new Exception(@"Random max must be greater than min");
                }

                return (i, param) => (j, row) => (k, c) => Random.Shared.NextInt64(min, max + 1).ToString();
            }));

        argumentGetters.Add("randFrom", newGetter(new[] { "param name", "field internalName", "row selector" },
            "Gives a random value from the cells/fields found using the given param, row selector and field, for each selected value",
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
            "Gives an integer for the current selected param, beginning at 0 and increasing by 1 for each param selected",
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return i.ToParamEditorString();
            }));

        argumentGetters.Add("rowIndex", newGetter(new string[0],
            "Gives an integer for the current selected row, beginning at 0 and increasing by 1 for each row selected",
            empty => (i, param) => (j, row) => (k, col) =>
            {
                return j.ToParamEditorString();
            }));

        argumentGetters.Add("fieldIndex", newGetter(new string[0],
            "Gives an integer for the current selected cell/field, beginning at 0 and increasing by 1 for each cell/field selected",
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
                    @$"Contextual value {arg[0]} has wrong number of arguments. Expected {opArgArgs.Length}");
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
