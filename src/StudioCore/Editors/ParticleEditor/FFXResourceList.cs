using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.FXR3;

namespace StudioCore.Editors.ParticleEditor
{
    public class FFXResourceList
    {
        public List<string> Entries { get; set; }

        public FFXResourceList()
        {
            Entries = new List<string>();
        }

        public FFXResourceList(List<string> contents)
        {
            Entries = contents;
        }
    }
}
