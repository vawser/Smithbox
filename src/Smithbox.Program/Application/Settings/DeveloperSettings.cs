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
///     The developer settings (<c>CFG.Current.Developer</c>): the Developer Kit's data paths and the developer options. Saved in Configuration.json under the same keys as before they moved here.
/// </summary>
public sealed class DeveloperSettings : ICfgSection
{

    // DEVKIT_
    public bool DEVKIT_DisplayDataSources = true;
    public string DEVKIT_DataPath_DataFolder = "";
    public string DEVKIT_DataPath_OutputFolder = "";
    public string DEVKIT_DataPath_DES = "";
    public string DEVKIT_DataPath_DS1 = "";
    public string DEVKIT_DataPath_DS1R = "";
    public string DEVKIT_DataPath_DS2 = "";
    public string DEVKIT_DataPath_DS2S = "";
    public string DEVKIT_DataPath_DS3 = "";
    public string DEVKIT_DataPath_BB = "";
    public string DEVKIT_DataPath_SDT = "";
    public string DEVKIT_DataPath_ER = "";
    public string DEVKIT_DataPath_AC6 = "";
    public string DEVKIT_DataPath_NR = "";
    public string DEVKIT_DataPath_DB_NT = "";
    public string DEVKIT_DataPath_DB = "";

    // Developer_
    public bool Developer_Enable_Tools = false;
    public string Developer_Smithbox_Build_Folder = "";
}
