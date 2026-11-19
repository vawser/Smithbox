using System;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// An Identifier tracked by the symbol table. Should be unique per scope/closure. An identifier is represented as
    /// a single 32-bit value which is tagged with the identifier type, which allows for memory efficiency and easy set
    /// creation.
    /// </summary>
    public record struct Identifier
    {
        public enum IdentifierType
        {
            Register = 0,
            RenamedRegister = 1,
            Global = 2,
            GlobalTable = 3,
            UpValue = 4,
            StackUpValue = 5,
            Varargs = 6,
            Null = 7,
        }
        
        private readonly uint _value;

        /// <summary>
        /// Gets or sets the identifier type, which occupies the top 3 bits in the internal value
        /// </summary>
        public IdentifierType Type
        {
            get => (IdentifierType)(_value >> 29);
            init => _value = (_value & (uint.MaxValue >> 3)) | ((uint)value << 29);
        }

        /// <summary>
        /// If this identifier is a register, the register number from the bytecode.
        /// Occupies the bottom 8 bits since Lua only supports up to ~200 registers per function.
        /// </summary>
        public uint RegNum
        {
            get => _value & 0xFF;
            init
            {
                if (value > 255)
                    throw new Exception("Register number out of bounds");
                _value = (_value & 0xFFFFFF00) | value;
            }
        }
        
        /// <summary>
        /// For renamed registers, the subscript ID that disambiguates this identifier from others with
        /// the same register number. Occupies the 16 bits following the register number.
        /// </summary>
        public uint RegSubscriptNum
        {
            get => (_value >> 8) & 0xFFFF;
            init
            {
                if (value > ushort.MaxValue)
                    throw new Exception("Register subscript number out of bounds");
                _value = (_value & 0xFF0000FF) | (value << 8);
            }
        }

        /// <summary>
        /// UpValues are also 8 bits (4 actually since 32 is the limit) so just alias the register number
        /// </summary>
        public uint UpValueNum
        {
            get => RegNum;
            init => RegNum = value;
        }
        
        /// <summary>
        /// Indices in the constant table use 24 bits
        /// </summary>
        public uint ConstantId
        {
            get => _value & 0xFFFFFF;
            init
            {
                if (value > 0xFFFFFF)
                    throw new Exception("Constant id out of bounds");
                _value = (_value & 0xFF000000) | value;
            }
        }
        
        public bool IsRegister => Type is IdentifierType.Register or IdentifierType.RenamedRegister;
        public bool IsRenamedRegister => Type is IdentifierType.RenamedRegister;
        public bool IsGlobal => Type is IdentifierType.Global;
        public bool IsGlobalTable => Type is IdentifierType.GlobalTable;
        public bool IsUpValue => Type is IdentifierType.UpValue;
        public bool IsStackUpValue => Type is IdentifierType.StackUpValue;
        public bool IsVarArgs => Type is IdentifierType.Varargs;
        public bool IsNull => Type is IdentifierType.Null;

        public static Identifier GetRegister(uint regNum) => new()
        {
            Type = IdentifierType.Register,
            RegNum = regNum,
            RegSubscriptNum = 0
        };
        
        public static Identifier GetRenamedRegister(uint regNum, uint subscript) => new()
        {
            Type = IdentifierType.RenamedRegister,
            RegNum = regNum,
            RegSubscriptNum = subscript
        };
        
        public static Identifier GetUpValue(uint upValueNum) => new()
        {
            Type = IdentifierType.UpValue,
            UpValueNum = upValueNum
        };
        
        public static Identifier GetStackUpValue(uint upValueNum) => new()
        {
            Type = IdentifierType.StackUpValue,
            UpValueNum = upValueNum
        };
        
        public static Identifier GetGlobal(uint constantId) => new()
        {
            Type = IdentifierType.Global,
            ConstantId = constantId
        };

        public static Identifier GetGlobalTable() => new()
        {
            Type = IdentifierType.GlobalTable,
        };

        public static Identifier GetVarArgs() => new()
        {
            Type = IdentifierType.Varargs
        };
        
        public static Identifier GetNull() => new()
        {
            Type = IdentifierType.Null
        };
        
        public override string ToString()
        {
            return FunctionPrinter.DebugPrintIdentifier(this);
        }
    }
}
