using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelInfo
    {
        public string Name { get; set; }
        public string ContainerDir { get; set; }
        public string ContainerPath { get; set; }
        public string Extension { get; set; }

        
        public ModelInfo(string name, string containerDir, string containerPath, string ext)
        {
            Name = name;
            ContainerDir = containerDir;
            ContainerPath = containerPath;
            Extension = ext;
        }
    }
}
