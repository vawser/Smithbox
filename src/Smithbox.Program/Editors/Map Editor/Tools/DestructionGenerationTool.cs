using Hexa.NET.ImGui;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Tool window section for generating the break data (BREAKOBJ/BREAKGEOM) of maps from their saved MSBs.
/// </summary>
public class DestructionGenerationTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    public DestructionGenerationTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse() => Project.Handler.MapData?.BreakDataGenerator?.CanUse() == true;

    /// <summary>
    /// ER and NR break assets; the older games break objects.
    /// </summary>
    public string Title => Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR
        ? "Asset Destruction Generation"
        : "Object Destruction Generation";

    private string PartType => Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR ? "asset" : "object";

    public void OnToolWindow()
    {
        ImGui.BeginChild("destructionGenerationSection", ImGuiChildFlags.Borders);

        if (!CanUse())
        {
            GUI.WrappedText("This project type has no break data.");
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText($"Rebuild the break data of maps from their saved MSBs. Each breakable {PartType} unchanged from vanilla keeps its vanilla break data, and each new one gets a copy from a vanilla instance of its model.");

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DS2 or ProjectType.DS2S)
        {
            GUI.WrappedText($"New {PartType}s need an Instance ID not used by another {PartType} of the same model, or they share its break data.");
        }

        GUI.WrappedText("Save the maps first: unsaved changes are not included.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("destructionGenerateButtons",
            "destructionGenerateLoadedButton", "Generate for Loaded Maps",
            "Rebuild the break data of the loaded maps.",
            () => View.ActionHandler.GenerateBreakData(loadedMapsOnly: true),
            "destructionGenerateAllButton", "Generate for All Maps",
            "Rebuild the break data of every map in the project. Only files that change are written.",
            () => View.ActionHandler.GenerateBreakData(loadedMapsOnly: false));

        ImGui.EndChild();
    }
}
