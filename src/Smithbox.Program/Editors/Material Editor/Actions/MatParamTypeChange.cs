using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Changes the type of an MTD or MATBIN param. The SoulsFormats Type setters replace Value with a default
/// of the new type, so the value is kept with the type to bring it back on undo.
/// </summary>
public class MatParamTypeChange : EditorAction
{
    private readonly object Param;
    private readonly MaterialFileWrapper FileWrapper;

    private readonly object OldType;
    private readonly object OldValue;
    private readonly object NewType;

    // The default the setter makes, kept so redo gives back the same object undo saw
    private object NewValue;

    public MatParamTypeChange(MaterialFileWrapper fileWrapper, object param, object newType)
    {
        FileWrapper = fileWrapper;
        Param = param;
        NewType = newType;

        (OldType, OldValue) = Read();
    }

    /// <summary>
    /// Whether this is a param type change that needs this action instead of a plain property change
    /// </summary>
    public static bool Handles(object obj, string propName)
    {
        return propName == "Type" && obj is MTD.Param or MATBIN.Param;
    }

    public override ActionEvent Execute()
    {
        if (NewValue == null)
        {
            Write(NewType, null);
            NewValue = Read().value;
        }
        else
        {
            Write(NewType, NewValue);
        }

        if (FileWrapper != null)
            FileWrapper.MarkChanged();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Write(OldType, OldValue);

        if (FileWrapper != null)
            FileWrapper.MarkUndone();

        return ActionEvent.NoEvent;
    }

    private (object type, object value) Read()
    {
        return Param switch
        {
            MTD.Param p => (p.Type, p.Value),
            MATBIN.Param p => (p.Type, p.Value),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Sets the type, then the value over the setter's default. A null value keeps the default.
    /// </summary>
    private void Write(object type, object value)
    {
        switch (Param)
        {
            case MTD.Param p:
                p.Type = (MTD.ParamType)type;
                if (value != null)
                    p.Value = value;
                break;

            case MATBIN.Param p:
                p.Type = (MATBIN.ParamType)type;
                if (value != null)
                    p.Value = value;
                break;
        }
    }
}
