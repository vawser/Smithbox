namespace Havok.ClassGeneration;

/// <summary>
/// What is being generated: set once from the command line.
/// </summary>
internal static class Options
{
    /// <summary>
    /// The namespace of the generated classes, e.g. Havok.hk2016.
    /// </summary>
    public static string Namespace { get; set; } = "Havok.hk2016";

    /// <summary>
    /// Classes with hand-written members in the SDK project's Manual folder, generated as partial.
    /// </summary>
    public static HashSet<string> PartialClasses { get; } = new();
}
