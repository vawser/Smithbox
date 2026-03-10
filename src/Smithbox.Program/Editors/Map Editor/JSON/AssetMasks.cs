using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class AssetMasks
{
    public List<AssetMaskEntry> List { get; set; } = new();
}

public class AssetMaskEntry
{
    public string model { get; set; }
    public List<AssetMaskSection> section_one { get; set; }
    public List<AssetMaskSection> section_two { get; set; }
    public List<AssetMaskSection> section_three { get; set; }
}

public class AssetMaskSection
{
    public string mask { get; set; }
    public string name { get; set; }
}