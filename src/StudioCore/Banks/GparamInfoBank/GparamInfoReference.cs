using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.GparamBank;

public class GparamInfoReference
{
    public string id { get; set; }
    public string name { get; set; }
    public List<GparamInfoMember> members { get; set; }
}
