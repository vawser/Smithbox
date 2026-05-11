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

    public void ExecuteMassEdit(string commandString, ParamBank targetBank, ParamSelection context)
    {
        CurrentView.Selection.SortSelection();

        MassParamEditRegex = new MassParamEditRegex(CurrentView);

        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(targetBank, commandString, context);

        if (child != null)
        {
            CurrentView.Editor.ActionManager.PushSubManager(child);
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
