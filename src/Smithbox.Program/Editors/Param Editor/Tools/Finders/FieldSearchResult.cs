using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FieldSearchResult : IComparable<DataSearchResult>
{
    public string ParamName;
    public string FieldInternalName;

    public FieldSearchResult() { }

    public int CompareTo(DataSearchResult other)
    {
        return this.ParamName.CompareTo(other.ParamName);
    }
}
