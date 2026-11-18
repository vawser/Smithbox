#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Renderer;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Fades the model by the opacity the events give (<see cref="TimeActSimulation.Opacity" />): the whole character,
/// with what it wears and holds. Nothing is stored: each frame every mesh is given the opacity, which is 1 when no
/// event says otherwise.
/// </summary>
public class TimeActOpacityDisplay
{
    private readonly TimeActEditorView _view;

    public TimeActOpacityDisplay(TimeActEditorView view)
    {
        _view = view;
    }

    public void Update(float opacity)
    {
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        if (container == null)
            return;

        if (container.CharacterMesh is { } characterMesh)
        {
            Apply(characterMesh, opacity);
        }

        foreach (Entity mesh in container.Meshes)
        {
            if (mesh.RenderSceneMesh is MeshRenderableProxy proxy)
            {
                Apply(proxy, opacity);
            }
        }
    }

    // Only on a change, as going below 1 or back rebuilds the pipelines of the mesh
    private static void Apply(MeshRenderableProxy proxy, float opacity)
    {
        if (proxy.Opacity != opacity)
        {
            proxy.Opacity = opacity;
        }
    }
}
