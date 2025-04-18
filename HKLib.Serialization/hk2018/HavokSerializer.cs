using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018;

public abstract class HavokSerializer
{
    public HavokSerializer(HavokTypeRegistry typeRegistry)
    {
        TypeRegistry = typeRegistry;
    }

    public HavokTypeRegistry TypeRegistry { get; set; }

    private static Stream GetReadStream(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException("The specified file does not exist.", nameof(path));
        }

        byte[] bytes = File.ReadAllBytes(path);
        return new MemoryStream(bytes);
    }

    private static Stream GetWriteStream(string path)
    {
        if (Directory.GetParent(path) is { } parent)
        {
            Directory.CreateDirectory(parent.FullName);
        }

        return File.Create(path);
    }

    /// <summary>
    /// Loads a Havok 2018 type compendium from a path into the serialier to enable deserialization of tagfiles which
    /// reference it.
    /// </summary>
    public virtual void LoadCompendium(string path) => LoadCompendium(GetReadStream(path));

    /// <summary>
    /// Loads a Havok 2018 type compendium from a stream into the serialier to enable deserialization of tagfiles which
    /// reference it.
    /// </summary>
    public abstract void LoadCompendium(Stream stream);

    /// <summary>
    /// Loads a type compendium into the serialier to enable deserialization of tagfiles which reference it.
    /// </summary>
    public abstract void LoadCompendium(HavokCompendium compendium);

    /// <summary>
    /// Reads a type compendium from a path.
    /// </summary>
    public virtual HavokCompendium ReadCompendium(string path) => ReadCompendium(GetReadStream(path));

    /// <summary>
    /// Reads a type compendium from a stream.
    /// </summary>
    public abstract HavokCompendium ReadCompendium(Stream stream);

    /// <summary>
    /// Writes a type compendium to a file at the given path.
    /// </summary>
    public virtual void Write(HavokCompendium compendium, string path) => Write(compendium, GetWriteStream(path));

    /// <summary>
    /// Writes a type compendium into a stream.
    /// </summary>
    public abstract void Write(HavokCompendium compendium, Stream stream);

    /// <summary>
    /// Reads a Havok 2018 file from a path and returns the root level object
    /// </summary>
    public virtual IHavokObject Read(string path) => Read(GetReadStream(path));

    /// <summary>
    /// Reads a Havok 2018 file from a stream and returns the root level object
    /// </summary>
    public virtual IHavokObject Read(Stream stream)
    {
        IEnumerable<IHavokObject> havokObjects = ReadAllObjects(stream);
        if (havokObjects.FirstOrDefault() is not { } rootLevelObject)
        {
            throw new InvalidDataException("No root level Havok object found.");
        }

        return rootLevelObject;
    }

    /// <summary>
    /// Reads a Havok 2018 file from a path and returns an <see cref="IEnumerable{T}" /> starting with the root level object of
    /// all objects contained within it.
    /// Useful for some applications to avoid having to traverse the entire object hierarchy
    /// </summary>
    public virtual IEnumerable<IHavokObject> ReadAllObjects(string path) => ReadAllObjects(GetReadStream(path));

    /// <summary>
    /// Reads a Havok 2018 file from a stream and returns an <see cref="IEnumerable{T}" /> starting with the root level object
    /// of all objects contained within it.
    /// Useful for some applications to avoid having to traverse the entire object hierarchy
    /// </summary>
    public abstract IEnumerable<IHavokObject> ReadAllObjects(Stream stream);

    /// <summary>
    /// Writes an <see cref="IHavokObject" /> to a Havok 2018 file at the given path. The serialization of any object is
    /// supported but most applications will likely expect an <see cref="hkRootLevelContainer" /> as the root level object
    /// </summary>
    public virtual void Write(IHavokObject havokObject, string path) => Write(havokObject, GetWriteStream(path));

    /// <summary>
    /// Writes an <see cref="IHavokObject" /> to a stream. The serialization of any object is supported but most applications
    /// will likely expect an <see cref="hkRootLevelContainer" /> as the root level object
    /// </summary>
    public abstract void Write(IHavokObject havokObject, Stream stream);
}