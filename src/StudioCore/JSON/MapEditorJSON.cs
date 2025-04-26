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

public class SpawnStateResource
{
    public List<SpawnStateEntry> list { get; set; }
}

public class SpawnStateEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<SpawnStatePair> states { get; set; }
}

public class SpawnStatePair
{
    public string value { get; set; }
    public string name { get; set; }
}

public class FormatMask
{
    public List<FormatMaskEntry> list { get; set; }
}

public class FormatMaskEntry
{
    public string model { get; set; }
    public List<MaskSection> section_one { get; set; }
    public List<MaskSection> section_two { get; set; }
    public List<MaskSection> section_three { get; set; }
}

public class MaskSection
{
    public string mask { get; set; }
    public string name { get; set; }
}
