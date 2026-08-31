using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using static SoulsFormats.FLVER.Node;
namespace StudioCore.Application;

public class FlverDummyList : Dictionary<string, List<FlverDummyRepresentation>>;

public class FlverDummyRepresentation
{
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }
    public Vector3 Upward { get; set; }
    public short ReferenceID { get; set; }
    public short AttachBoneIndex { get; set; }
    public Color Color { get; set; }
    public bool FollowAttachBone { get; set; }
    public bool UseUpwardVector { get; set; }
    public int Unk30 { get; set; }
    public int Unk34 { get; set; }
}

public class FlverNodeList : Dictionary<string, List<FlverNodeRepresentation>>;

public class FlverNodeRepresentation
{
    public string Name { get; set; }
    public short ParentIndex { get; set; }
    public short FirstChildIndex { get; set; }
    public short NextSiblingIndex { get; set; }
    public short PreviousSiblingIndex { get; set; }
    public Vector3 Translation { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 BoundingBoxMin { get; set; }
    public Vector3 BoundingBoxMax { get; set; }
    public NodeFlags Flags { get; set; }
}