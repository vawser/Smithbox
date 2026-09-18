using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public class ParamDataImportMenu
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamDataTransferTool Parent;

    public ParamDataImportMenu(ParamEditorView view, ProjectEntry project, ParamDataTransferTool parent)
    {
        View = view;
        Project = project;
        Parent = parent;
    }
    public void Display()
    {
        var activeParamExists = View.Selection.ActiveParamExists();

        // Import CSV
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Import_CSV")}##exportCsvMenuHeader",
            activeParamExists))
        {
            ImportMenu();

            ImGui.EndMenu();
        }
    }
    public void ImportMenu()
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        // All Fields
        if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_All_Fields")}##importAllFieldsAction"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        // Row Name
        if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_Row_Name")}##importRowNameAction"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditSingleCSVImport/Name");
        }

        // Specific Field
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Import_Specific_Field")}##importSpecificFieldMenuHeader"))
        {
            foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
            {
                // <field>
                if (ImGui.MenuItem(field.InternalName))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVImport/{field.InternalName}");
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_From_File")}##fromFileMenuHeader", View.Selection.ActiveParamExists()))
        {
            // All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_All_Fields")}##importAllFieldsAction_file"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

                if (dialog)
                {
                    Parent.ImportPath = path;
                    Parent.ImportAllFields(ImportSourceType.File, Parent.ImportPath);
                }
            }

            // Row Name
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_Row_Name")}##importRowNameAction_file"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

                if (dialog)
                {
                    Parent.ImportPath = path;
                    Parent.ImportSpecificField(ImportSourceType.File, Parent.ImportPath, "Name");
                }
            }

            // Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Import_Specific_Field")}##importSpecificFieldMenuHeader_file"))
            {
                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        Parent.SpecificFieldName = field.InternalName;

                        var dialog = PlatformUtils.Instance.OpenFileDialog(
                            LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

                        if (dialog)
                        {
                            Parent.ImportPath = path;
                            Parent.ImportSpecificField(ImportSourceType.File, Parent.ImportPath, Parent.SpecificFieldName);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

}
