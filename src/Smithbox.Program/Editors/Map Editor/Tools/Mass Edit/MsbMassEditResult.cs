using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MsbMassEditResult
{
    public List<string> SelectionMessages { get; set; } = new();
    public List<string> EditMessages { get; set; } = new();

    public MsbMassEditResult() { }
}