using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Puts a different object in a list slot: a param, sampler or texture reverted to vanilla. Undo puts the old
/// object back, so anything still holding it (the Properties tree) stays valid.
/// </summary>
public class MatReplaceListEntry : EditorAction
{
    private readonly MaterialFileWrapper FileWrapper;
    private readonly IList List;
    private readonly int Index;
    private readonly object NewValue;
    private readonly object OldValue;

    public MatReplaceListEntry(MaterialFileWrapper fileWrapper, IList list, int index, object newValue)
    {
        FileWrapper = fileWrapper;
        List = list;
        Index = index;
        NewValue = newValue;
        OldValue = list[index];
    }

    public override ActionEvent Execute()
    {
        List[Index] = NewValue;
        FileWrapper?.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        List[Index] = OldValue;
        FileWrapper?.MarkUndone();
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Makes a material the same as the game's: every field and list is replaced with a copy of the vanilla file's.
/// The lists are swapped, not edited, so undo gives back the very objects that were there.
/// </summary>
public class MatRevertMaterial : EditorAction
{
    private readonly MaterialFileWrapper FileWrapper;
    private readonly object Material;
    private readonly object Before;
    private readonly object After;

    public MatRevertMaterial(MaterialFileWrapper fileWrapper, object material, object vanillaCopy)
    {
        FileWrapper = fileWrapper;
        Material = material;
        Before = Snapshot(material);
        After = Snapshot(vanillaCopy);
    }

    private record MtdState(string ShaderPath, string Description, List<MTD.Param> Params, List<MTD.Texture> Textures);
    private record MatbinState(string ShaderPath, string SourcePath, uint Key, List<MATBIN.Param> Params, List<MATBIN.Sampler> Samplers);

    private static object Snapshot(object material) => material switch
    {
        MTD m => new MtdState(m.ShaderPath, m.Description, m.Params, m.Textures),
        MATBIN m => new MatbinState(m.ShaderPath, m.SourcePath, m.Key, m.Params, m.Samplers),
        _ => null
    };

    private void Restore(object state)
    {
        switch (Material, state)
        {
            case (MTD m, MtdState s):
                m.ShaderPath = s.ShaderPath;
                m.Description = s.Description;
                m.Params = s.Params;
                m.Textures = s.Textures;
                break;

            case (MATBIN m, MatbinState s):
                m.ShaderPath = s.ShaderPath;
                m.SourcePath = s.SourcePath;
                m.Key = s.Key;
                m.Params = s.Params;
                m.Samplers = s.Samplers;
                break;
        }
    }

    public override ActionEvent Execute()
    {
        Restore(After);
        FileWrapper?.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Restore(Before);
        FileWrapper?.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
