using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class IconConfigurations
{
    public List<IconConfigurationEntry> Configurations { get; set; }
}

public class IconConfigurationEntry
{
    public string Name { get; set; }
    public string File { get; set; }
    public string SubTexturePrefix { get; set; }
    public List<string> InternalFiles { get; set; }
}
