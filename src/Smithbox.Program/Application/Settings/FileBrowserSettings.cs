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
///     The File Browser's settings (<c>CFG.Current.FileBrowser</c>). Saved in Configuration.json under the same FileBrowser_ keys as
///     before they moved here.
/// </summary>
public sealed class FileBrowserSettings : ICfgSection
{
    // File Browser unpacker: how many files are unpacked at the same time
    public int FileBrowser_Unpack_Threads = 8;

    // File Browser exporter: replace files that already exist in the export directory (off = skip them)
    public bool FileBrowser_Export_Overwrite = false;

    // File Browser exporter: put inner files and textures in a folder named after their container, at the
    // container's path (e.g. chr/c0000-chrbnd-dcx/c0000.flver), instead of directly in the export directory
    public bool FileBrowser_Export_Keep_Container_Path = false;

    // File Browser exporter: write DCX-compressed files decompressed, without their ".dcx"
    public bool FileBrowser_Export_Decompress = false;

    // File Browser exporter: the format textures are written in
    public StudioCore.Editors.TextureViewer.TextureExportType FileBrowser_Export_Texture_Type = StudioCore.Editors.TextureViewer.TextureExportType.DDS;

    // ExtractDirectory
    public string ExtractDirectory = "";

    // UnpackDirectory
    public string UnpackDirectory = "";
}
