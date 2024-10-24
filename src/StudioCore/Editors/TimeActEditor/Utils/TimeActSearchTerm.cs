using StudioCore.Editors.TimeActEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Utils;

public class TimeActSearchTerm
{
    public TimeActSearchType CurrentSearchType = TimeActSearchType.AnimationID;
    public string SearchInput = "";
    public bool AllowPartialMatch = false;

    public TimeActSearchTerm() { }
}
