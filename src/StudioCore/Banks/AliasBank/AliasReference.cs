using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Banks.AliasBank
{
    public class AliasReference
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> tags { get; set; }
    }
}
