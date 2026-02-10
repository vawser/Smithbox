using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

public static class EntityHelper
{
    /// <summary>
    /// Returns true if this entity is a Part
    /// </summary>
    public static bool IsPart(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part ||
            ent.WrappedObject is MSB2.Part ||
            ent.WrappedObject is MSB3.Part ||
            ent.WrappedObject is MSBB.Part ||
            ent.WrappedObject is MSBD.Part ||
            ent.WrappedObject is MSBE.Part ||
            ent.WrappedObject is MSBS.Part ||
            ent.WrappedObject is MSB_AC6.Part ||
            ent.WrappedObject is MSB_NR.Part ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Region
    /// </summary>
    public static bool IsRegion(Entity ent)
    {
        return ent.WrappedObject is MSB1.Region ||
            ent.WrappedObject is MSB2.Region ||
            ent.WrappedObject is MSB3.Region ||
            ent.WrappedObject is MSBB.Region ||
            ent.WrappedObject is MSBD.Region ||
            ent.WrappedObject is MSBE.Region ||
            ent.WrappedObject is MSBS.Region ||
            ent.WrappedObject is MSB_AC6.Region ||
            ent.WrappedObject is MSB_NR.Region ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Event
    /// </summary>
    public static bool IsEvent(Entity ent)
    {
        return ent.WrappedObject is MSB1.Event ||
            ent.WrappedObject is MSB2.Event ||
            ent.WrappedObject is MSB3.Event ||
            ent.WrappedObject is MSBB.Event ||
            ent.WrappedObject is MSBD.Event ||
            ent.WrappedObject is MSBE.Event ||
            ent.WrappedObject is MSBS.Event ||
            ent.WrappedObject is MSB_AC6.Event ||
            ent.WrappedObject is MSB_NR.Event ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Light
    /// </summary>
    public static bool IsLight(Entity ent)
    {
        return ent.WrappedObject is BTL.Light ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Enemy
    /// </summary>
    public static bool IsPartEnemy(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.Enemy ||
            ent.WrappedObject is MSB3.Part.Enemy ||
            ent.WrappedObject is MSBB.Part.Enemy ||
            ent.WrappedObject is MSBD.Part.Enemy ||
            ent.WrappedObject is MSBE.Part.Enemy ||
            ent.WrappedObject is MSBS.Part.Enemy ||
            ent.WrappedObject is MSB_AC6.Part.Enemy ||
            ent.WrappedObject is MSB_NR.Part.Enemy ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an DummyEnemy
    /// </summary>
    public static bool IsPartDummyEnemy(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.DummyEnemy ||
            ent.WrappedObject is MSB3.Part.DummyEnemy ||
            ent.WrappedObject is MSBB.Part.DummyEnemy ||
            ent.WrappedObject is MSBD.Part.DummyEnemy ||
            ent.WrappedObject is MSBE.Part.DummyEnemy ||
            ent.WrappedObject is MSBS.Part.DummyEnemy ||
            ent.WrappedObject is MSB_AC6.Part.DummyEnemy ||
            ent.WrappedObject is MSB_NR.Part.DummyEnemy ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Asset
    /// </summary>
    public static bool IsPartAsset(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.Object ||
            ent.WrappedObject is MSB2.Part.Object ||
            ent.WrappedObject is MSB3.Part.Object ||
            ent.WrappedObject is MSBB.Part.Object ||
            ent.WrappedObject is MSBD.Part.Object ||
            ent.WrappedObject is MSBE.Part.Asset ||
            ent.WrappedObject is MSBS.Part.Object ||
            ent.WrappedObject is MSB_AC6.Part.Asset ||
            ent.WrappedObject is MSB_NR.Part.Asset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an actual Asset
    /// </summary>
    public static bool IsPartPureAsset(Entity ent)
    {
        return ent.WrappedObject is MSBE.Part.Asset ||
            ent.WrappedObject is MSB_AC6.Part.Asset ||
            ent.WrappedObject is MSB_NR.Part.Asset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an DummyAsset
    /// </summary>
    public static bool IsPartDummyAsset(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.DummyObject ||
            ent.WrappedObject is MSB3.Part.DummyObject ||
            ent.WrappedObject is MSBB.Part.DummyObject ||
            ent.WrappedObject is MSBD.Part.DummyObject ||
            ent.WrappedObject is MSBE.Part.DummyAsset ||
            ent.WrappedObject is MSBS.Part.DummyObject ||
            ent.WrappedObject is MSB_AC6.Part.DummyAsset ||
            ent.WrappedObject is MSB_NR.Part.DummyAsset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an MapPiece
    /// </summary>
    public static bool IsPartMapPiece(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.MapPiece ||
            ent.WrappedObject is MSB2.Part.MapPiece ||
            ent.WrappedObject is MSB3.Part.MapPiece ||
            ent.WrappedObject is MSBB.Part.MapPiece ||
            ent.WrappedObject is MSBD.Part.MapPiece ||
            ent.WrappedObject is MSBE.Part.MapPiece ||
            ent.WrappedObject is MSBS.Part.MapPiece ||
            ent.WrappedObject is MSB_AC6.Part.MapPiece ||
            ent.WrappedObject is MSB_NR.Part.MapPiece ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Collision
    /// </summary>
    public static bool IsPartCollision(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.Collision ||
            ent.WrappedObject is MSB2.Part.Collision ||
            ent.WrappedObject is MSB3.Part.Collision ||
            ent.WrappedObject is MSBB.Part.Collision ||
            ent.WrappedObject is MSBD.Part.Collision ||
            ent.WrappedObject is MSBE.Part.Collision ||
            ent.WrappedObject is MSBS.Part.Collision ||
            ent.WrappedObject is MSB_AC6.Part.Collision ||
            ent.WrappedObject is MSB_NR.Part.Collision ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an ConnectCollision
    /// </summary>
    public static bool IsPartConnectCollision(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.ConnectCollision ||
            ent.WrappedObject is MSB2.Part.ConnectCollision ||
            ent.WrappedObject is MSB3.Part.ConnectCollision ||
            ent.WrappedObject is MSBB.Part.ConnectCollision ||
            ent.WrappedObject is MSBD.Part.ConnectCollision ||
            ent.WrappedObject is MSBE.Part.ConnectCollision ||
            ent.WrappedObject is MSBS.Part.ConnectCollision ||
            ent.WrappedObject is MSB_AC6.Part.ConnectCollision ||
            ent.WrappedObject is MSB_NR.Part.ConnectCollision ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Player
    /// </summary>
    public static bool IsPartPlayer(Entity ent)
    {
        return ent.WrappedObject is MSB1.Part.Player ||
            ent.WrappedObject is MSB3.Part.Player ||
            ent.WrappedObject is MSBB.Part.Player ||
            ent.WrappedObject is MSBD.Part.Player ||
            ent.WrappedObject is MSBE.Part.Player ||
            ent.WrappedObject is MSBS.Part.Player ||
            ent.WrappedObject is MSB_AC6.Part.Player ||
            ent.WrappedObject is MSB_NR.Part.Player ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Treasure
    /// </summary>
    public static bool IsEventTreasure(Entity ent)
    {
        return ent.WrappedObject is MSB1.Event.Treasure ||
            ent.WrappedObject is MSB3.Event.Treasure ||
            ent.WrappedObject is MSBB.Event.Treasure ||
            ent.WrappedObject is MSBD.Event.Treasure ||
            ent.WrappedObject is MSBE.Event.Treasure ||
            ent.WrappedObject is MSBS.Event.Treasure ||
            ent.WrappedObject is MSB_AC6.Event.Treasure ||
            ent.WrappedObject is MSB_NR.Event.Treasure ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an ObjAct
    /// </summary>
    public static bool IsEventObjAct(Entity ent)
    {
        return ent.WrappedObject is MSB1.Event.ObjAct ||
            ent.WrappedObject is MSB3.Event.ObjAct ||
            ent.WrappedObject is MSBB.Event.ObjAct ||
            ent.WrappedObject is MSBE.Event.ObjAct ||
            ent.WrappedObject is MSBS.Event.ObjAct ||
            ent.WrappedObject is MSB_NR.Event.ObjAct ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an PatrolInfo
    /// </summary>
    public static bool IsEventPatrolInfo(Entity ent)
    {
        return ent.WrappedObject is MSB3.Event.PatrolInfo ||
            ent.WrappedObject is MSBB.Event.PatrolInfo ||
            ent.WrappedObject is MSBE.Event.PatrolInfo ||
            ent.WrappedObject is MSBS.Event.PatrolInfo ||
            ent.WrappedObject is MSB_NR.Event.PatrolInfo ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Mount
    /// </summary>
    public static bool IsEventMount(Entity ent)
    {
        return ent.WrappedObject is MSBE.Event.Mount ||
            ent.WrappedObject is MSB_NR.Event.Riding ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Connection
    /// </summary>
    public static bool IsRegionConnection(Entity ent)
    {
        return ent.WrappedObject is MSBE.Region.Connection ||
            ent.WrappedObject is MSB_NR.Region.MapConnection ? true : false;
    }

}
