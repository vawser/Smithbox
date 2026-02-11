using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

/// <summary>
/// Entity used to serialize a map.
/// </summary>
public class MapSerializationEntity
{
    public string Name { get; set; }
    public int Msbidx { get; set; } = -1;
    public MsbEntityType Type { get; set; }
    public Transform Transform { get; set; }
    public List<MapSerializationEntity> Children { get; set; }

    public bool ShouldSerializeChildren()
    {
        return Children != null && Children.Count > 0;
    }
}
