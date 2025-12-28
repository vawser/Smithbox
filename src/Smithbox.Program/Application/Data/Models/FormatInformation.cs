using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class FormatResource
{
    public List<FormatReference> list { get; set; }
}

public class FormatReference
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
    public List<FormatMember> members { get; set; }
}

public class FormatMember
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
}

public class FormatEnum
{
    public List<FormatEnumEntry> list { get; set; }
}

public class FormatEnumEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<FormatEnumMember> members { get; set; }
}

public class FormatEnumMember
{
    public string id { get; set; }
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