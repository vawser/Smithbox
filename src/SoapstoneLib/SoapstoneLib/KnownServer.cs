using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SoapstoneLib
{
    /// <summary>
    /// Connection info used by both locally running servers and by local clients, so they can find each other.
    /// 
    /// Currently, this only supports localhost connections.
    /// </summary>
    public sealed class KnownServer
    {
        /// <summary>
        /// Standard server info for DSMapStudio.
        /// </summary>
        public static readonly KnownServer DSMapStudio = new KnownServer(22720, "DSMapStudio");

        /// <summary>
        /// Standard server info for Smithbox.
        /// </summary>
        public static readonly KnownServer Smithbox = new KnownServer(22721, "Smithbox");

        /// <summary>
        /// Expected local process name of this server. This usually matches the exe name.
        /// </summary>
        public string ProcessName { get; }

        /// <summary>
        /// Standard port for this server to run at. A different one may be selected if it's busy.
        /// </summary>
        public ushort PortHint { get; }

        /// <summary>
        /// Construct an address for local server connections.
        /// 
        /// The server's process must match the process name, if provided here.
        /// Otherwise, the port is used directly, if non-zero. The port is also preferred
        /// if there are multiple ports associated with the given process name.
        /// </summary>
        public KnownServer(ushort portHint, string processName)
        {
            if (portHint == 0 && processName == null)
            {
                throw new ArgumentException($"One of process or port must be provided in KnownServer");
            }
            PortHint = portHint;
            ProcessName = processName;
        }

        /// <inheritdoc />
        public override string ToString() => $"KnownServer[PortHint={PortHint},ProcessName={ProcessName}]";

        /// <summary>
        /// Try to find a running server using heuristic info.
        /// 
        /// This does a lookup of system TCP state and will throw an exception if that fails.
        /// </summary>
        internal bool FindServer(out int realPort)
        {
            realPort = 0;

            // Make sure the server is at least running. This may throw an exception.
            MIB_TCPROW_OWNER_PID[] rows = GetAllTcpConnections<MIB_TCPROW_OWNER_PID>();
            MIB_TCP6ROW_OWNER_PID[] rows6 = GetAllTcpConnections<MIB_TCP6ROW_OWNER_PID>();

            // If process is given, always try to use it, and fail if not present
            if (ProcessName != null)
            {
                Process[] processes = Process.GetProcessesByName(ProcessName);
                if (processes.Length == 0)
                {
                    return false;
                }
                HashSet<int> processIds = new HashSet<int>(processes.Select(p => p.Id));
                List<int> matchingPorts = new List<int>();
                foreach (MIB_TCPROW_OWNER_PID row in rows)
                {
                    if (processIds.Contains(row.owningPid) && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                    {
                        matchingPorts.Add(ConvertPort(row.localPort));
                    }
                    // if (row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN) Console.WriteLine($"local {row.localPort} remote {row.remotePort} pid {row.owningPid}");
                }
                foreach (MIB_TCP6ROW_OWNER_PID row in rows6)
                {
                    if (processIds.Contains(row.owningPid) && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                    {
                        matchingPorts.Add(ConvertPort(row.localPort));
                    }
                    // if (row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN) Console.WriteLine($"local6 {row.localPort} remote {row.remotePort} pid {row.owningPid}");
                }
                if (matchingPorts.Count > 0)
                {
                    // Prefer PortHint if given
                    realPort = PortHint > 0 && matchingPorts.Contains(PortHint) ? PortHint : matchingPorts[0];
                    return true;
                }
                return false;
            }

            // Use port if process is not given. This will fail later on if there's not a gRPC service there.
            int netPort = ConvertPort(PortHint);
            foreach (MIB_TCPROW_OWNER_PID row in rows)
            {
                if (row.localPort == netPort && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                {
                    realPort = PortHint;
                    return true;
                }
            }
            foreach (MIB_TCP6ROW_OWNER_PID row in rows6)
            {
                if (row.localPort == netPort && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                {
                    realPort = PortHint;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the hinted port is in use by a server, as a heuristic for choosing a different port if it is.
        /// </summary>
        internal bool IsPortInUse()
        {
            if (PortHint == 0)
            {
                return false;
            }
            MIB_TCPROW_OWNER_PID[] rows = GetAllTcpConnections<MIB_TCPROW_OWNER_PID>();
            MIB_TCP6ROW_OWNER_PID[] rows6 = GetAllTcpConnections<MIB_TCP6ROW_OWNER_PID>();
            int netPort = ConvertPort(PortHint);
            return rows.Any(row => netPort == row.localPort && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN)
                || rows6.Any(row => netPort == row.localPort && row.state == MIB_TCP_STATE.MIB_TCP_STATE_LISTEN);
        }

        // Windows API
        // https://docs.microsoft.com/en-us/windows/win32/api/iphlpapi/nf-iphlpapi-getextendedtcptable
        // C# marshalling example code
        // http://www.pinvoke.net/default.aspx/iphlpapi/GetExtendedTcpTable.html
        // https://stackoverflow.com/questions/577433/which-pid-listens-on-a-given-port-in-c-sharp

        private static ushort ConvertPort(ushort port) => (ushort)(port >> 8 | ((port & 0xFF) << 8));

        private enum MIB_TCP_STATE : int
        {
            MIB_TCP_STATE_CLOSED = 1,
            MIB_TCP_STATE_LISTEN = 2,
            MIB_TCP_STATE_SYN_SENT = 3,
            MIB_TCP_STATE_SYN_RCVD = 4,
            MIB_TCP_STATE_ESTAB = 5,
            MIB_TCP_STATE_FIN_WAIT1 = 6,
            MIB_TCP_STATE_FIN_WAIT2 = 7,
            MIB_TCP_STATE_CLOSE_WAIT = 8,
            MIB_TCP_STATE_CLOSING = 9,
            MIB_TCP_STATE_LAST_ACK = 10,
            MIB_TCP_STATE_TIME_WAIT = 11,
            MIB_TCP_STATE_DELETE_TCB = 12,
        }

        private enum TCP_TABLE_CLASS : int
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public MIB_TCP_STATE state;
            public uint localAddr;
            // Uses network byte order, but only within two bytes. Use ConvertPort to convert.
            public ushort localPort;
            public uint remoteAddr;
            public ushort remotePort;
            public int owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] localAddr;
            public uint localScopeId;
            public ushort localPort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] remoteAddr;
            public uint remoteScopeId;
            public ushort remotePort;
            public MIB_TCP_STATE state;
            public int owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COMMON_MIB_TCPTABLE_OWNER_PID
        {
            public uint numEntries;
            // This is followed by a variable length array of either row type.
            // It's easiest to walk through it manually in code.
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(
            IntPtr tcpTable,
            ref int tcpTableLength,
            bool sort,
            int ipVersion,
            TCP_TABLE_CLASS tcpTableType,
            int reserved = 0);

        private static TRow[] GetAllTcpConnections<TRow>()
        {
            TRow[] rows;
            // 2 is IPv4, 23 is IPv6
            int ipVersion;
            if (typeof(TRow) == typeof(MIB_TCPROW_OWNER_PID))
            {
                // IPv4
                ipVersion = 2;
            }
            else if (typeof(TRow) == typeof(MIB_TCP6ROW_OWNER_PID))
            {
                // IPv6
                ipVersion = 23;
            }
            else
            {
                throw new Exception($"Internal error: unsupported GetExtendedTcpTable type {typeof(TRow).FullName}");
            }
            int buffSize = 0;

            uint ret = GetExtendedTcpTable(
                IntPtr.Zero,
                ref buffSize,
                true,
                ipVersion,
                TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
            // 122 is "insufficient buffer", expected
            if (ret != 0 && ret != 122)
            {
                throw new Exception("GetExtendedTcpTable in iphlpapi.dll failed with error code " + ret);
            }

            // Race conditions may be possible, if this changes between allocation and re-allocation
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);
            try
            {
                ret = GetExtendedTcpTable(
                    buffTable,
                    ref buffSize,
                    true,
                    ipVersion,
                    TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
                if (ret != 0)
                {
                    throw new Exception("GetExtendedTcpTable in iphlpapi.dll failed with error code " + ret);
                }

                // Get the total size and copy all row structs individually
                // This is pointing to memory we've allocated, so unboxing it should be safe.
#pragma warning disable CS8605
                COMMON_MIB_TCPTABLE_OWNER_PID tab =
                    (COMMON_MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(COMMON_MIB_TCPTABLE_OWNER_PID));
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf<int>());

                rows = new TRow[tab.numEntries];
                for (int i = 0; i < tab.numEntries; i++)
                {
                    rows[i] = (TRow)Marshal.PtrToStructure(rowPtr, typeof(TRow));
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf<TRow>());
                }
#pragma warning restore CS8605
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }
            return rows;
        }
    }
}
