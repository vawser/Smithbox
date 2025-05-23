using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Formats;

public class MaterialDisplayConfiguration
{
    public List<MaterialFileListConfiguration> FileListConfigurations { get; set; }
}

public class MaterialFileListConfiguration
{
    public string SourceType { get; set; }
    public string Binder { get; set; }
    public string CommonPath { get; set; }
}
