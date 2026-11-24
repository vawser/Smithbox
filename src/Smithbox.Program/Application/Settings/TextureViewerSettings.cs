using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The Texture Viewer's settings (<c>CFG.Current.TextureViewer</c>). Saved in Configuration.json under the same TextureViewer_ keys as
///     before they moved here.
/// </summary>
public sealed class TextureViewerSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Texture Viewer
    ///------------------------------------------------------------
    // General
    public bool TextureViewer_File_List_Display_Low_Detail_Entries = true;

    public bool TextureViewer_File_List_Display_Character_Aliases = true;
    public bool TextureViewer_File_List_Display_Asset_Aliases = true;
    public bool TextureViewer_File_List_Display_Part_Aliases = true;

    public bool TextureViewer_File_List_Display_Particle_Aliases = true;

    // TextureViewerImport_
    // Texture Import: 0 = keep the replaced texture's format, otherwise the DXGI format to write
    public int TextureViewerImport_Format = 0;
    // 0 = match the replaced texture, 1 = full chain, 2 = top level only
    public int TextureViewerImport_MipMode = 0;
    // Resize imported images to the replaced texture's size
    public bool TextureViewerImport_MatchSize = false;
    public string TextureViewerImport_LastFolder = "";
    // New cells: 0 = the image's size, 1 = the selected cell's size
    public int TextureViewerImport_CellSizeMode = 0;
    // Double a full sheet to fit new cells
    public bool TextureViewerImport_GrowSheet = false;

    // TextureViewerToolbar_
    public string TextureViewerToolbar_ExportTextureLocation = "";
    public bool TextureViewerToolbar_ExportTexture_IncludeFolder = true;
    public bool TextureViewerToolbar_ExportTexture_DisplayConfirm = true;
    public TextureExportType TextureViewerToolbar_ExportTextureType = TextureExportType.DDS;

    // TextureViewerLayout_
    // Layout editor (T5)
    public bool TextureViewerLayout_ShowCells = true;
    public bool TextureViewerLayout_ShowNames = false;
    public int TextureViewerLayout_Snap = 1;

    // TextureViewerTwin_
    // Low twin size of new cells: 0 = guess from the name family, 1 = full size, 2 = half size
    public int TextureViewerTwin_ScaleMode = 0;
}
