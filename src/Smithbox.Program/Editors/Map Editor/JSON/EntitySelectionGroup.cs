using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class EntitySelectionGroupList
{
    public List<EntitySelectionGroupResource> Resources { get; set; }
}

public class EntitySelectionGroupResource
{
    public string Name { get; set; }
    public int SelectionGroupKeybind { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Selection { get; set; }
}