using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextContainerInfo : IComparable<TextContainerInfo>
{
    public string Name { get; private set; }
    public string AbsolutePath { get; private set; }

    public bool IsModified { get; set; }
    public bool HasUnsavedChanges { get; set; }

    public DCX.Type CompressionType { get; private set; }

    public TextContainerType ContainerType { get; private set; }

    public TextContainerCategory Category { get; private set; }

    public List<FmgInfo> FmgInfos { get; private set; }

    public TextContainerInfo(string name, string path, DCX.Type compressionType, TextContainerType containerType, TextContainerCategory category, List<FmgInfo> fmgInfos)
    {
        Name = name;
        AbsolutePath = path;
        IsModified = false;
        CompressionType = compressionType;
        ContainerType = containerType;
        Category = category;
        FmgInfos = fmgInfos;
        HasUnsavedChanges = false;
    }

    public int CompareTo(TextContainerInfo other)
    {
        return string.Compare(Name, other.Name);
    }
}