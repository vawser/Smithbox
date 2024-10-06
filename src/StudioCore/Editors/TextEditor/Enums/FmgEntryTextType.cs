using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Enums;

/// <summary>
/// Entry type for Title, Summary, Description, or other.
/// </summary>
public enum FmgEntryTextType
{
    TextBody = 0,
    Title = 1,
    Summary = 2,
    Description = 3,
    ExtraText = 4
}
