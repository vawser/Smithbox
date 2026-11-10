using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The TPF a texture edit applies to: the container and binder file to mark modified, and the TPF itself.
/// </summary>
public record TextureTarget(BinderContents Binder, BinderFile File, TPF Tpf);

/// <summary>
/// A texture's editable fields. Applying a captured state puts back the same payload array, so undoing every edit of
/// a texture returns it to the bytes it was read with, which the TPF writer then keeps as read (T1).
/// </summary>
public sealed class TextureState
{
    public string Name;
    public byte Format;
    public TPF.TexType Type;
    public byte Mipmaps;
    public byte Flags1;
    public byte[] Bytes;
    public bool HasFloatStruct;
    public int FloatStructUnk00;
    public List<float> FloatStructValues;

    public static TextureState Capture(TPF.Texture tex)
    {
        return new TextureState
        {
            Name = tex.Name,
            Format = tex.Format,
            Type = tex.Type,
            Mipmaps = tex.Mipmaps,
            Flags1 = tex.Flags1,
            Bytes = tex.Bytes,
            HasFloatStruct = tex.FloatStruct != null,
            FloatStructUnk00 = tex.FloatStruct?.Unk00 ?? 0,
            FloatStructValues = tex.FloatStruct?.Values.ToList(),
        };
    }

    public TextureState Clone()
    {
        var clone = (TextureState)MemberwiseClone();
        clone.FloatStructValues = FloatStructValues?.ToList();
        return clone;
    }

    public void Apply(TPF.Texture tex)
    {
        tex.Name = Name;
        tex.Format = Format;
        tex.Type = Type;
        tex.Mipmaps = Mipmaps;
        tex.Flags1 = Flags1;
        tex.Bytes = Bytes;

        if (!HasFloatStruct)
        {
            tex.FloatStruct = null;
        }
        else
        {
            tex.FloatStruct ??= new TPF.FloatStruct();
            tex.FloatStruct.Unk00 = FloatStructUnk00;
            tex.FloatStruct.Values = FloatStructValues.ToList();
        }
    }

    /// <summary>A new texture with this state, for duplicates and imports.</summary>
    public TPF.Texture CreateTexture(TPF.TPFPlatform platform)
    {
        var tex = new TPF.Texture(platform);
        Apply(tex);
        return tex;
    }
}

/// <summary>
/// Changes a texture's fields and/or payload (replace image, rename, format byte, flags, FloatStruct).
/// </summary>
public class TextureChangeAction : EditorAction
{
    private readonly TexEditorView View;
    private readonly TextureTarget Target;
    private readonly TPF.Texture Texture;
    private readonly TextureState Before;
    private readonly TextureState After;

    public TextureChangeAction(TexEditorView view, TextureTarget target, TPF.Texture texture, TextureState before, TextureState after)
    {
        View = view;
        Target = target;
        Texture = texture;
        Before = before;
        After = after;
    }

    public override ActionEvent Execute()
    {
        After.Apply(Texture);
        Target.Binder.MarkModified(Target.File);
        View.OnTextureChanged(Target.Tpf, Texture);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Apply(Texture);
        Target.Binder.MarkModified(Target.File);
        View.OnTextureChanged(Target.Tpf, Texture);
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Adds a texture to a TPF at an index, or removes it (add / duplicate / import as new, delete).
/// </summary>
public class TextureInsertRemoveAction : EditorAction
{
    private readonly TexEditorView View;
    private readonly TextureTarget Target;
    private readonly TPF.Texture Texture;
    private readonly int Index;
    private readonly bool Insert;

    public TextureInsertRemoveAction(TexEditorView view, TextureTarget target, TPF.Texture texture, int index, bool insert)
    {
        View = view;
        Target = target;
        Texture = texture;
        Index = index;
        Insert = insert;
    }

    public string TextureName => Texture.Name;

    public override ActionEvent Execute()
    {
        Apply(Insert);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(!Insert);
        return ActionEvent.NoEvent;
    }

    private void Apply(bool insert)
    {
        var textures = Target.Tpf.Textures;

        if (insert)
        {
            textures.Insert(System.Math.Clamp(Index, 0, textures.Count), Texture);
            Target.Binder.MarkModified(Target.File);
            View.OnTextureListChanged(Target.Tpf, Texture);
        }
        else
        {
            textures.Remove(Texture);
            Target.Binder.MarkModified(Target.File);
            View.OnTextureListChanged(Target.Tpf, textures.Count > 0 ? textures[System.Math.Clamp(Index, 0, textures.Count - 1)] : null);
        }
    }
}
