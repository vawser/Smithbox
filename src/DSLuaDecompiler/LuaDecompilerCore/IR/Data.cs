using System.Collections.Generic;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// Data instruction. Don't know what it does but doesn't seem to do anything important for decompilation purposes.
    /// Basically have this so the label generation algorithm doesn't break
    /// </summary>
    public sealed class Data : Instruction
    {
        /// <summary>
        /// Lua locals are often defined at the last data instruction
        /// </summary>
        public List<LuaFile.Local>? Locals = null;
    }
}
