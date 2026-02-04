using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class CellSearchEngine : SearchEngine<(string, Param.Row), (ParamEditorPseudoColumn, Param.Column)>
{
    public ParamEditorView CurrentView;

    public CellSearchEngine(ParamEditorView curView)
    {
        CurrentView = curView;
        Setup();
    }

    internal void Setup()
    {
        ParamMeta pMeta = null;

        unpacker = row =>
        {
            var metaDict = CurrentView.GetParamData().ParamMeta;
            pMeta = metaDict[row.Item2.Def];

            List<(ParamEditorPseudoColumn, Param.Column)> list = new();
            list.Add((ParamEditorPseudoColumn.ID, null));
            list.Add((ParamEditorPseudoColumn.Name, null));
            list.AddRange(row.Item2.Columns.Select((cell, i) => (ParamEditorPseudoColumn.None, cell)));
            return list;
        };
        defaultFilter = newCmd(new[] { "field internalName (regex)" },
            "Selects cells/fields where the internal name of that field matches the given regex", (args, lenient) =>
            {
                var matchID = args[0] == "ID";
                var matchName = args[0] == "Name";
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(cell =>
                {
                    if (matchID && cell.Item1 == ParamEditorPseudoColumn.ID)
                    {
                        return true;
                    }

                    if (matchName && cell.Item1 == ParamEditorPseudoColumn.Name)
                    {
                        return true;
                    }

                    if (cell.Item2 != null)
                    {
                        var meta = lenient ? pMeta.GetField(cell.Item2.Def) : null;
                        if (lenient && meta?.AltName != null && rx.IsMatch(meta?.AltName))
                        {
                            return true;
                        }

                        if (rx.IsMatch(cell.Item2.Def.InternalName))
                        {
                            return true;
                        }
                    }

                    return false;
                });
            });
        filterList.Add("field_value", newCmd(new[] { "field value" },
            "Selects cells/fields where the cell has the specified value",
            (args, lenient) =>
            {
                var startValue = args[0];

                ParamBank bank = CurrentView.GetPrimaryBank();

                return row =>
                {
                    if (row.Item1 == null)
                    {
                        throw new Exception("Can't check if cell - not part of a param");
                    }

                    Param curParam = bank.Params?[row.Item1];
                    if (curParam == null)
                    {
                        throw new Exception("Can't check if cell - no param");
                    }

                    Param.Row r = curParam[row.Item2.ID];

                    if (r == null)
                    {
                        return col => false;
                    }

                    return col =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) curCol = col.GetAs(curParam);
                        var curValue = r.Get(curCol);

                        if (curCol.Item2 == null)
                            return false;

                        if (IsValueMatch($"{curValue}", $"{startValue}", $"{startValue}", curCol.Item2.Def.InternalType))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    };
                };
            }));
        filterList.Add("field_range", newCmd(new[] { "field start value", "field end value" },
            "Selects cells/fields where the cell has a value between the start and end values specified",
            (args, lenient) =>
            {
                var startValue = args[0];
                var endValue = args[1];

                ParamBank bank = CurrentView.GetPrimaryBank();

                return row =>
                {
                    if (row.Item1 == null)
                    {
                        throw new Exception("Can't check if cell - not part of a param");
                    }

                    Param curParam = bank.Params?[row.Item1];
                    if (curParam == null)
                    {
                        throw new Exception("Can't check if cell - no param");
                    }

                    Param.Row r = curParam[row.Item2.ID];

                    if (r == null)
                    {
                        return col => false;
                    }

                    return col =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) curCol = col.GetAs(curParam);
                        var curValue = r.Get(curCol);

                        if (curCol.Item2 == null)
                            return false;

                        if (IsValueMatch($"{curValue}", $"{startValue}", $"{endValue}", curCol.Item2.Def.InternalType))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    };
                };
            }));
        filterList.Add("modified", newCmd(new string[0],
            "Selects cells/fields where the equivalent cell in the vanilla regulation or parambnd has a different value",
            (args, lenient) => row =>
            {
                if (row.Item1 == null)
                {
                    throw new Exception("Can't check if cell is modified - not part of a param");
                }

                var vBank = CurrentView.GetVanillaBank();

                Param vParam = vBank.Params?[row.Item1];
                if (vParam == null)
                {
                    throw new Exception("Can't check if cell is modified - no vanilla param");
                }

                Param.Row r = vParam[row.Item2.ID];
                if (r == null)
                {
                    return col => true;
                }

                return col =>
                {
                    (ParamEditorPseudoColumn, Param.Column) vcol = col.GetAs(vParam);
                    var valA = row.Item2.Get(col);
                    var valB = r.Get(vcol);
                    return ParamUtils.IsValueDiff(ref valA, ref valB, col.GetColumnType());
                };
            }));
        filterList.Add("auxmodified", newCmd(new[] { "parambank name" },
            "Selects cells/fields where the equivalent cell in the specified regulation or parambnd has a different value",
            (args, lenient) =>
            {
                var vBank = CurrentView.GetVanillaBank();
                var auxBank = CurrentView.Project.Handler.ParamData.AuxBanks;

                if (!auxBank.ContainsKey(args[0]))
                {
                    throw new Exception("Can't check if cell is modified - parambank not found");
                }

                ParamBank bank = auxBank[args[0]];
                return row =>
                {
                    if (row.Item1 == null)
                    {
                        throw new Exception("Can't check if cell is modified - not part of a param");
                    }

                    Param auxParam = bank.Params?[row.Item1];
                    if (auxParam == null)
                    {
                        throw new Exception("Can't check if cell is modified - no aux param");
                    }

                    Param vParam = vBank.Params?[row.Item1];
                    if (vParam == null)
                    {
                        throw new Exception("Can't check if cell is modified - no vanilla param");
                    }

                    Param.Row r = auxParam[row.Item2.ID];
                    Param.Row r2 = vParam[row.Item2.ID];
                    if (r == null)
                    {
                        return col => false;
                    }

                    if (r2 == null)
                    {
                        return col => true;
                    }

                    return col =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) auxcol = col.GetAs(auxParam);
                        (ParamEditorPseudoColumn, Param.Column) vcol = col.GetAs(vParam);
                        var valA = r.Get(auxcol);
                        var valB = r2.Get(vcol);
                        return ParamUtils.IsValueDiff(ref valA, ref valB, col.GetColumnType());
                    };
                };
            }, () => CurrentView.Project.Handler.ParamData.AuxBanks.Count > 0));

        filterList.Add("sftype", newCmd(new[] { "paramdef type" },
            "Selects cells/fields where the field's data type, as enumerated by soulsformats, matches the given regex",
            (args, lenient) =>
            {
                Regex r = new('^' + args[0] + '$',
                    lenient ? RegexOptions.IgnoreCase : RegexOptions.None); //Leniency rules break from the norm
                return row => col => r.IsMatch(col.GetColumnSfType());
            }));

        filterList.Add("default_value", newCmd(new string[0],
            "Selects cells/fields where the cell value is the same as the 'default' for the field.",
            (args, lenient) =>
            {
                ParamBank bank = CurrentView.GetPrimaryBank();

                return row =>
                {
                    if (row.Item1 == null)
                    {
                        throw new Exception("Can't check if cell - not part of a param");
                    }

                    Param curParam = bank.Params?[row.Item1];
                    if (curParam == null)
                    {
                        throw new Exception("Can't check if cell - no param");
                    }

                    Param.Row r = curParam[row.Item2.ID];

                    if (r == null)
                    {
                        return col => false;
                    }

                    return col =>
                    {
                        (ParamEditorPseudoColumn, Param.Column) curCol = col.GetAs(curParam);
                        var curValue = $"{r.Get(curCol)}" as object;

                        if (curCol.Item2 == null)
                            return false;


                        var meta = lenient ? pMeta.GetField(curCol.Item2.Def) : null;
                        if (meta.DefaultValue != null)
                        {
                            var defaultValue = meta.DefaultValue as object;

                            if (!ParamUtils.IsValueDiff(ref curValue, ref defaultValue, col.GetColumnType()))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // Default to true if DefaultValue isn't present
                            return true;
                        }
                    };
                };
            }));
    }

    private bool IsValueMatch(string fieldValue, string startValue, string endValue, string internalType)
    {
        var fieldSuccess = false;
        var startSuccess = false;
        var endSuccess = false;

        switch (internalType)
        {
            case "s8":
                sbyte sbyteFieldVal;
                sbyte sbyteStartVal;
                sbyte sbyteEndVal;

                fieldSuccess = sbyte.TryParse(fieldValue, out sbyteFieldVal);
                startSuccess = sbyte.TryParse(startValue, out sbyteStartVal);
                endSuccess = sbyte.TryParse(endValue, out sbyteEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((sbyteFieldVal >= sbyteStartVal) &&
                        (sbyteFieldVal <= sbyteEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "u8":
                byte byteFieldVal;
                byte byteStartVal;
                byte byteEndVal;

                fieldSuccess = byte.TryParse(fieldValue, out byteFieldVal);
                startSuccess = byte.TryParse(startValue, out byteStartVal);
                endSuccess = byte.TryParse(endValue, out byteEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((byteFieldVal >= byteStartVal) &&
                        (byteFieldVal <= byteEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "s16":
                short shortFieldVal;
                short shortStartVal;
                short shortEndVal;

                fieldSuccess = short.TryParse(fieldValue, out shortFieldVal);
                startSuccess = short.TryParse(startValue, out shortStartVal);
                endSuccess = short.TryParse(endValue, out shortEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((shortFieldVal >= shortStartVal) &&
                        (shortFieldVal <= shortEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "u16":
                ushort ushortFieldVal;
                ushort ushortStartVal;
                ushort ushortEndVal;

                fieldSuccess = ushort.TryParse(fieldValue, out ushortFieldVal);
                startSuccess = ushort.TryParse(startValue, out ushortStartVal);
                endSuccess = ushort.TryParse(endValue, out ushortEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((ushortFieldVal >= ushortStartVal) &&
                        (ushortFieldVal <= ushortEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "s32":
                int intFieldVal;
                int intStartVal;
                int intEndVal;

                fieldSuccess = int.TryParse(fieldValue, out intFieldVal);
                startSuccess = int.TryParse(startValue, out intStartVal);
                endSuccess = int.TryParse(endValue, out intEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((intFieldVal >= intStartVal) &&
                        (intFieldVal <= intEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "u32":
                uint uintFieldVal;
                uint uintStartVal;
                uint uintEndVal;

                fieldSuccess = uint.TryParse(fieldValue, out uintFieldVal);
                startSuccess = uint.TryParse(startValue, out uintStartVal);
                endSuccess = uint.TryParse(endValue, out uintEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((uintFieldVal >= uintStartVal) &&
                        (uintFieldVal <= uintEndVal))
                    {
                        return true;
                    }
                }
                break;
            case "f32":
                double floatFieldVal;
                double floatStartVal;
                double floatEndVal;

                fieldSuccess = double.TryParse(fieldValue, out floatFieldVal);
                startSuccess = double.TryParse(startValue, out floatStartVal);
                endSuccess = double.TryParse(endValue, out floatEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((floatFieldVal >= floatStartVal) &&
                        (floatFieldVal <= floatEndVal))
                    {
                        return true;
                    }
                }
                break;
            default: break;
        }

        return false;
    }
}