using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MsbMassEditLog
{
    public List<string> SelectionLog { get; set; } = new();
    public List<string> EditLog { get; set; } = new();

    public MsbMassEditLog() { }
}