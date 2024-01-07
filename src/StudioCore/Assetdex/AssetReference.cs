using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Assetdex
{
    /// <summary>
    /// Class <c>AssetReference</c> is an entry contained within a <c>AssetReference</c> list within a <c>GameReference</c>.
    /// </summary>
    public class AssetReference
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> tags { get; set; }
    }
}
