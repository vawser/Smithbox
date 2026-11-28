#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The open character's model as skinned interchange meshes for an FBX export (Animation Editor Analysis.md §14, F5):
/// its own FLVER from its container (chrbnd, objbnd, asset geombnd), or for a composite character (c0000) the skinned
/// parts of its bare body, as the viewport shows it. Weapons and parts in containers of their own are left out.
/// </summary>
public static class AnimationMeshSource
{
    public static List<InterchangeMesh> Load(ProjectEntry project, AnimationCharacter character,
        FlverInterchange.Report report, List<string> notes)
    {
        var meshes = new List<InterchangeMesh>();
        var game = project.Descriptor.ProjectType;
        var fs = project.VFS.FS;

        void Add(string? container, string modelName)
        {
            if (container == null || !fs.FileExists(container) || fs.ReadFile(container) is not { } data)
            {
                notes.Add($"{modelName}: no model file");
                return;
            }

            switch (ModelRig.ReadModel(game, data, modelName))
            {
                case FLVER2 flver:
                    meshes.AddRange(FlverInterchange.Meshes(flver, modelName, character.Skeleton, report));
                    break;
                case FLVER0:
                    notes.Add($"{modelName}: FLVER0 models (Demon's Souls) aren't exported yet");
                    break;
                default:
                    notes.Add($"{modelName}: no model in {container}");
                    break;
            }
        }

        if (character.Kind == AnimatedModelKind.Character && CharacterComposition.IsComposite(character.Name))
        {
            // The bare body's parts, skinned to the character's skeleton (weapons follow a bone as a whole: left out)
            var appearance = CharacterComposition.Resolve(project);
            foreach (var part in appearance.Parts)
            {
                if (part.Container != null || part.AttachNode != null)
                    continue;

                var asset = ModelLocator.GetPartsModel(project, part.Name, part.Name);
                Add(asset.AssetVirtualPath != null ? PathBuilder.GetRelativePath(project, asset.AssetVirtualPath) : null, part.Name);
            }
        }
        else
        {
            Add(character.ContainerPath, character.Name);
        }

        return meshes;
    }
}
