using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editor;

public static class EditorCommandQueue
{
    private static readonly ConcurrentQueue<string[]> QueuedCommands = new();

    public static void AddCommand(string cmd)
    {
        QueuedCommands.Enqueue(cmd.Split("/"));
    }

    public static void AddCommand(IEnumerable<string> cmd)
    {
        QueuedCommands.Enqueue(cmd.ToArray());
    }

    public static string[] GetNextCommand()
    {
        QueuedCommands.TryDequeue(out var cmd);
        return cmd;
    }
}
