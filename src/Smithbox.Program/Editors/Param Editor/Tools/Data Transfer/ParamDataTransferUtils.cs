using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public static class ParamDataTransferUtils
{
    public static void TryWriteFile(string path, string text)
    {
        try
        {
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Write_File_FAIL", path), e);
        }

        Smithbox.Log<ParamDataTransferTool>(
            LOC.Get("PARAM_DataTransfer_Write_file_PASS", path));
    }

    public static string TryReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Write_File_FAIL", path), e);

            return null;
        }
    }
}

public enum CsvImportMode
{
    [Display(Name = "PARAM_ENUM_CsvExportType_All_Params")]
    AllParams,

    [Display(Name = "PARAM_ENUM_CsvExportType_Selected_Param")]
    SelectedParam
}
public enum CsvExportType
{
    [Display(Name = "PARAM_ENUM_CsvExportType_All_Params")]
    AllParams,

    [Display(Name = "PARAM_ENUM_CsvExportType_Modified_Params")]
    ModifiedParams,

    [Display(Name = "PARAM_ENUM_CsvExportType_Selected_Param")]
    SelectedParam,

    [Display(Name = "PARAM_ENUM_CsvExportType_Modified_Rows")]
    ModifiedRows,

    [Display(Name = "PARAM_ENUM_CsvExportType_Selected_Rows")]
    SelectedRows
}

public enum CsvImportType
{
    [Display(Name = "PARAM_ENUM_CsvImportType_All_Fields")]
    AllFields,
    [Display(Name = "PARAM_ENUM_CsvImportType_Row_Name")]
    RowName,
    [Display(Name = "PARAM_ENUM_CsvImportType_Specific_Field")]
    SpecificField
}

public enum ImportSourceType
{
    UserInput,
    File
}

public enum CsvExportMode
{
    [Display(Name = "PARAM_ENUM_CsvExportMode_Window")]
    Window,
    [Display(Name = "PARAM_ENUM_CsvExportMode_File")]
    File
}