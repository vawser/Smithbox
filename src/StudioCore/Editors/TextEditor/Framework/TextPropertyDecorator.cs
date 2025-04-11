using SoulsFormats;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextPropertyDecorator
{
    private TextEditorScreen Screen;

    public TextPropertyDecorator(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }
}