using System.Collections.Generic;

namespace StudioCore.Application;

// The Model Editor's Data Transfer format: dummies and materials taken out of one model, to be added to another
// (directly, or through a JSON file). Bones are named rather than indexed, as indices differ between models.
// Vectors are float arrays and colours ARGB arrays, so the JSON stays readable. See ModelDataTransfer.

public class ModelTransferFile
{
    public string Format = "SmithboxModelData";
    public int Version = 1;

    /// <summary>The game and model the data came from, for the import summary</summary>
    public string Game;
    public string SourceModel;

    public List<ModelTransferDummy> Dummies = new();
    public List<ModelTransferMaterial> Materials = new();
}

public class ModelTransferDummy
{
    public short ReferenceID;

    /// <summary>In the parent bone's space, as the FLVER stores it</summary>
    public float[] Position;
    public float[] Forward;
    public float[] Upward;

    /// <summary>Bone names; null for none</summary>
    public string ParentBone;
    public string AttachBone;

    /// <summary>A, R, G, B</summary>
    public int[] Color;

    public bool FollowAttachBone;
    public bool UseUpwardVector;
    public int Unk30;
    public int Unk34;
}

public class ModelTransferMaterial
{
    public string Name;
    public string MTD;
    public List<ModelTransferTexture> Textures = new();

    /// <summary>The material's GX list, or null when it has none</summary>
    public ModelTransferGxList GX;
}

public class ModelTransferTexture
{
    public string ParamName;
    public string Path;
    public float[] TilingScale;
    public int TilingTypeU;
    public int TilingTypeV;
    public float Unk14;
    public float Unk18;
    public float Unk1C;
}

public class ModelTransferGxList
{
    public int TerminatorID;
    public int TerminatorLength;
    public List<ModelTransferGxItem> Items = new();
}

public class ModelTransferGxItem
{
    public string ID;
    public int Unk04;

    /// <summary>Base64</summary>
    public string Data;
}
