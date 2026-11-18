using StudioCore.Application;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EsdEditor;
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

    public string Icon;

    public EditorCategory Category;

    /// <summary>Whether the project has this editor: enabled in its descriptor, supported by its game, stub made</summary>
    public Func<ProjectEntry, bool> IsAvailable;

    /// <summary>The editor itself, or null while it is still being made</summary>
    public Func<ProjectEditorHandler, EditorScreen> Screen;

    /// <summary>Draws the editor's window, through its stub</summary>
    public Action<ProjectEditorHandler, float, string[]> Display;
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

    public static readonly IReadOnlyList<EditorEntry> Entries = new List<EditorEntry>
    {
        new()
        {
            Id = "file", LabelKey = "FILE_FileBrowser_Stub", Icon = Icons.FolderOpen, Category = EditorCategory.Project,
            IsAvailable = p => p.Descriptor.EnableFileBrowser &&
                               EditorConditions.SupportsFileBrowser(p.Descriptor.ProjectType) &&
                               p.Handler?.FileBrowserStub != null,
            Screen = h => h.FileBrowser,
            Display = (h, dt, c) => h.FileBrowserStub.Display(dt, c)
        },
        new()
        {
            Id = "map", LabelKey = "MAP_Editor_Stub", Icon = Icons.Map, Category = EditorCategory.World,
            IsAvailable = p => p.Descriptor.EnableMapEditor &&
                               EditorConditions.SupportsMapEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.MapEditorStub != null,
            Screen = h => h.MapEditor,
            Display = (h, dt, c) => h.MapEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "model", LabelKey = "MODEL_Window_Model_Editor", Icon = Icons.Cube, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableModelEditor &&
                               EditorConditions.SupportsModelEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.ModelEditorStub != null,
            Screen = h => h.ModelEditor,
            Display = (h, dt, c) => h.ModelEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "material", LabelKey = "MAT_Editor_Stub", Icon = Icons.PaintBrush, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableMaterialEditor &&
                               EditorConditions.SupportsMaterialEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.MaterialEditorStub != null,
            Screen = h => h.MaterialEditor,
            Display = (h, dt, c) => h.MaterialEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "texture", LabelKey = "TEXVIEW_Texture_Viewer_Stub", Icon = Icons.PictureO, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableTextureViewer &&
                               EditorConditions.SupportsTextureViewer(p.Descriptor.ProjectType) &&
                               p.Handler?.TextureViewerStub != null,
            Screen = h => h.TextureViewer,
            Display = (h, dt, c) => h.TextureViewerStub.Display(dt, c)
        },
        new()
        {
            Id = "fx", LabelKey = "FX_Window_Fx_Editor", Icon = Icons.Magic, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableFxEditor &&
                               EditorConditions.SupportsFxEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.FxEditorStub != null,
            Screen = h => h.FxEditor,
            Display = (h, dt, c) => h.FxEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "hkx", LabelKey = "HAVOK_Window_Havok_Editor", Icon = Icons.Cogs, Category = EditorCategory.Assets,
            IsAvailable = p => p.Descriptor.EnableHavokEditor &&
                               EditorConditions.SupportsHavokEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.HavokEditorStub != null,
            Screen = h => h.HavokEditor,
            Display = (h, dt, c) => h.HavokEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "tae", LabelKey = "TAE_Editor_Title", Icon = Icons.HourglassHalf, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableTimeActEditor &&
                               EditorConditions.SupportsTimeActEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.TimeActEditorStub != null,
            Screen = h => h.TimeActEditor,
            Display = (h, dt, c) => h.TimeActEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "cutscene", LabelKey = "CUTSCENE_Window_Cutscene_Editor", Icon = Icons.VideoCamera, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableCutsceneEditor &&
                               EditorConditions.SupportsCutsceneEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.CutsceneEditorStub != null,
            Screen = h => h.CutsceneEditor,
            Display = (h, dt, c) => h.CutsceneEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "behavior", LabelKey = "BHV_Editor_Stub", Icon = Icons.Sitemap, Category = EditorCategory.Animation,
            IsAvailable = p => p.Descriptor.EnableBehaviorEditor &&
                               EditorConditions.SupportsBehaviorEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.BehaviorEditorStub != null,
            Screen = h => h.BehaviorEditor,
            Display = (h, dt, c) => h.BehaviorEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "param", LabelKey = "PARAM_Window_Param_Editor", Icon = Icons.Table, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsParamEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.ParamEditorStub != null,
            Screen = h => h.ParamEditor,
            Display = (h, dt, c) => h.ParamEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "projectile", LabelKey = "PROJECTILE_Window_Projectile_Editor", Icon = Icons.Rocket, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableProjectileEditor &&
                               p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsProjectileEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.ProjectileEditorStub != null,
            Screen = h => h.ProjectileEditor,
            Display = (h, dt, c) => h.ProjectileEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "facegen", LabelKey = "FACEGEN_Window_FaceGen_Editor", Icon = Icons.User, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableFaceGenEditor &&
                               p.Descriptor.EnableParamEditor &&
                               EditorConditions.SupportsFaceGenEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.FaceGenEditorStub != null,
            Screen = h => h.FaceGenEditor,
            Display = (h, dt, c) => h.FaceGenEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "text", LabelKey = "TEXT_Window_TextEditor", Icon = Icons.Font, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableTextEditor &&
                               EditorConditions.SupportsTextEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.TextEditorStub != null,
            Screen = h => h.TextEditor,
            Display = (h, dt, c) => h.TextEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "gparam", LabelKey = "GPARAM_Editor_Stub", Icon = Icons.SunO, Category = EditorCategory.Data,
            IsAvailable = p => p.Descriptor.EnableGparamEditor &&
                               EditorConditions.SupportsGraphicsParamEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.GparamEditorStub != null,
            Screen = h => h.GparamEditor,
            Display = (h, dt, c) => h.GparamEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "emevd", LabelKey = "EMEVD_Editor_Stub", Icon = Icons.Code, Category = EditorCategory.Scripting,
            IsAvailable = p => p.Descriptor.EnableEmevdEditor &&
                               EditorConditions.SupportsEmevdEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.EmevdEditorStub != null,
            Screen = h => h.EmevdEditor,
            Display = (h, dt, c) => h.EmevdEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "esd", LabelKey = "ESD_Editor_Stub", Icon = Icons.Hashtag, Category = EditorCategory.Scripting,
            IsAvailable = p => p.Descriptor.EnableEsdEditor &&
                               EditorConditions.SupportsEsdEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.EsdEditorStub != null,
            Screen = h => h.EsdEditor,
            Display = (h, dt, c) => h.EsdEditorStub.Display(dt, c)
        },
        new()
        {
            Id = "sound", LabelKey = "SOUND_Editor_Stub", Icon = Icons.VolumeUp, Category = EditorCategory.Audio,
            IsAvailable = p => p.Descriptor.EnableSoundEditor &&
                               EditorConditions.SupportsSoundEditor(p.Descriptor.ProjectType) &&
                               p.Handler?.SoundEditorStub != null,
            Screen = h => h.SoundEditor,
            Display = (h, dt, c) => h.SoundEditorStub.Display(dt, c)
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
