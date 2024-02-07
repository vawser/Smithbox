using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Data.Aliases
{
    public class AliasReference
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> tags { get; set; }
    }
}
