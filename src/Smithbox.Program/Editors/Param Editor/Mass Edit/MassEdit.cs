using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class MassEdit
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView CurrentView;

    public MassEditState State;

    public MassEditToolMenu ToolMenu;
    public MassEditPopupMenu PopupMenu;

    // These are created here so the Mass Edit systems are local to the current project
    public SearchEngine<ParamSelection, (ParamMassEditRowSource, Param.Row)> PARSE;
    public ParamSearchEngine PSE;
    public RowSearchEngine RSE;
    public CellSearchEngine CSE;
    public VarSearchEngine VSE;

    public AutoFill AutoFill;

    public MassParamEditRegex MassParamEditRegex;

    public MEGlobalOperation GlobalOps;
    public MERowOperation RowOps;
    public MEValueOperation FieldOps;
    public MEOperationArgument OperationArgs;

    public MassEdit(ParamEditorScreen editor, ProjectEntry project, ParamEditorView parentView)
    {
        Editor = editor;
        Project = project;
        CurrentView = parentView;

        State = new(this);

        PSE = new ParamSearchEngine(CurrentView);
        RSE = new RowSearchEngine(CurrentView);
        PARSE = new ParamAndRowSearchEngine(CurrentView, this);
        CSE = new CellSearchEngine(CurrentView);
        VSE = new VarSearchEngine(CurrentView);

        MassParamEditRegex = new MassParamEditRegex(CurrentView);

        GlobalOps = new MEGlobalOperation(CurrentView);
        RowOps = new MERowOperation(CurrentView);
        FieldOps = new MEValueOperation(CurrentView);
        OperationArgs = new MEOperationArgument(CurrentView);

        AutoFill = new AutoFill(CurrentView, this);

        ToolMenu = new(this);
        PopupMenu = new(this);
    }

    public void ApplyMassEdit(string commandString)
    {
        ExecuteMassEdit(commandString, CurrentView.GetPrimaryBank(), CurrentView.Selection);
    }

    /// <summary>
    /// Runs the script and reports what it would change, then undoes it, so nothing is changed.
    /// <para>
    /// The script really runs (later commands can depend on earlier ones, so it can't be predicted), into a
    /// separate undo history that is then undone. What undo doesn't restore is put back by hand: the params'
    /// modified (*) marks, the row clipboard (copy, clear), and mass edit variables (newvar, clearvars).
    /// </para>
    /// </summary>
    public void PreviewMassEdit(string commandString)
    {
        var targetBank = CurrentView.GetPrimaryBank();
        var context = CurrentView.Selection;

        if (CurrentView.Editor.IsSaving)
        {
            var reason = LOC.Get("PARAM_Action_Blocked_During_Save");
            Smithbox.Log(this, reason, Microsoft.Extensions.Logging.LogLevel.Warning);
            State.MassEditResult = reason;
            return;
        }

        var cleanParams = targetBank.Params.Values.Where(p => !ParamDirtyTracker.IsDirty(p)).ToList();
        var clipboardParam = targetBank.ClipboardParam;
        var clipboardRows = targetBank.ClipboardRows.ToList();
        var variables = new Dictionary<string, object>(MassParamEdit.massEditVars);

        MassEditResult r;

        try
        {
            CurrentView.Selection.SortSelection();

            MassParamEditRegex = new MassParamEditRegex(CurrentView);

            ActionManager child;
            (r, child) = MassParamEditRegex.PerformMassEdit(targetBank, commandString, context);

            // A failed run has already rolled itself back
            if (child != null && child.CanUndo())
            {
                child.UndoAllAction();
            }
        }
        finally
        {
            foreach (var param in cleanParams)
            {
                ParamDirtyTracker.ClearDirty(param);
            }

            targetBank.ClipboardParam = clipboardParam;
            targetBank.ClipboardRows.Clear();
            targetBank.ClipboardRows.AddRange(clipboardRows);

            MassParamEdit.massEditVars.Clear();

            foreach (var (name, value) in variables)
            {
                MassParamEdit.massEditVars[name] = value;
            }
        }

        State.MassEditResult = r.Type == ParamMassEditResultType.SUCCESS
            ? $"{LOC.Get("PARAM_MassEdit_Result_Preview_Header")}\n{r.Information}"
            : r.Information;
    }

    /// <summary>
    /// "Mass Edit: " and the script on one line, shortened
    /// </summary>
    private static string GetMassEditLabel(string script)
    {
        var text = (script ?? "").ReplaceLineEndings(" ").Trim();

        const int maxLength = 60;

        if (text.Length > maxLength)
        {
            text = text[..maxLength] + "...";
        }

        return LOC.Get("PARAM_Action_Name_Mass_Edit", text);
    }

    public void ExecuteMassEdit(string commandString, ParamBank targetBank, ParamSelection context)
    {
        // Mass edits apply their changes directly (through a child ActionManager), so the save guard on the
        // editor's ActionManager doesn't cover them. Refuse the whole edit while a save is running.
        if (CurrentView.Editor.IsSaving)
        {
            var reason = LOC.Get("PARAM_Action_Blocked_During_Save");
            Smithbox.Log(this, reason, Microsoft.Extensions.Logging.LogLevel.Warning);
            State.MassEditResult = reason;
            return;
        }

        CurrentView.Selection.SortSelection();

        MassParamEditRegex = new MassParamEditRegex(CurrentView);

        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(targetBank, commandString, context);

        if (child != null)
        {
            // Named by its script in the Edit menu and the History window (the full script in the tooltip)
            CurrentView.Editor.ActionManager.PushSubManager(child, GetMassEditLabel(commandString), commandString?.Trim());
        }

        if (r.Type == ParamMassEditResultType.SUCCESS)
        {
            CurrentView.GetParamData().RefreshParamDifferenceCacheTask();
        }

        State.MassEditResult = r.Information;
    }

    public void OpenMassEditPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        State.DisplayMassEditPopup = true;
    }

    public void ConstructCommandFromField(string internalName)
    {
        var propertyName = internalName.Replace(" ", "\\s");
        string currInput = State.CurrentMenuInput;

        if (currInput == "")
        {
            // Add selection section if input is empty
            State.CurrentMenuInput = $"selection: {propertyName}: ";
        }
        else
        {
            // Otherwise just add the property name
            currInput = $"{currInput}{propertyName}";
            State.CurrentMenuInput = currInput;
        }
    }
}
