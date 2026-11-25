using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

// Same members and order as FmgImportType: the Data Transfer panel shows both as "Scope"
public enum ExportType
{
    [Display(Name = "TEXT_ENUM_TransferScope_Container")]
    Container,

    [Display(Name = "TEXT_ENUM_TransferScope_Text_File")]
    FMG,

    [Display(Name = "TEXT_ENUM_TransferScope_Text_Entries")]
    FMG_Entries
}
