using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

// Same members and order as ExportType: the Data Transfer panel shows both as "Scope"
public enum FmgImportType
{
    [Display(Name = "TEXT_ENUM_TransferScope_Container")]
    Container,

    [Display(Name = "TEXT_ENUM_TransferScope_Text_File")]
    FMG,

    [Display(Name = "TEXT_ENUM_TransferScope_Text_Entries")]
    FMG_Entries
}
