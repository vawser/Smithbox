using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;

namespace StudioCore.Editors.Common;

public static class FocusManager
{
    public static EditorFocusContext Focus = EditorFocusContext.None;

    public static void SetFocus(EditorFocusContext focus)
    {
        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
        {
            Focus = focus;
        }
    }

    public static bool IsFocus(EditorFocusContext focus)
    {
        return Focus == focus;
    }

    // Convenience functions

    public static bool IsInFileBrowser()
    {
        if (Focus is EditorFocusContext.FileBrowser_None
            or EditorFocusContext.FileBrowser_FileList
            or EditorFocusContext.FileBrowser_Item
            or EditorFocusContext.FileBrowser_Tools)
            return true;

        return false;
    }

    public static bool IsInMapEditor()
    {
        if (Focus is EditorFocusContext.MapEditor_None
            or EditorFocusContext.MapEditor_FileList
            or EditorFocusContext.MapEditor_ContentList
            or EditorFocusContext.MapEditor_Properties
            or EditorFocusContext.MapEditor_Tools
            or EditorFocusContext.MapEditor_Viewport)
            return true;

        return false;
    }

    public static bool IsInModelEditor()
    {
        if (Focus is EditorFocusContext.ModelEditor_None
            or EditorFocusContext.ModelEditor_ContainerList
            or EditorFocusContext.ModelEditor_FileList
            or EditorFocusContext.ModelEditor_ContentList
            or EditorFocusContext.ModelEditor_Properties
            or EditorFocusContext.ModelEditor_Tools
            or EditorFocusContext.ModelEditor_Viewport)
            return true;

        return false;
    }

    public static bool IsInParamEditor()
    {
        if (Focus is EditorFocusContext.ParamEditor_None
            or EditorFocusContext.ParamEditor_ParamList
            or EditorFocusContext.ParamEditor_TableList
            or EditorFocusContext.ParamEditor_RowList
            or EditorFocusContext.ParamEditor_FieldList
            or EditorFocusContext.ParamEditor_Tools)
            return true;

        return false;
    }

    public static bool IsInTextEditor()
    {
        if (Focus is EditorFocusContext.TextEditor_None
            or EditorFocusContext.TextEditor_FileList
            or EditorFocusContext.TextEditor_EntryList
            or EditorFocusContext.TextEditor_EntryContents
            or EditorFocusContext.TextEditor_Tools)
            return true;

        return false;
    }

    public static bool IsInGparamEditor()
    {
        if (Focus is EditorFocusContext.GparamEditor_None
            or EditorFocusContext.GparamEditor_FileList
            or EditorFocusContext.GparamEditor_GroupList
            or EditorFocusContext.GparamEditor_FieldList
            or EditorFocusContext.GparamEditor_Properties
            or EditorFocusContext.GparamEditor_Tools)
            return true;

        return false;
    }


    public static bool IsInMaterialEditor()
    {
        if (Focus is EditorFocusContext.MaterialEditor_None
            or EditorFocusContext.MaterialEditor_FileList
            or EditorFocusContext.MaterialEditor_Properties
            or EditorFocusContext.MaterialEditor_Tools)
            return true;

        return false;
    }

    public static bool IsInTextureViewer()
    {
        if (Focus is EditorFocusContext.TextureViewer_None
            or EditorFocusContext.TextureViewer_FileList
            or EditorFocusContext.TextureViewer_Viewer
            or EditorFocusContext.TextureViewer_Properties
            or EditorFocusContext.TextureViewer_Tools)
            return true;

        return false;
    }
}

public enum EditorFocusContext
{
    None,

    FileBrowser_None,
    FileBrowser_FileList,
    FileBrowser_Item,
    FileBrowser_Tools,

    MapEditor_None,
    MapEditor_FileList,
    MapEditor_ContentList,
    MapEditor_Properties,
    MapEditor_Tools,
    MapEditor_Viewport,

    ModelEditor_None,
    ModelEditor_ContainerList,
    ModelEditor_FileList,
    ModelEditor_ContentList,
    ModelEditor_Properties,
    ModelEditor_Tools,
    ModelEditor_Viewport,

    ParamEditor_None,
    ParamEditor_ParamList,
    ParamEditor_TableList,
    ParamEditor_RowList,
    ParamEditor_FieldList,
    ParamEditor_Tools,

    TextEditor_None,
    TextEditor_FileList,
    TextEditor_EntryList,
    TextEditor_EntryContents,
    TextEditor_Tools,

    GparamEditor_None,
    GparamEditor_FileList,
    GparamEditor_GroupList,
    GparamEditor_FieldList,
    GparamEditor_Properties,
    GparamEditor_Tools,

    MaterialEditor_None,
    MaterialEditor_FileList,
    MaterialEditor_Properties,
    MaterialEditor_Tools,

    TextureViewer_None,
    TextureViewer_FileList,
    TextureViewer_Viewer,
    TextureViewer_Properties,
    TextureViewer_Tools
}
