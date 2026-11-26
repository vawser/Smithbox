using StudioCore.Application;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.LuaEditor;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.SoundEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.Common;

/// <summary>
/// The groups the editor rail separates its buttons into
/// </summary>
public enum EditorCategory
{
    Project,
    World,
    Assets,
    Animation,
    Data,
    Scripting,
    Audio
}

/// <summary>
/// One top-level editor, as the editor rail lists it and the editor host shows it
/// </summary>
public sealed class EditorEntry
{
    /// <summary>The editor's command endpoint, also used as its id</summary>
    public string Id;

    /// <summary>Localization key of the editor's name</summary>
    public string LabelKey;

    /// <summary>
    /// The ImGui id of the editor's window (after ###). Window state is saved under it, so it mustn't change.
    /// </summary>
    public string WindowId;

    public string Icon;

    public EditorCategory Category;

    /// <summary>Whether the project has this editor: enabled in its descriptor, supported by its game, hosted</summary>
    public Func<ProjectEntry, bool> IsAvailable;

    /// <summary>The editor itself, or null while it is still being made</summary>
    public Func<ProjectEditorHandler, EditorScreen> Screen;

    /// <summary>Whether the editor is told (<see cref="EditorScreen.OnDefocus"/>) when its window isn't shown</summary>
    public bool DefocusWhenHidden;

    /// <summary>Draws the editor's window</summary>
    public void Display(ProjectEditorHandler handler, float dt, string[] commands)
    {
        EditorStub.Display(this, handler, dt, commands);
    }
}

/// <summary>
/// The top-level editors, in the order the editor rail lists them
/// </summary>
public static class EditorRegistry
{
    /// <summary>The id the Project Editor is shown under. It isn't a project's editor, so it has no entry.</summary>
    public const string ProjectEditorId = "project";

    /// <summary>The id the Settings Editor is shown under. Like the Project Editor, it has no entry.</summary>
    public const string SettingsEditorId = "settings";

    /// <summary>Whether the project's editor host is set up, so its editors' windows can be shown</summary>
    private static bool IsHosted(ProjectEntry p) => p.Handler?.IsHostReady == true;

    public static readonly IReadOnlyList<EditorEntry> Entries = new List<EditorEntry>
    {
        new()
        {
            Id = "file", LabelKey = "FILE_FileBrowser_Stub", WindowId = "fileBrowserStub", Icon = Icons.FolderOpen, Category = EditorCategory.Project,
            IsAvailable = p => p.Descriptor.EnableFileBrowser &&
                               EditorConditions.SupportsFileBrowser(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.FileBrowser
        },
        new()
        {
            Id = "map", LabelKey = "MAP_Editor_Stub", WindowId = "mapEditor", Icon = Icons.Map, Category = EditorCategory.World,
            IsAvailable = p => p.Descriptor.EnableMapEditor &&
                               EditorConditions.SupportsMapEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.MapEditor
        },
        new()
        {
            Id = "model", LabelKey = "MODEL_Window_Model_Editor", WindowId = "modelEditor", Icon = Icons.Cube, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableModelEditor &&
                               EditorConditions.SupportsModelEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.ModelEditor
        },
        new()
        {
            Id = "material", LabelKey = "MAT_Editor_Stub", WindowId = "materialEditor", Icon = Icons.PaintBrush, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableMaterialEditor &&
                               EditorConditions.SupportsMaterialEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.MaterialEditor,
            DefocusWhenHidden = true
        },
        new()
        {
            Id = "texture", LabelKey = "TEXVIEW_Texture_Viewer_Stub", WindowId = "textureViewerStub", Icon = Icons.PictureO, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableTextureViewer &&
                               EditorConditions.SupportsTextureViewer(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.TextureViewer
        },
        new()
        {
            Id = "fx", LabelKey = "FX_Window_Fx_Editor", WindowId = "fxEditor", Icon = Icons.Magic, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableFxEditor &&
                               EditorConditions.SupportsFxEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.FxEditor
        },
        new()
        {
            Id = "hkx", LabelKey = "HAVOK_Window_Havok_Editor", WindowId = "havokEditor", Icon = Icons.Cogs, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableHavokEditor &&
                               EditorConditions.SupportsHavokEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.HavokEditor
        },
        new()
        {
            Id = "tae", LabelKey = "TAE_Editor_Title", WindowId = "timeActEditor", Icon = Icons.HourglassHalf, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableTimeActEditor &&
                               EditorConditions.SupportsTimeActEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.TimeActEditor
        },
        new()
        {
            Id = "cutscene", LabelKey = "CUTSCENE_Window_Cutscene_Editor", WindowId = "cutsceneEditor", Icon = Icons.VideoCamera, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableCutsceneEditor &&
                               EditorConditions.SupportsCutsceneEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.CutsceneEditor
        },
        new()
        {
            Id = "animation", LabelKey = "ANIM_Window_Animation_Editor", WindowId = "animationEditor", Icon = Icons.Male, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableAnimationEditor &&
                               EditorConditions.SupportsAnimationEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.AnimationEditor
        },
        new()
        {
            Id = "behavior", LabelKey = "BHV_Editor_Stub", WindowId = "behaviorEditor", Icon = Icons.Sitemap, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableBehaviorEditor &&
                               EditorConditions.SupportsBehaviorEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.BehaviorEditor
        },
        new()
        {
            Id = "param", LabelKey = "PARAM_Window_Param_Editor", WindowId = "paramEditor", Icon = Icons.Table, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsParamEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.ParamEditor
        },
        new()
        {
            Id = "projectile", LabelKey = "PROJECTILE_Window_Projectile_Editor", WindowId = "projectileEditor", Icon = Icons.Rocket, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableProjectileEditor &&
                               p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsProjectileEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.ProjectileEditor
        },
        new()
        {
            Id = "facegen", LabelKey = "FACEGEN_Window_FaceGen_Editor", WindowId = "faceGenEditor", Icon = Icons.User, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableFaceGenEditor &&
                               p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsFaceGenEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.FaceGenEditor
        },
        new()
        {
            Id = "text", LabelKey = "TEXT_Window_TextEditor", WindowId = "textEditorStub", Icon = Icons.Font, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableTextEditor &&
                               EditorConditions.SupportsTextEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.TextEditor
        },
        new()
        {
            Id = "gparam", LabelKey = "GPARAM_Editor_Stub", WindowId = "gparamEditorStub", Icon = Icons.SunO, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableGparamEditor &&
                               EditorConditions.SupportsGraphicsParamEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.GparamEditor
        },
        new()
        {
            Id = "emevd", LabelKey = "EMEVD_Editor_Stub", WindowId = "emevdEditor", Icon = Icons.Code, Category = EditorCategory.Scripting,
            IsAvailable = p => p.Descriptor.EnableEmevdEditor &&
                               EditorConditions.SupportsEmevdEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.EmevdEditor
        },
        new()
        {
            Id = "esd", LabelKey = "ESD_Editor_Stub", WindowId = "esdEditor", Icon = Icons.Hashtag, Category = EditorCategory.Scripting,
            IsAvailable = p => p.Descriptor.EnableEsdEditor &&
                               EditorConditions.SupportsEsdEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.EsdEditor
        },
        new()
        {
            Id = "lua", LabelKey = "LUA_Editor_Stub", WindowId = "luaEditor", Icon = Icons.FileCodeO, Category = EditorCategory.Scripting,
            IsAvailable = p => p.Descriptor.EnableLuaEditor &&
                               EditorConditions.SupportsLuaEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.LuaEditor
        },
        new()
        {
            Id = "sound", LabelKey = "SOUND_Editor_Stub", WindowId = "soundEditor", Icon = Icons.VolumeUp, Category = EditorCategory.Audio,
            IsAvailable = p => p.Descriptor.EnableSoundEditor &&
                               EditorConditions.SupportsSoundEditor(p.Descriptor.ProjectType) &&
                               IsHosted(p),
            Screen = h => h.SoundEditor
        },
    };

    public static EditorEntry FindById(string id)
    {
        foreach (var entry in Entries)
        {
            if (entry.Id == id)
                return entry;
        }

        return null;
    }

    public static EditorEntry FindByScreen(ProjectEditorHandler handler, EditorScreen screen)
    {
        if (handler == null || screen == null)
            return null;

        foreach (var entry in Entries)
        {
            if (ReferenceEquals(entry.Screen(handler), screen))
                return entry;
        }

        return null;
    }
}
