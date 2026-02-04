using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class DataSearchResult : IComparable<DataSearchResult>
{
    public string ParamName;
    public string RowName;
    public string FieldInternalName;
    public string FieldDisplayName;
    public string FieldValue;

    public int RowID;
    public int RowIndex;

    public string AliasName;
    public string AliasID;
    public string AliasDisplayName;

    public DataSearchResult() { }

    public int CompareTo(DataSearchResult other)
    {
        return this.ParamName.CompareTo(other.ParamName);
    }
}
