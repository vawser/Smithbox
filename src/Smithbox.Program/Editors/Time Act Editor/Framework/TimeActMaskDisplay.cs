#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Shows and hides the meshes of the model by the masks the events set (<see cref="TimeActSimulation.MaskOverrides" />).
/// Nothing is stored: each frame a mesh is shown as the events say, or as the character has it where they say nothing.
/// For an enemy that is what the Model Mask Toggler left it at; for a player, what it wears, whose hidden meshes are
/// left out of the model altogether, so an event can only hide those and show them again.
/// </summary>
public class TimeActMaskDisplay
{
    private readonly TimeActEditorView _view;

    public TimeActMaskDisplay(TimeActEditorView view)
    {
        _view = view;
    }

    public void Update(bool?[] overrides)
    {
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        if (container == null)
            return;

        if (container.CharacterMesh is { } characterMesh)
        {
            var parts = container.CharacterAppearance?.Parts;

            for (var i = 0; i < characterMesh.Submeshes.Count; i++)
            {
                // The masks are for the body and armor, and mean something else on a weapon
                if (parts != null && i < parts.Count && parts[i].AttachNode != null)
                    continue;

                var part = characterMesh.Submeshes[i];

                ApplyToChildren(part, part.Visible, overrides);
            }

            return;
        }

        foreach (Entity mesh in container.Meshes)
        {
            if (mesh.RenderSceneMesh is MeshRenderableProxy proxy)
            {
                Apply(proxy, mesh.EditorVisible, overrides);
            }
        }
    }

    private static void Apply(MeshRenderableProxy proxy, bool visible, bool?[] overrides)
    {
        if (proxy.Submeshes.Count > 0)
        {
            ApplyToChildren(proxy, visible, overrides);
            return;
        }

        if (proxy._meshProvider is not FlverSubmeshProvider submesh)
            return;

        var mask = submesh.MaterialMask;
        var desired = mask >= 0 && mask < overrides.Length && overrides[mask] is { } shown ? shown : visible;

        // Only on a change, as setting it goes through the renderables of the mesh
        if (proxy.Visible != desired)
        {
            proxy.Visible = desired;
        }
    }

    private static void ApplyToChildren(MeshRenderableProxy proxy, bool visible, bool?[] overrides)
    {
        IReadOnlyList<MeshRenderableProxy> children = proxy.Submeshes;

        for (var i = 0; i < children.Count; i++)
        {
            Apply(children[i], visible, overrides);
        }
    }
}
