using StudioCore.Banks.AliasBank;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.GraphicsEditor;
using StudioCore.MaterialEditor;
using StudioCore.ParticleEditor;
using StudioCore.EmevdEditor;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.BehaviorEditor;

namespace StudioCore.Editors;
public static class EditorContainer
{
    public static MapEditorScreen MsbEditor { get; set; }
    public static ModelEditorScreen ModelEditor { get; set; }
    public static TextEditorScreen TextEditor { get; set; }
    public static ParamEditorScreen ParamEditor { get; set; }
    public static TimeActEditorScreen TimeActEditor { get; set; }
    public static CutsceneEditorScreen CutsceneEditor { get; set; }
    public static GparamEditorScreen GparamEditor { get; set; }
    public static MaterialEditorScreen MaterialEditor { get; set; }
    public static ParticleEditorScreen ParticleEditor { get; set; }
    public static EmevdEditorScreen ScriptEditor { get; set; }
    public static EsdEditorScreen TalkEditor { get; set; }
    public static TextureViewerScreen TextureViewer { get; set; }
    public static HavokEditorScreen BehaviorEditor { get; set; }
}
