using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class AliasStore : Dictionary<ProjectAliasType, List<AliasEntry>>;

public class AliasEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
}