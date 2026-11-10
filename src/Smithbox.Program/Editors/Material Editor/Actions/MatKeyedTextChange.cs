using StudioCore.Editors.Common;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Changes the text a MATBIN key is made from (MATBIN.SourcePath, Param.Name, Sampler.Type) and the key with it,
/// so the key never goes stale. Undo restores both as they were, even a key that didn't match before.
/// </summary>
public class MatKeyedTextChange : EditorAction
{
    private readonly object Target;
    private readonly PropertyInfo TextProperty;
    private readonly PropertyInfo KeyProperty;
    private readonly MaterialFileWrapper FileWrapper;

    private readonly object OldText;
    private readonly object OldKey;
    private readonly string NewText;

    public MatKeyedTextChange(MaterialFileWrapper fileWrapper, object target, PropertyInfo textProperty, string newText)
    {
        FileWrapper = fileWrapper;
        Target = target;
        TextProperty = textProperty;
        KeyProperty = target.GetType().GetProperty("Key");
        NewText = newText;

        OldText = TextProperty.GetValue(Target);
        OldKey = KeyProperty?.GetValue(Target);
    }

    /// <summary>
    /// Whether this property is the text a key is made from
    /// </summary>
    public static bool Handles(object obj, string propName)
    {
        return MaterialUtils.KeySourceProperty(obj) == propName;
    }

    public override ActionEvent Execute()
    {
        TextProperty.SetValue(Target, NewText);
        MaterialUtils.ApplyMatbinKey(Target);

        if (FileWrapper != null)
            FileWrapper.MarkChanged();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TextProperty.SetValue(Target, OldText);
        KeyProperty?.SetValue(Target, OldKey);

        if (FileWrapper != null)
            FileWrapper.MarkUndone();

        return ActionEvent.NoEvent;
    }
}
