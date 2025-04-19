// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO.VFS
{
    public class BinderPath
    {
        public static string Combine(string left, string right)
        {
            if (Path.IsPathRooted(right))
                throw new ArgumentException($"Tried to combine with rooted right path: {right}");
            return Path.Combine(left, right).Replace('\\', '/');
        }
    }
}