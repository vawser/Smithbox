using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class GxDescriptorBank
{
    private ModelEditorScreen Screen;

    private string DescriptorPath = $"{Smithbox.SmithboxDataRoot}\\GX Items\\GXItemDescriptors.json";

    public GxDescriptorBank(ModelEditorScreen screen)
    {
        Screen = screen;

         
    }
}
