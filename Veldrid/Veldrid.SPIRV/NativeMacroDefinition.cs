using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct NativeMacroDefinition
    {
        public uint NameLength;
        public fixed byte Name[128];
        public uint ValueLength;
        public fixed byte Value[128];

        public NativeMacroDefinition(MacroDefinition macroDefinition)
        {
            if (string.IsNullOrEmpty(macroDefinition.Name))
            {
                throw new SpirvCompilationException($"MacroDefinition Name must be non-null.");
            }
            if (macroDefinition.Name.Length > 128)
            {
                throw new SpirvCompilationException($"Macro names must be less than or equal to 128 characters.");
            }

            fixed (char* nameU16Ptr = macroDefinition.Name)
            fixed (byte* namePtr = Name)
            {
                NameLength = (uint)Encoding.ASCII.GetBytes(nameU16Ptr, macroDefinition.Name.Length, namePtr, 128);
            }

            if (!string.IsNullOrEmpty(macroDefinition.Value))
            {
                if (macroDefinition.Value.Length > 128)
                {
                    throw new SpirvCompilationException($"Macro values must be less than or equal to 128 characters.");
                }

                fixed (char* valueU16 = macroDefinition.Value)
                fixed (byte* valuePtr = Value)
                {
                    ValueLength = (uint)Encoding.ASCII.GetBytes(valueU16, macroDefinition.Value.Length, valuePtr, 128);
                }
            }
            else
            {
                ValueLength = 0;
            }
        }
    }
}