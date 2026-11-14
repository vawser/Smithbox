namespace StudioCore.Memory;

/// <summary>
/// An open connection to a running game, for memory code that doesn't own the connection itself.
/// Calls that touch the game are made while holding <see cref="Lock"/>.
/// </summary>
public interface IGameProcess
{
    /// <summary>Held around every read or write, so other users of the connection don't interleave</summary>
    object Lock { get; }

    /// <summary>Whether the process handle is still open; false once the game has gone away</summary>
    bool IsOpen { get; }

    /// <summary>Every match of an AOB ("48 8B ?? ...") in the executable sections of the main module</summary>
    List<nint> Scan(string aob);

    byte[] ReadBytes(nint address, int length);

    void WriteBytes(nint address, byte[] bytes);
}
