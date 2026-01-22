using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class IconConfig
{
    /// <summary>
    /// The icon configuration to use for this tex ref
    /// </summary>
    public string TargetConfiguration = "";

    internal IconConfig(ParamMeta parent, string refString)
    {
        TargetConfiguration = refString;
    }

    internal string getStringForm()
    {
        return TargetConfiguration;
    }
}