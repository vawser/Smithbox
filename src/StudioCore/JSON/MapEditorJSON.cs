using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resources.JSON;

public class MassEditTemplate
{
    public string Name { get; set; }
    public int MapLogic { get; set; }
    public int SelectionLogic { get; set; }
    public List<string> MapInputs { get; set; }
    public List<string> SelectionInputs { get; set; }
    public List<string> EditInputs { get; set; }
}

public class EntitySelectionGroupList
{
    public List<EntitySelectionEntry> Resources { get; set; }
}

public class EntitySelectionEntry
{
    public string Name { get; set; }
    public int SelectionGroupKeybind { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Selection { get; set; }
}
