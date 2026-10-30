using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
///     Chooses what a player-like character is wearing while it is displayed. Nothing here is saved.
/// </summary>
public class ModelCharacterPreview
{
    public ModelEditorView View;
    public ProjectEntry Project;

    /// <summary>
    ///     The CharaInitParam row characters are dressed from. Anything negative leaves them with a bare body.
    /// </summary>
    public int SelectedCharaInitId { get; private set; } = -1;

    private string _filter = "";

    public ModelCharacterPreview(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_CharacterPreview_Header")}##modelCharacterPreviewHeader"))
        {
            ImGui.BeginChild("ModelCharacterPreviewSection");
            Display();
            ImGui.EndChild();
        }
    }

    private void Display()
    {
        ModelWrapper wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper?.Container == null || !CharacterComposition.IsComposite(wrapper.Name))
        {
            GUI.WrappedText($"Load {CharacterComposition.CompositeModel} to preview its equipment.");
            return;
        }

        ModelContainer container = wrapper.Container;

        if (container.CharacterAppearance is { Parts.Count: > 0 } appearance)
        {
            GUI.SimpleHeader("Parts", "The models that are currently displayed.");

            foreach (CharacterPart part in appearance.Parts)
            {
                GUI.WrappedText(part.Name);
            }

            GUI.Spacer();
        }
        else
        {
            GUI.WrappedText("This game has no parts to display.");
        }

        GUI.SimpleHeader("Equipment", "Click a row of CharaInitParam to dress the character in it.");

        Param charaInit = null;
        Project.Handler.ParamData?.PrimaryBank?.Params?.TryGetValue("CharaInitParam", out charaInit);

        if (charaInit == null)
        {
            GUI.WrappedText("CharaInitParam is not available.");
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##characterPreviewFilter", "Filter...", ref _filter, 255);

        if (ImGui.BeginChild("characterPreviewList", default, ImGuiChildFlags.Borders))
        {
            if (ImGui.Selectable("Bare body##characterPreviewBare", SelectedCharaInitId < 0))
            {
                Select(container, -1);
            }

            foreach (Param.Row row in charaInit.Rows)
            {
                var label = $"[{row.ID}] {row.Name}";

                if (_filter.Length > 0 && !label.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ImGui.Selectable($"{label}##characterPreviewRow{row.ID}", row.ID == SelectedCharaInitId))
                {
                    Select(container, row.ID);
                }
            }
        }

        ImGui.EndChild();
    }

    private void Select(ModelContainer container, int charaInitId)
    {
        SelectedCharaInitId = charaInitId;

        container.AssignCharacterParts(CharacterComposition.Resolve(Project, charaInitId));

        View.Universe.ScheduleTextureRefresh();
    }
}
