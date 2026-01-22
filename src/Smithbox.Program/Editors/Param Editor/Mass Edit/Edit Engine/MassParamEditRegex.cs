using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class MassParamEditRegex
{
    private int argc;

    // Run data        
    private string[] argNames;

    // Do we want these variables? shouldn't they be contained?
    private ParamBank bank;
    private string cellOperation;
    private string cellSelector;
    private ParamSelection context;
    private Func<object, string[], object> genericFunc;

    private Func<ParamSelection, string[], bool> globalFunc;

    private string globalOperation;
    private Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>[] paramArgFuncs;
    private string paramRowSelector;

    private string paramSelector;
    private Func<(string, Param.Row), string[], (Param, Param.Row)> rowFunc;
    private string rowOperation;


    private Func<int, Func<int, (ParamEditorPseudoColumn, Param.Column), string>[], string, Param.Row, List<EditorAction>,
        MassEditResult> rowOpOrCellStageFunc;

    private string rowSelector;
    private string varOperation;

    // Parsing data
    private string varSelector;

    private ParamView CurrentView;

    public MassParamEditRegex(ParamView curView)
    {
        CurrentView = curView;
    }

    public (MassEditResult, ActionManager child) PerformMassEdit(ParamBank bank, string commandsString,
        ParamSelection context)
    {
        int currentLine = 0;
        try
        {
            var commands = commandsString.Split(';');
            var changeCount = 0;
            ActionManager childManager = new();
            foreach (var cmd in commands)
            {
                currentLine++;
                var command = cmd;

                // Ignore comments
                if (command.StartsWith("##") || string.IsNullOrWhiteSpace(command))
                {
                    continue;
                }

                (MassEditResult result, List<EditorAction> actions) = (null, null);

                this.bank = bank;
                this.context = context;

                var primaryFilter = command.Split(':', 2)[0].Trim();

                if (CurrentView.MassEdit.GlobalOps.HandlesCommand(primaryFilter.Split(" ", 2)[0]))
                {
                    (result, actions) = ParseGlobalOpStep(currentLine, command);
                }
                else if (CurrentView.MassEdit.VSE.HandlesCommand(primaryFilter.Split(" ", 2)[0]))
                {
                    (result, actions) = ParseVarStep(currentLine, command);
                }
                else if (CurrentView.MassEdit.PARSE.HandlesCommand(primaryFilter.Split(" ", 2)[0]))
                {
                    (result, actions) = ParseParamRowStep(currentLine, command);
                }
                else
                {
                    (result, actions) = ParseParamStep(currentLine, command);
                }

                if (result.Type != ParamMassEditResultType.SUCCESS)
                {
                    return (result, null);
                }

                changeCount += actions.Count;
                childManager.ExecuteAction(new CompoundAction(actions));
            }

            return (new MassEditResult(ParamMassEditResultType.SUCCESS, $@"{changeCount} cells affected"), childManager);
        }
        catch (Exception e)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Unknown parsing error on line {currentLine}: " + e.ToString()), null);
        }
    }

    private (MassEditResult, List<EditorAction>) ParseGlobalOpStep(int currentLine, string restOfStages)
    {
        var opStage = restOfStages.Split(" ", 2);
        globalOperation = opStage[0].Trim();
        if (!CurrentView.MassEdit.GlobalOps.operations.ContainsKey(globalOperation))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Unknown global operation {globalOperation} (line {currentLine})"), null);
        }

        string wiki;
        (argNames, wiki, globalFunc) = CurrentView.MassEdit.GlobalOps.operations[globalOperation];
        ExecParamOperationArguments(currentLine, opStage.Length > 1 ? opStage[1] : null);
        if (argc != paramArgFuncs.Length)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Invalid number of arguments for operation {globalOperation} (line {currentLine})"), null);
        }

        return SandboxMassEditExecution(currentLine, partials => ExecGlobalOp(currentLine));
    }

    private (MassEditResult, List<EditorAction>) ParseVarStep(int currentLine, string restOfStages)
    {
        var varstage = restOfStages.Split(":", 2);
        varSelector = varstage[0].Trim();
        if (varSelector.Equals(""))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find variable filter. Add : and one of {String.Join(", ", CurrentView.MassEdit.VSE.AvailableCommandsForHelpText())} (line {currentLine})"), null);
        }

        if (varstage.Length < 2)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find var operation. Check your colon placement. (line {currentLine})"), null);
        }

        return ParseVarOpStep(currentLine, varstage[1]);
    }

    private (MassEditResult, List<EditorAction>) ParseVarOpStep(int currentLine, string restOfStages)
    {
        var operationstage = restOfStages.TrimStart().Split(" ", 2);
        varOperation = operationstage[0].Trim();
        if (varOperation.Equals("") || !CurrentView.MassEdit.FieldOps.operations.ContainsKey(varOperation))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find operation to perform. Add : and one of + - * / replace (line {currentLine})"), null);
        }

        string wiki;
        (argNames, wiki, genericFunc) = CurrentView.MassEdit.FieldOps.operations[varOperation];
        ExecParamOperationArguments(currentLine, operationstage.Length > 1 ? operationstage[1] : null);
        if (argc != paramArgFuncs.Length)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Invalid number of arguments for operation {varOperation} (line {currentLine})"), null);
        }

        return SandboxMassEditExecution(currentLine, partials => ExecVarStage(currentLine));
    }

    private (MassEditResult, List<EditorAction>) ParseParamRowStep(int currentLine, string restOfStages)
    {
        var paramrowstage = restOfStages.Split(":", 2);
        paramRowSelector = paramrowstage[0].Trim();
        if (paramRowSelector.Equals(""))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find paramrow filter. Add : and one of {String.Join(", ", CurrentView.MassEdit.PARSE.AvailableCommandsForHelpText())} (line {currentLine})"), null);
        }

        if (paramrowstage.Length < 2)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find cell filter or row operation. Check your colon placement. (line {currentLine})"), null);
        }

        if (CurrentView.MassEdit.RowOps.HandlesCommand(paramrowstage[1].Trim().Split(" ", 2)[0]))
        {
            return ParseRowOpStep(currentLine, paramrowstage[1]);
        }

        return ParseCellStep(currentLine, paramrowstage[1]);
    }

    private (MassEditResult, List<EditorAction>) ParseParamStep(int currentLine, string restOfStages)
    {
        var paramstage = restOfStages.Split(":", 2);
        paramSelector = paramstage[0].Trim();
        if (paramSelector.Equals(""))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find param filter. Add : and one of {String.Join(", ", CurrentView.MassEdit.PSE.AvailableCommandsForHelpText())} (line {currentLine})"), null);
        }

        if (paramstage.Length < 2)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find row filter. Check your colon placement. (line {currentLine})"), null);
        }

        return ParseRowStep(currentLine, paramstage[1]);
    }

    private (MassEditResult, List<EditorAction>) ParseRowStep(int currentLine, string restOfStages)
    {
        var rowstage = restOfStages.Split(":", 2);
        rowSelector = rowstage[0].Trim();
        if (rowSelector.Equals(""))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find row filter. Add : and one of {String.Join(", ", CurrentView.MassEdit.RSE.AvailableCommandsForHelpText())} (line {currentLine})"), null);
        }

        if (rowstage.Length < 2)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find cell filter or row operation to perform. Check your colon placement. (line {currentLine})"), null);
        }

        if (CurrentView.MassEdit.RowOps.HandlesCommand(rowstage[1].Trim().Split(" ", 2)[0]))
        {
            return ParseRowOpStep(currentLine, rowstage[1]);
        }

        return ParseCellStep(currentLine, rowstage[1]);
    }

    private (MassEditResult, List<EditorAction>) ParseCellStep(int currentLine, string restOfStages)
    {
        var cellstage = restOfStages.Split(":", 2);
        cellSelector = cellstage[0].Trim();
        if (cellSelector.Equals(""))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find cell/property filter. Add : and one of {String.Join(", ", CurrentView.MassEdit.CSE.AvailableCommandsForHelpText())} or Name (0 args) (line {currentLine})"), null);
        }

        if (cellstage.Length < 2)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find operation to perform. Check your colon placement. (line {currentLine})"), null);
        }

        rowOpOrCellStageFunc = ExecCellStage;
        return ParseCellOpStep(currentLine, cellstage[1]);
    }

    private (MassEditResult, List<EditorAction>) ParseRowOpStep(int currentLine, string restOfStages)
    {
        var operationstage = restOfStages.TrimStart().Split(" ", 2);
        rowOperation = operationstage[0].Trim();
        if (!CurrentView.MassEdit.RowOps.operations.ContainsKey(rowOperation))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Unknown row operation {rowOperation} (line {currentLine})"), null);
        }

        string wiki;
        (argNames, wiki, rowFunc) = CurrentView.MassEdit.RowOps.operations[rowOperation];
        ExecParamOperationArguments(currentLine, operationstage.Length > 1 ? operationstage[1] : null);
        if (argc != paramArgFuncs.Length)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Invalid number of arguments for operation {rowOperation} (line {currentLine})"), null);
        }

        rowOpOrCellStageFunc = ExecRowOp;
        return SandboxMassEditExecution(currentLine, partials =>
            paramRowSelector != null ? ExecParamRowStage(currentLine, partials) : ExecParamStage(currentLine, partials));
    }

    private (MassEditResult, List<EditorAction>) ParseCellOpStep(int currentLine, string restOfStages)
    {
        var operationstage = restOfStages.TrimStart().Split(" ", 2);
        cellOperation = operationstage[0].Trim();

        if (cellOperation.Equals("") || !CurrentView.MassEdit.FieldOps.operations.ContainsKey(cellOperation))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Could not find operation to perform. Add : and one of + - * / replace (line {currentLine})"), null);
        }

        if (!CurrentView.MassEdit.FieldOps.operations.ContainsKey(cellOperation))
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Unknown cell operation {cellOperation} (line {currentLine})"), null);
        }

        string wiki;
        (argNames, wiki, genericFunc) = CurrentView.MassEdit.FieldOps.operations[cellOperation];
        ExecParamOperationArguments(currentLine, operationstage.Length > 1 ? operationstage[1] : null);
        if (argc != paramArgFuncs.Length)
        {
            return (new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Invalid number of arguments for operation {cellOperation} (line {currentLine})"), null);
        }

        return SandboxMassEditExecution(currentLine, partials =>
            paramRowSelector != null ? ExecParamRowStage(currentLine, partials) : ExecParamStage(currentLine, partials));
    }

    private void ExecParamOperationArguments(int currentLine, string opargs)
    {
        argc = argNames.Length;
        paramArgFuncs = CurrentView.MassEdit.OperationArgs.getContextualArguments(argc, opargs);
    }

    private (MassEditResult, List<EditorAction>) SandboxMassEditExecution(int currentLine,
        Func<List<EditorAction>, MassEditResult> innerFunc)
    {
        List<EditorAction> partialActions = new();

        try
        {
            return (innerFunc(partialActions), partialActions);
        }
        catch (Exception e)
        {
            return (new MassEditResult(ParamMassEditResultType.OPERATIONERROR, @$"Error on line {currentLine}" + '\n' + e.ToString()), null);
        }
    }

    private MassEditResult ExecGlobalOp(int currentLine)
    {
        var globalArgValues = paramArgFuncs.Select(f => f(-1, null)(-1, null)(-1, (ParamEditorPseudoColumn.None, null)))
            .ToArray();
        var result = globalFunc(context, globalArgValues);
        if (!result)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Error performing global operation {globalOperation} (line {currentLine})");
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecVarStage(int currentLine)
    {
        ;
        var varArgs = paramArgFuncs
            .Select((func, i) => func(-1, null)(-1, null)(-1, (ParamEditorPseudoColumn.None, null))).ToArray();
        foreach (var varName in CurrentView.MassEdit.VSE.Search(false, varSelector, false, false))
        {
            MassEditResult res = ExecVarOpStage(currentLine, varName, varArgs);
            if (res.Type != ParamMassEditResultType.SUCCESS)
            {
                return res;
            }
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecVarOpStage(int currentLine, string var, string[] args)
    {
        MassParamEdit.massEditVars[var] = genericFunc(MassParamEdit.massEditVars[var], args);
        var result = true; // Anything that practicably can go wrong 
        if (!result)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Error performing var operation {varOperation} (line {currentLine})");
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecParamRowStage(int currentLine, List<EditorAction> partialActions)
    {
        Param activeParam = bank.Params[context.GetActiveParam()];
        IEnumerable<Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>> paramArgFunc =
            paramArgFuncs.Select((func, i) => func(0, activeParam)); // technically invalid for clipboard
        var rowEditCount = -1;
        foreach ((ParamMassEditRowSource source, Param.Row row) in CurrentView.MassEdit.PARSE.Search(context,
                     paramRowSelector, false, false))
        {
            rowEditCount++;
            Func<int, (ParamEditorPseudoColumn, Param.Column), string>[] rowArgFunc =
                paramArgFunc.Select((rowFunc, i) => rowFunc(rowEditCount, row)).ToArray();
            var paramname = source == ParamMassEditRowSource.Selection
                ? context.GetActiveParam()
                : CurrentView.GetPrimaryBank().ClipboardParam;
            MassEditResult res = rowOpOrCellStageFunc(currentLine, rowArgFunc, paramname, row, partialActions);
            if (res.Type != ParamMassEditResultType.SUCCESS)
            {
                return res;
            }
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecParamStage(int currentLine, List<EditorAction> partialActions)
    {
        var paramEditCount = -1;
        var operationForPrint = rowOperation != null ? rowOperation : cellOperation;
        foreach ((ParamBank b, Param p) in CurrentView.MassEdit.PSE.Search(false, paramSelector, false, false))
        {
            paramEditCount++;
            IEnumerable<Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>> paramArgFunc =
                paramArgFuncs.Select((func, i) => func(paramEditCount, p));
            if (argc != paramArgFuncs.Length)
            {
                return new MassEditResult(ParamMassEditResultType.PARSEERROR, $@"Invalid number of arguments for operation {operationForPrint} (line {currentLine})");
            }

            var paramname = b.GetKeyForParam(p);
            MassEditResult res = ExecRowStage(currentLine, paramArgFunc, paramname, b, p, partialActions);
            if (res.Type != ParamMassEditResultType.SUCCESS)
            {
                return res;
            }
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecRowStage(int currentLine,
        IEnumerable<Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>> paramArgFunc,
        string paramname, ParamBank b, Param p, List<EditorAction> partialActions)
    {
        var rowEditCount = -1;
        foreach (Param.Row row in CurrentView.MassEdit.RSE.Search((b, p), rowSelector, false, false))
        {
            rowEditCount++;
            Func<int, (ParamEditorPseudoColumn, Param.Column), string>[] rowArgFunc =
                paramArgFunc.Select((rowFunc, i) => rowFunc(rowEditCount, row)).ToArray();
            MassEditResult res = rowOpOrCellStageFunc(currentLine, rowArgFunc, paramname, row, partialActions);
            if (res.Type != ParamMassEditResultType.SUCCESS)
            {
                return res;
            }
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecRowOp(int currentLine, Func<int, (ParamEditorPseudoColumn, Param.Column), string>[] rowArgFunc, string paramname,
        Param.Row row, List<EditorAction> partialActions)
    {
        var rowArgValues = rowArgFunc.Select((argV, i) => argV(-1, (ParamEditorPseudoColumn.None, null))).ToArray();
        (Param p2, Param.Row rs) = rowFunc((paramname, row), rowArgValues);

        if (p2 == null)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Could not perform operation {rowOperation} {String.Join(' ', rowArgValues)} on row (line {currentLine})");
        }

        if (rs != null)
        {
            partialActions.Add(new AddParamsAction(CurrentView.Editor, p2, "FromMassEdit", new List<Param.Row> { rs }, false, true));
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecCellStage(int currentLine, Func<int, (ParamEditorPseudoColumn, Param.Column), string>[] rowArgFunc,
        string paramname, Param.Row row, List<EditorAction> partialActions)
    {
        var cellEditCount = -1;
        foreach ((ParamEditorPseudoColumn, Param.Column) col in CurrentView.MassEdit.CSE.Search((paramname, row), cellSelector,
                     false, false))
        {
            cellEditCount++;
            var cellArgValues = rowArgFunc.Select((argV, i) => argV(cellEditCount, col)).ToArray();
            MassEditResult res = ExecCellOp(currentLine, cellArgValues, paramname, row, col, partialActions);
            if (res.Type != ParamMassEditResultType.SUCCESS)
            {
                return res;
            }
        }

        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }

    private MassEditResult ExecCellOp(int currentLine, string[] cellArgValues, string paramname, Param.Row row,
        (ParamEditorPseudoColumn, Param.Column) col, List<EditorAction> partialActions)
    {
        object res = null;
        string errHelper = null;
        try
        {
            res = genericFunc(row.Get(col), cellArgValues);
        }
        catch (FormatException e)
        {
            errHelper = $"Type is not correct: {e}";
        }
        catch (InvalidCastException e)
        {
            errHelper = $"Cannot cast to correct type: {e}";
        }
        catch (Exception e)
        {
            errHelper = $"Unknown error: {e}";
        }

        if (res == null && col.Item1 == ParamEditorPseudoColumn.ID)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Could not perform operation {cellOperation} {String.Join(' ', cellArgValues)} on ID ({errHelper}) (line {currentLine})");
        }

        if (res == null && col.Item1 == ParamEditorPseudoColumn.Name)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Could not perform operation {cellOperation} {String.Join(' ', cellArgValues)} on Name ({errHelper}) (line {currentLine})");
        }

        if (res == null)
        {
            return new MassEditResult(ParamMassEditResultType.OPERATIONERROR, $@"Could not perform operation {cellOperation} {String.Join(' ', cellArgValues)} on field {col.Item2.Def.InternalName} ({errHelper}) (line {currentLine})");
        }

        partialActions.AppendParamEditAction(row, col, res);
        return new MassEditResult(ParamMassEditResultType.SUCCESS, "");
    }
}
