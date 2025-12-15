using System.Collections.Generic;

namespace StudioCore.Application;

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
