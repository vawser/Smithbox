namespace Hexa.NET.ImGui.Widgets.Extensions
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for enumerations to cast them to integral types.
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Casts an enumeration value to the specified integral type, with type size checking.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <typeparam name="TInt">The integral type to cast to.</typeparam>
        /// <param name="enumValue">The enumeration value to cast.</param>
        /// <returns>The casted value of the specified integral type.</returns>
        /// <exception cref="Exception">Thrown when the size of the enumeration does not match the size of the target integral type.</exception>
        public static TInt AsInteger<TEnum, TInt>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
            where TInt : unmanaged
        {
            if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<TInt>()) throw new Exception("type mismatch");
            TInt value = Unsafe.As<TEnum, TInt>(ref enumValue);
            return value;
        }

        /// <summary>
        /// Casts an enumeration value to a long integral type with size checking.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="enumValue">The enumeration value to cast.</param>
        /// <returns>The casted value of type long.</returns>
        /// <exception cref="Exception">Thrown when the size of the enumeration does not match supported integral types.</exception>
        public static long AsInteger<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            long value;
            if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<byte>()) value = Unsafe.As<TEnum, byte>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<short>()) value = Unsafe.As<TEnum, short>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<int>()) value = Unsafe.As<TEnum, int>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<long>()) value = Unsafe.As<TEnum, long>(ref enumValue);
            else throw new Exception("type mismatch");
            return value;
        }
    }
}