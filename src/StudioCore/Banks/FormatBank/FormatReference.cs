using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.FormatBank;

public class FormatReference
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
    public List<FormatMember> members { get; set; }
}
