namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// Simple string based placeholder until IR is implemented
    /// </summary>
    public sealed class PlaceholderInstruction : Instruction
    {
        public readonly string Placeholder;
        
        public PlaceholderInstruction(string place)
        {
            Placeholder = place;
        }
    }
}
