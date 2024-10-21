using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// A wrapper class that holds the information relating to the Text FMG file.
/// </summary>
public class TextFmgWrapper : IComparable<TextFmgWrapper>
{
    /// <summary>
    /// Binder ID assigned to this FMG file in the container.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Name of this FMG file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// FMG object for this FMG file.
    /// </summary>
    public FMG File { get; set; }

    /// <summary>
    /// The associated container wrapper this FMG wrapper belongs to.
    /// </summary>
    public TextContainerWrapper Parent { get; set; }

    public TextFmgWrapper()
    {

    }

    public int CompareTo(TextFmgWrapper other)
    {
        if (ID < other.ID)
            return -1;

        if (ID > other.ID)
            return 1;

        return 0;
    }
}