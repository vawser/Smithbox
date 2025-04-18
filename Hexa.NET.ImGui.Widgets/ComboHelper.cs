namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using System;
    using System.Linq;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A helper class for working with ImGui combo boxes to select enum values of a specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static class ComboEnumHelper<T> where T : struct, Enum
    {
#if NET5_0_OR_GREATER
        private static readonly T[] values = Enum.GetValues<T>();
        private static readonly string[] names = Enum.GetNames<T>();
#else
        private static readonly T[] values = (T[])Enum.GetValues(typeof(T));
        private static readonly string[] names = Enum.GetNames(typeof(T));
#endif

        /// <summary>
        /// Displays a combo box to select an enum value.
        /// </summary>
        /// <param name="label">The label for the combo box.</param>
        /// <param name="value">The currently selected enum value (modified by user interaction).</param>
        /// <returns><c>true</c> if the user selects a new value, <c>false</c> otherwise.</returns>
        public static bool Combo(string label, ref T value)
        {
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays the text representation of an enum value.
        /// </summary>
        /// <param name="value">The enum value to display.</param>
        public static void Text(T value)
        {
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }

        public static string GetName(T value)
        {
            int index = Array.IndexOf(values, value);
            return names[index];
        }
    }

    /// <summary>
    /// A helper class for working with ImGui combo boxes to select enum values of various enum types.
    /// </summary>
    public static class ComboEnumHelper
    {
        private static readonly Dictionary<Type, object[]> values = new();
        private static readonly Dictionary<Type, string[]> names = new();
#if NET7_0_OR_GREATER

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "All members are included by [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)].")]
#endif
        private static void Get(
#if NET7_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type type, out object[] values, out string[] names)
        {
            if (ComboEnumHelper.values.TryGetValue(type, out var objects))
            {
                values = objects;
                names = ComboEnumHelper.names[type];
                return;
            }

            values = Enum.GetValues(type).Cast<object>().ToArray();
            names = Enum.GetNames(type);
            ComboEnumHelper.values.Add(type, values);
            ComboEnumHelper.names.Add(type, names);
        }

        /// <summary>
        /// Displays a combo box to select an enum value of a specified enum type.
        /// </summary>
        /// <param name="label">The label for the combo box.</param>
        /// <param name="type">The enum type to select values from.</param>
        /// <param name="value">The currently selected enum value (modified by user interaction).</param>
        /// <returns><c>true</c> if the user selects a new value, <c>false</c> otherwise.</returns>
        public static bool Combo(string label,
#if NET7_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type type, ref object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays the text representation of an enum value of a specified enum type.
        /// </summary>
        /// <param name="type">The enum type to select values from.</param>
        /// <param name="value">The enum value to display.</param>
        public static void Text(
#if NET7_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        Type type, object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }

    public static class ComboTextHelper
    {
        public static bool Combo(string label, string[] values, ref string value)
        {
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, values, values.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }
    }
}