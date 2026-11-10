using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class IconConfigurations
{
    public List<IconConfigurationEntry> Groups { get; set; } = new();
}

public class IconConfigurationEntry
{
    public string Name { get; set; }
    public string File { get; set; }
    public string SubTexturePrefix { get; set; }
    public List<string> InternalFiles { get; set; }

    /// <summary>
    /// True: only the InternalFiles sheets are searched. False: every sheet of every layout binder is searched and
    /// InternalFiles only comes first. Unset: restricted for Smithbox's bundled layouts (DS3, BB), open for the
    /// games' own sblytbnd layouts, so a mod's new sheets are found (see IconIndex).
    /// </summary>
    public bool? RestrictToInternalFiles { get; set; }
}
