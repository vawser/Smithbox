using HKLib.hk2018;
using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;

public class HavokCharacterGraphView
{
    private HavokEditorScreen Screen;
    private HavokPropertyEditor PropertyEditor;

    public HavokCharacterGraphView(HavokEditorScreen screen)
    {
        Screen = screen;
        PropertyEditor = new HavokPropertyEditor(screen);
    }

    private int depth = 0;
    private IHavokObject _selectedHavokObject;

    public void DisplayGraph()
    {
        var loadedFile = Screen.SelectedContainerInfo.LoadedFile;

        
    }
}
