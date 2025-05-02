using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.JSON;

public class EsdMeta_Root
{
    public List<EsdMeta_Command> commands { get; set; }
    public List<EsdMeta_Function> functions { get; set; }
    public List<EsdMeta_Enum> enums { get; set; }
}

public class EsdMeta_Command
{
    public long id { get; set; }
    public long bank { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_Arg> args { get; set; }
}

public class EsdMeta_Function
{
    public long id { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_Arg> args { get; set; }
}

public class EsdMeta_Arg
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
    public string argLink { get; set; }
    public string argEnum { get; set; }
}

public class EsdMeta_Enum
{
    public string referenceName { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_EnumMember> members { get; set; }
}

public class EsdMeta_EnumMember
{
    public string identifier { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
}
