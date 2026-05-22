using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public static class MsbEditorUtils
{
    public static bool IsPart(object entry)
    {
        if(entry is MSB1.Part ||
            entry is MSB2.Part ||
            entry is MSB3.Part ||
            entry is MSBB.Part ||
            entry is MSBD.Part ||
            entry is MSBE.Part ||
            entry is MSBS.Part ||
            entry is MSB_AC6.Part ||
            entry is MSB_NR.Part)
        {
            return true;
        }

        return false;
    }

    public static bool IsRegion(object entry)
    {
        if (entry is MSB1.Region ||
            entry is MSB2.Region ||
            entry is MSB3.Region ||
            entry is MSBB.Region ||
            entry is MSBD.Region ||
            entry is MSBE.Region ||
            entry is MSBS.Region ||
            entry is MSB_AC6.Region ||
            entry is MSB_NR.Region)
        {
            return true;
        }

        return false;
    }

    public static bool IsEvent(object entry)
    {
        if (entry is MSB1.Event ||
            entry is MSB2.Event ||
            entry is MSB3.Event ||
            entry is MSBB.Event ||
            entry is MSBD.Event ||
            entry is MSBE.Event ||
            entry is MSBS.Event ||
            entry is MSB_AC6.Event ||
            entry is MSB_NR.Event)
        {
            return true;
        }

        return false;
    }

    public static bool IsLight(object entry)
    {
        if (entry is BTL.Light)
        {
            return true;
        }

        return false;
    }

    public static bool IsEnemy(object entry)
    {
        if (entry is MSB1.Part.Enemy ||
            entry is MSB3.Part.Enemy ||
            entry is MSBB.Part.Enemy ||
            entry is MSBD.Part.Enemy ||
            entry is MSBE.Part.Enemy ||
            entry is MSBS.Part.Enemy ||
            entry is MSB_AC6.Part.Enemy ||
            entry is MSB_NR.Part.Enemy)
        {
            return true;
        }

        return false;
    }

    public static bool IsDummyEnemy(object entry)
    {
        if (entry is MSB1.Part.DummyEnemy ||
            entry is MSB3.Part.DummyEnemy ||
            entry is MSBB.Part.DummyEnemy ||
            entry is MSBD.Part.DummyEnemy ||
            entry is MSBE.Part.DummyEnemy ||
            entry is MSBS.Part.DummyEnemy ||
            entry is MSB_AC6.Part.DummyEnemy ||
            entry is MSB_NR.Part.DummyEnemy)
        {
            return true;
        }

        return false;
    }

    public static bool IsAsset(object entry)
    {
        if (entry is MSB1.Part.Object ||
            entry is MSB2.Part.Object ||
            entry is MSB3.Part.Object ||
            entry is MSBB.Part.Object ||
            entry is MSBD.Part.Object ||
            entry is MSBE.Part.Asset ||
            entry is MSBS.Part.Object ||
            entry is MSB_AC6.Part.Asset ||
            entry is MSB_NR.Part.Asset)
        {
            return true;
        }

        return false;
    }

    public static bool IsDummyAsset(object entry)
    {
        if (entry is MSB1.Part.DummyObject ||
            entry is MSB3.Part.DummyObject ||
            entry is MSBB.Part.DummyObject ||
            entry is MSBD.Part.DummyObject ||
            entry is MSBE.Part.DummyAsset ||
            entry is MSBS.Part.DummyObject ||
            entry is MSB_AC6.Part.DummyAsset ||
            entry is MSB_NR.Part.DummyAsset)
        {
            return true;
        }

        return false;
    }

    public static bool IsGeom(object entry)
    {
        if (entry is MSBE.Part.Asset ||
            entry is MSB_AC6.Part.Asset ||
            entry is MSB_NR.Part.Asset)
        {
            return true;
        }

        return false;
    }

    public static bool IsMapPiece(object entry)
    {
        if (entry is MSB1.Part.MapPiece ||
            entry is MSB2.Part.MapPiece ||
            entry is MSB3.Part.MapPiece ||
            entry is MSBB.Part.MapPiece ||
            entry is MSBD.Part.MapPiece ||
            entry is MSBE.Part.MapPiece ||
            entry is MSBS.Part.MapPiece ||
            entry is MSB_AC6.Part.MapPiece ||
            entry is MSB_NR.Part.MapPiece)
        {
            return true;
        }

        return false;
    }

    public static bool IsCollision(object entry)
    {
        if (entry is MSB1.Part.Collision ||
            entry is MSB2.Part.Collision ||
            entry is MSB3.Part.Collision ||
            entry is MSBB.Part.Collision ||
            entry is MSBD.Part.Collision ||
            entry is MSBE.Part.Collision ||
            entry is MSBS.Part.Collision ||
            entry is MSB_AC6.Part.Collision ||
            entry is MSB_NR.Part.Collision)
        {
            return true;
        }

        return false;
    }

    public static bool IsConnectCollision(object entry)
    {
        if (entry is MSB1.Part.ConnectCollision ||
            entry is MSB2.Part.ConnectCollision ||
            entry is MSB3.Part.ConnectCollision ||
            entry is MSBB.Part.ConnectCollision ||
            entry is MSBD.Part.ConnectCollision ||
            entry is MSBE.Part.ConnectCollision ||
            entry is MSBS.Part.ConnectCollision ||
            entry is MSB_AC6.Part.ConnectCollision ||
            entry is MSB_NR.Part.ConnectCollision)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayer(object entry)
    {
        if (entry is MSB1.Part.Player ||
            entry is MSB3.Part.Player ||
            entry is MSBB.Part.Player ||
            entry is MSBD.Part.Player ||
            entry is MSBE.Part.Player ||
            entry is MSBS.Part.Player ||
            entry is MSB_AC6.Part.Player ||
            entry is MSB_NR.Part.Player)
        {
            return true;
        }

        return false;
    }

    public static bool IsTreasure(object entry)
    {
        if (entry is MSB1.Event.Treasure ||
            entry is MSB3.Event.Treasure ||
            entry is MSBB.Event.Treasure ||
            entry is MSBD.Event.Treasure ||
            entry is MSBE.Event.Treasure ||
            entry is MSBS.Event.Treasure ||
            entry is MSB_AC6.Event.Treasure ||
            entry is MSB_NR.Event.Treasure)
        {
            return true;
        }

        return false;
    }

    public static bool IsObjAct(object entry)
    {
        if (entry is MSB1.Event.ObjAct ||
            entry is MSB3.Event.ObjAct ||
            entry is MSBB.Event.ObjAct ||
            entry is MSBE.Event.ObjAct ||
            entry is MSBS.Event.ObjAct ||
            entry is MSB_NR.Event.ObjAct)
        {
            return true;
        }

        return false;
    }
}
