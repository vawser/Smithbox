using StudioCore.Keybinds;
using System.Text.Json.Serialization;
using static StudioCore.Keybinds.InputManager;

namespace StudioCore.Application;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Program
[JsonSerializable(typeof(Startup))]
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(GraphicsSettings))]
[JsonSerializable(typeof(ViewportSettings))]
[JsonSerializable(typeof(MapEditorSettings))]
[JsonSerializable(typeof(ParamEditorSettings))]
[JsonSerializable(typeof(InterfaceSettings))]
[JsonSerializable(typeof(TextEditorSettings))]
[JsonSerializable(typeof(ModelEditorSettings))]
[JsonSerializable(typeof(TimeActEditorSettings))]
[JsonSerializable(typeof(LuaEditorSettings))]
[JsonSerializable(typeof(FxEditorSettings))]
[JsonSerializable(typeof(ProjectileEditorSettings))]
[JsonSerializable(typeof(AnimEditorSettings))]
[JsonSerializable(typeof(GparamEditorSettings))]
[JsonSerializable(typeof(CutsceneEditorSettings))]
[JsonSerializable(typeof(HavokEditorSettings))]
[JsonSerializable(typeof(MaterialEditorSettings))]
[JsonSerializable(typeof(BehaviorEditorSettings))]
[JsonSerializable(typeof(EmevdEditorSettings))]
[JsonSerializable(typeof(EsdEditorSettings))]
[JsonSerializable(typeof(SoundEditorSettings))]
[JsonSerializable(typeof(FaceGenEditorSettings))]
[JsonSerializable(typeof(FileBrowserSettings))]
[JsonSerializable(typeof(TextureViewerSettings))]
[JsonSerializable(typeof(ProjectSettings))]
[JsonSerializable(typeof(DeveloperSettings))]
[JsonSerializable(typeof(ProgramSettings))]
[JsonSerializable(typeof(UI))]

[JsonSerializable(typeof(KeybindStore))]
[JsonSerializable(typeof(KeyBinding))]
[JsonSerializable(typeof(List<KeyBinding>))]

[JsonSerializable(typeof(MousebindStore))]
[JsonSerializable(typeof(MouseBinding))]
[JsonSerializable(typeof(List<MouseBinding>))]

// Project
[JsonSerializable(typeof(ProjectDescriptor))]
[JsonSerializable(typeof(LegacyProjectDescriptor))]
[JsonSerializable(typeof(LegacyProjectDescriptorAlt))]

[JsonSerializable(typeof(FileDictionary))]
[JsonSerializable(typeof(FileDictionaryEntry))]

// Alias Data
[JsonSerializable(typeof(AliasStore))]
[JsonSerializable(typeof(AliasEntry))]

// Project Enums
[JsonSerializable(typeof(ProjectEnumResource))]
[JsonSerializable(typeof(ProjectEnumEntry))]
[JsonSerializable(typeof(ProjectEnumOption))]

// Format Information
[JsonSerializable(typeof(FormatResource))]
[JsonSerializable(typeof(FormatReference))]
[JsonSerializable(typeof(FormatMember))]

[JsonSerializable(typeof(FormatEnum))]
[JsonSerializable(typeof(FormatEnumEntry))]
[JsonSerializable(typeof(FormatEnumMember))]

[JsonSerializable(typeof(FormatMask))]
[JsonSerializable(typeof(FormatMaskEntry))]
[JsonSerializable(typeof(MaskSection))]

internal partial class ProjectJsonSerializerContext : JsonSerializerContext
{
}
