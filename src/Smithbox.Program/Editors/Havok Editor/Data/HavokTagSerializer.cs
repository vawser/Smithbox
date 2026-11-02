using Havok.Shared;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The HKLib tagfile serializer for the Havok version of the project, so the bank can read and write files
/// without depending on the version: 2018 for ER/NR/DB, 2019 for AC6.
/// </summary>
public abstract class HavokTagSerializer
{
    public static HavokTagSerializer Create(ProjectEntry project)
    {
        return HavokTypeUtils.IsHKX3_2019(project)
            ? new HavokTagSerializer2019()
            : new HavokTagSerializer2018();
    }

    /// <summary>
    /// Whether files refer to the compendium in their binder when it has one, so it is loaded to read and write them.
    /// AC6 does this for map collision and animations.
    /// </summary>
    public abstract bool UsesBinderCompendium { get; }

    public abstract void LoadCompendium(Stream stream);

    public abstract IRootContainer Read(Stream stream);

    public abstract void Write(IRootContainer root, Stream stream);
}

public class HavokTagSerializer2018 : HavokTagSerializer
{
    private readonly HKLib.Serialization.hk2018.Binary.HavokBinarySerializer _serializer = new();

    public override bool UsesBinderCompendium => false;

    public override void LoadCompendium(Stream stream) => _serializer.LoadCompendium(stream);

    public override IRootContainer Read(Stream stream) =>
        (HKLib.hk2018.hkRootLevelContainer)_serializer.Read(stream);

    public override void Write(IRootContainer root, Stream stream) =>
        _serializer.Write((HKLib.hk2018.hkRootLevelContainer)root, stream);
}

public class HavokTagSerializer2019 : HavokTagSerializer
{
    private readonly HKLib.Serialization.hk2019.Binary.HavokBinarySerializer _serializer = new();

    public override bool UsesBinderCompendium => true;

    public override void LoadCompendium(Stream stream) => _serializer.LoadCompendium(stream);

    public override IRootContainer Read(Stream stream) =>
        (HKLib.hk2019.hkRootLevelContainer)_serializer.Read(stream);

    public override void Write(IRootContainer root, Stream stream) =>
        _serializer.Write((HKLib.hk2019.hkRootLevelContainer)root, stream);
}
