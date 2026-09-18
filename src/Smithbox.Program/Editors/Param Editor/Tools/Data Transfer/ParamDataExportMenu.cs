using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public class ParamDataExportMenu
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamDataTransferTool Parent;

    public ParamDataExportMenu(ParamEditorView view, ProjectEntry project, ParamDataTransferTool parent)
    {
        View = view;
        Project = project;
        Parent = parent;
    }

    public void Display()
    {
        var activeParamExists = View.Selection.ActiveParamExists();

        // Export CSV
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_CSV")}##importCsvMenuHeader",
            activeParamExists))
        {
            ExportMenu();

            ImGui.EndMenu();
        }
    }
    public void ExportMenu()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;

        // All Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_All_Rows")}##allRowsMenuHeader"))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.AllRows);
            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Quick Actions
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Quick_Action")}##quickActionMenuHeader"))
        {
            // Export Selected Names to Window
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Names_Wnd")}##exportSelectedNamesWnd"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
            }

            // Export Selected Param to Window
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Param_Wnd")}##exportSelectedParamWnd"))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
            }

            // Export Selected Param to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Param_File")}##exportSelectedParamFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    Parent.ExportPath = path;
                    Parent.ExportEntireParam();
                }
            }

            ImGui.EndMenu();
        }

        // Modified Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Modified_Rows")}##modifiedRowsMenuHeader", primaryBank.GetVanillaDiffRows(View.Selection.GetActiveParam()).Any()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.ModifiedRows);
            ImGui.EndMenu();
        }

        // Selected Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Selected_Rows")}##selectedRowsMenuHeader", View.Selection.RowSelectionExists()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.SelectedRows);
            ImGui.EndMenu();
        }

        // All Params
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_All_Params")}##allParamsMenuHeader"))
        {
            // Export All Params to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Params_to_File")}##exportAllParamsToFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    Parent.ExportPath = path;
                    Parent.ExportAllParams();
                }
            }

            // Export All Modified Params to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Modified_Params_to_File")}##exportAllModifiedParamsToFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    Parent.ExportPath = path;
                    Parent.ExportAllModifiedParams();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void CsvExportDisplay(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        // Export to Window
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_to_Window")}##exportToWindowMenuHeader"))
        {
            // Export All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Fields")}##exportAllFieldsAction"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{rowType}");
            }

            // Export Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_Specific_Field")}##exportSpecificFieldHeader"))
            {
                // Row Name
                if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_Row_Name")}##exportRowNameAction"))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/{rowType}");
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/{field.InternalName}/{rowType}");
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        // Export to File
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_to_File")}##exportToFileMenuHeader"))
        {
            // Export All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Fields")}##exportAllFieldsAction"))
            {
                Parent.RowType = rowType;

                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    Parent.ExportPath = path;
                    Parent.ExportAllFields(Parent.RowType);
                }
            }

            // Export Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_Specific_Field")}##exportSpecificFieldHeader"))
            {
                // Row Name
                if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_Row_Name")}##exportRowNameAction"))
                {
                    Parent.RowType = rowType;

                    var dialog = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                    if (dialog)
                    {
                        Parent.ExportPath = path;
                        Parent.ExportNameField(Parent.RowType);
                    }
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        Parent.SpecificFieldName = field.InternalName;
                        Parent.RowType = rowType;

                        var dialog = PlatformUtils.Instance.OpenFolderDialog(
                            LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                        if (dialog)
                        {
                            Parent.ExportPath = path;
                            Parent.ExportSpecificField(Parent.RowType);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

}
