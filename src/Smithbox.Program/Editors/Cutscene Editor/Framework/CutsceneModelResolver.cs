#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

public enum CutsceneModelKind
{
    Character,
    Asset,
    Object
}

/// <summary>
/// The model an MQB model resource (type 1) shows, from its name: the name is the model's with an instance number
/// after it. c4750_0000 is chr c4750, AEG210_025_8500 is asset aeg210_025, o118110_0000 is obj o118110. Others, such
/// as the dNNNN_NNNN locators, have no model.
/// </summary>
public sealed record CutsceneModel(CutsceneModelKind Kind, string ModelName)
{
    private static readonly Regex Character = new(@"^(c\d{4})_\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Asset = new(@"^(aeg\d{3}_\d{3})_\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Object = new(@"^(o\d{4,6})_\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static CutsceneModel? FromResourceName(string resourceName)
    {
        if (Character.Match(resourceName) is { Success: true } c)
            return new CutsceneModel(CutsceneModelKind.Character, c.Groups[1].Value.ToLowerInvariant());

        if (Asset.Match(resourceName) is { Success: true } a)
            return new CutsceneModel(CutsceneModelKind.Asset, a.Groups[1].Value.ToLowerInvariant());

        if (Object.Match(resourceName) is { Success: true } o)
            return new CutsceneModel(CutsceneModelKind.Object, o.Groups[1].Value.ToLowerInvariant());

        return null;
    }

    /// <summary>The model, and its textures when texturing is on, for ResourcePool.LoadAssets.</summary>
    public List<ResourceDescriptor> GetLoadList(ProjectEntry project)
    {
        var list = new List<ResourceDescriptor>();

        void Add(ResourceDescriptor d)
        {
            if (d.IsValid())
                list.Add(d);
        }

        switch (Kind)
        {
            case CutsceneModelKind.Character:
                Add(ModelLocator.GetChrModel(project, ModelName, ModelName));
                if (CFG.Current.Viewport_Enable_Texturing)
                {
                    Add(TextureLocator.GetCharacterTextureVirtualPath(project, ModelName, false));
                    Add(TextureLocator.GetCharacterTextureVirtualPath(project, ModelName, true));
                }
                break;

            case CutsceneModelKind.Asset:
                Add(ModelLocator.GetObjModel(project, ModelName, ModelName));
                if (CFG.Current.Viewport_Enable_Texturing)
                    Add(TextureLocator.GetAssetTextureVirtualPath(project, ModelName));
                break;

            case CutsceneModelKind.Object:
                Add(ModelLocator.GetObjModel(project, ModelName, ModelName));
                if (CFG.Current.Viewport_Enable_Texturing)
                    Add(TextureLocator.GetObjectTextureVirtualPath(project, ModelName));
                break;
        }

        return list;
    }

    /// <summary>The model's descriptor, whose name the loaded FLVER resource is found by.</summary>
    public ResourceDescriptor GetModelDescriptor(ProjectEntry project) => Kind == CutsceneModelKind.Character
        ? ModelLocator.GetChrModel(project, ModelName, ModelName)
        : ModelLocator.GetObjModel(project, ModelName, ModelName);

    /// <summary>The project path of the binder holding the model, for reading its rig, or null if there is none.</summary>
    public string? FindContainerPath(ProjectEntry project)
    {
        string[] extensions = Kind switch
        {
            CutsceneModelKind.Character => ["chrbnd"],
            CutsceneModelKind.Asset => ["geombnd"],
            _ => ["objbnd"]
        };

        return project.Locator.FileDictionary.Entries
            .FirstOrDefault(e => extensions.Contains(e.Extension)
                                 && e.Filename.Equals(ModelName, StringComparison.OrdinalIgnoreCase))?.Path;
    }
}
