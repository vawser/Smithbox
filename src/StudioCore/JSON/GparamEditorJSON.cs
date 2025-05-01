using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resources.JSON;

public class GparamFormatResource
{
    public List<GparamFormatReference> list { get; set; }
}

public class GparamFormatReference
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
    public List<GparamFormatMember> members { get; set; }
}

public class GparamFormatMember
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
}

public class GparamFormatEnum
{
    public List<GparamFormatEnumEntry> list { get; set; }
}

public class GparamFormatEnumEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<GparamFormatEnumMember> members { get; set; }
}

public class GparamFormatEnumMember
{
    public string id { get; set; }
    public string name { get; set; }
}