using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorSelection
{
    public BehaviorContainerWrapper SelectedContainer { get; set; } = null;
    public BehaviorWrapper SelectedFile { get; set; } = null;
}
