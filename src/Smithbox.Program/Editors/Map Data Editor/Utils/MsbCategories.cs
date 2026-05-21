using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MsbCategories
{
    public Dictionary<ProjectType, Dictionary<string, Type>> BaseCategories { get; set; } = new()
    {
        [ProjectType.DES] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB1.ModelParam),
            ["Event"] = typeof(MSB1.EventParam),
            ["Region"] = typeof(MSB1.PointParam),
            ["Part"] = typeof(MSB1.PartsParam)
        }
    };

    public Dictionary<ProjectType, Dictionary<string, Dictionary<string, Type>>> SubCategories { get; set; } = new()
    {
        [ProjectType.DES] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Model.MapPiece),
                ["Object"] = typeof(MSB1.Model.Object),
                ["Enemy"] = typeof(MSB1.Model.Enemy),
                ["Player"] = typeof(MSB1.Model.Player),
                ["Collision"] = typeof(MSB1.Model.Collision),
                ["Navmesh"] = typeof(MSB1.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSB1.Event.Light),
                ["Sound"] = typeof(MSB1.Event.Sound),
                ["SFX"] = typeof(MSB1.Event.SFX),
                ["Wind"] = typeof(MSB1.Event.Wind),
                ["Treasure"] = typeof(MSB1.Event.Treasure),
                ["Generator"] = typeof(MSB1.Event.Generator),
                ["Message"] = typeof(MSB1.Event.Message),
                ["ObjAct"] = typeof(MSB1.Event.ObjAct),
                ["SpawnPoint"] = typeof(MSB1.Event.SpawnPoint),
                ["MapOffset"] = typeof(MSB1.Event.MapOffset),
                ["Navmesh"] = typeof(MSB1.Event.Navmesh),
                ["Environment"] = typeof(MSB1.Event.Environment),
                ["PseudoMultiplayer"] = typeof(MSB1.Event.PseudoMultiplayer)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region"] = typeof(MSB1.Region)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Part.MapPiece),
                ["Object"] = typeof(MSB1.Part.Object),
                ["Enemy"] = typeof(MSB1.Part.Enemy),
                ["Player"] = typeof(MSB1.Part.Player),
                ["Collision"] = typeof(MSB1.Part.Collision),
                ["Navmesh"] = typeof(MSB1.Part.Navmesh),
                ["DummyObject"] = typeof(MSB1.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSB1.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB1.Part.ConnectCollision)
            }
        }
    };
}
