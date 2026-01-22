using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class ParamCommandQueue
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public ParamCommandQueue(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] args)
    {
        DoFocus = false;

        var viewHandler = Project.Handler.ParamEditor.ViewHandler;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var primaryBank = Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        // Parse select commands
        if (args != null)
        {
            if (args[0] == "select" || args[0] == "view")
            {
                if (args.Length > 2 && primaryBank.Params.ContainsKey(args[2]))
                {
                    DoFocus = args[0] == "select";

                    if (!DoFocus)
                    {
                        activeView.JumpToSelectedRow = true;
                    }

                    ParamView viewToModify = activeView;

                    if (args[1].Equals("new"))
                    {
                        viewToModify = viewHandler.AddView();
                    }
                    else
                    {
                        var cmdIndex = -1;
                        var parsable = int.TryParse(args[1], out cmdIndex);

                        if (parsable && cmdIndex >= 0 &&
                            cmdIndex < viewHandler.ParamViews.Count)
                        {
                            viewToModify = viewHandler.ParamViews[cmdIndex];
                        }
                    }

                    viewHandler.ActiveView = viewToModify;
                    viewToModify.Selection.SetActiveParam(args[2]);

                    var curActiveParam = viewToModify.Selection.GetActiveParam();

                    // In TAble Group mode: update the table group list
                    if (viewToModify.ParamTableWindow.IsInTableGroupMode(curActiveParam))
                    {
                        viewToModify.ParamTableWindow.UpdateTableSelection(curActiveParam);
                    }

                    if (args.Length > 3)
                    {
                        bool onlyAddToSelection = args.Length > 4 && args[4] == "addOnly";
                        if (!onlyAddToSelection)
                            viewToModify.Selection.SetActiveRow(null, DoFocus);

                        Param p = primaryBank.Params[viewToModify.Selection.GetActiveParam()];
                        int id;
                        var parsed = int.TryParse(args[3], out id);

                        // In Table Group mode: set the current Table Group to this ID
                        if (viewToModify.ParamTableWindow.IsInTableGroupMode(curActiveParam))
                        {
                            viewToModify.ParamTableWindow.CurrentTableGroup = id;
                        }

                        if (parsed)
                        {
                            Param.Row r = p.Rows.FirstOrDefault(r => r.ID == id);
                            if (r != null)
                            {
                                if (onlyAddToSelection)
                                    viewToModify.Selection.AddRowToSelection(r);
                                else
                                    viewToModify.Selection.SetActiveRow(r, DoFocus);
                            }
                        }
                    }
                }
            }
            else if (args[0] == "back")
            {
                activeView.Selection.PopHistory();
            }
            else if (args[0] == "search")
            {
                if (args.Length > 1)
                {
                    activeView.Selection.SetCurrentRowSearchString(args[1]);
                }
            }
            else if (args[0] == "menu" && args.Length > 1)
            {
                if (args[1] == "ctrlVPopup")
                {
                    ImGui.OpenPopup("ctrlVPopup");
                }
                else if (args[1] == "massEditRegex")
                {
                    activeView.MassEdit.CurrentMenuInput = args.Length > 2 ? args[2] : activeView.MassEdit.CurrentMassEditInput;

                    activeView.MassEdit.OpenMassEditPopup("massEditMenuRegex");
                }
                else if (args[1] == "massEditCSVExport")
                {
                    IReadOnlyList<Param.Row> rows = ParamCsvTools.CsvExportGetRows(
                        Editor,
                        Enum.Parse<ParamUpgradeRowGetType>(args[2]));

                    activeView.MassEdit.MassEditOutput_CSV = ParamIO.GenerateCSV(
                        Project,
                        rows,
                        primaryBank.Params[activeView.Selection.GetActiveParam()],
                        delimiter[0]);

                    activeView.MassEdit.OpenMassEditPopup("massEditMenuCSVExport");
                }
                else if (args[1] == "massEditCSVImport")
                {
                    activeView.MassEdit.OpenMassEditPopup("massEditMenuCSVImport");
                }
                else if (args[1] == "massEditSingleCSVExport")
                {
                    activeView.MassEdit.MassEdit_SingleField_CSV = args[2];

                    IReadOnlyList<Param.Row> rows = ParamCsvTools.CsvExportGetRows(
                        Editor,
                        Enum.Parse<ParamUpgradeRowGetType>(args[3]));

                    activeView.MassEdit.MassEditOutput_CSV = ParamIO.GenerateSingleCSV(
                        rows,
                        primaryBank.Params[activeView.Selection.GetActiveParam()],
                        activeView.MassEdit.MassEdit_SingleField_CSV,
                        delimiter[0]);

                    activeView.MassEdit.OpenMassEditPopup("massEditMenuSingleCSVExport");
                }
                else if (args[1] == "massEditSingleCSVImport" && args.Length > 2)
                {
                    activeView.MassEdit.MassEdit_SingleField_CSV = args[2];

                    activeView.MassEdit.OpenMassEditPopup("massEditMenuSingleCSVImport");
                }
                else if (args[1] == "distributionPopup" && args.Length > 2)
                {
                    Param p = primaryBank.GetParamFromName(activeView.Selection.GetActiveParam());

                    (ParamEditorPseudoColumn, Param.Column) col = p.GetCol(args[2]);

                    Editor.StatisticsMenu._distributionOutput =
                        ParamUtils.GetParamValueDistribution(activeView.Selection.GetSelectedRows(), col);

                    var ouputStr = Editor.StatisticsMenu._distributionOutput
                        .Select(e => e.Item1.ToString().PadLeft(9) +
                        " " +
                        e.Item2.ToParamEditorString() +
                        " times");

                    Editor.StatisticsMenu._statisticPopupOutput = string.Join('\n', ouputStr);
                    Editor.StatisticsMenu._statisticPopupParameter = args[2];

                    Editor.StatisticsMenu.OpenStatisticPopup("distributionPopup");
                }
            }
        }
    }
}