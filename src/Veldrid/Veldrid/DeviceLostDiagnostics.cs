using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Works out what the GPU was doing when the device was lost. Nothing can be asked of the GPU after that,
    /// so command buffers leave named checkpoints behind them, which the driver reports the last of.
    /// </summary>
    internal unsafe sealed class DeviceLostDiagnostics
    {
        private const int MaxCheckpoints = 16384;

        private readonly GraphicsDevice _gd;
        private readonly VkQueue[] _queues;
        private readonly bool _hasCheckpoints;
        private readonly bool _hasDeviceFault;
        private readonly bool _hasMemoryBudget;

        // A checkpoint can only hold a pointer sized value, so it is the index of its name here
        private readonly string[] _checkpointNames = new string[MaxCheckpoints];
        private long _nextCheckpoint;

        private readonly object _reportLock = new object();
        private string _report;

        public DeviceLostDiagnostics(GraphicsDevice gd, IEnumerable<VkQueue> queues, bool hasCheckpoints,
            bool hasDeviceFault, bool hasMemoryBudget)
        {
            _gd = gd;
            _queues = queues.Distinct().ToArray();
            _hasCheckpoints = hasCheckpoints;
            _hasDeviceFault = hasDeviceFault;
            _hasMemoryBudget = hasMemoryBudget;
        }

        public void SetCheckpoint(VkCommandBuffer commandBuffer, string name)
        {
            if (!_hasCheckpoints)
                return;

            // Starts at 1, as some drivers skip a null marker
            long id = Interlocked.Increment(ref _nextCheckpoint);
            _checkpointNames[id % MaxCheckpoints] = name;
            vkCmdSetCheckpointNV(commandBuffer, (void*)id);
        }

        /// <summary>
        /// Every use of a lost device fails, so this only does the work for the first of them.
        /// </summary>
        public string GetReport()
        {
            lock (_reportLock)
            {
                if (_report == null)
                {
                    StringBuilder report = new StringBuilder();
                    report.AppendLine("The graphics device was lost (VK_ERROR_DEVICE_LOST).");

                    AppendSection(report, "GPU fault", AppendDeviceFault);
                    AppendSection(report, "Last GPU checkpoints", AppendCheckpoints);
                    AppendSection(report, "Memory", AppendMemoryBudget);
                    AppendSection(report, "Application", AppendApplicationState);

                    _report = report.ToString();
                }

                return _report;
            }
        }

        private static void AppendSection(StringBuilder report, string title, Action<StringBuilder> append)
        {
            report.AppendLine();
            report.AppendLine($"{title}:");

            try
            {
                append(report);
            }
            catch (Exception e)
            {
                report.AppendLine($"  Unavailable: {e.Message}");
            }
        }

        private void AppendCheckpoints(StringBuilder report)
        {
            if (!_hasCheckpoints)
            {
                report.AppendLine("  VK_NV_device_diagnostic_checkpoints is not supported.");
                return;
            }

            for (int i = 0; i < _queues.Length; i++)
            {
                uint count = 0;
                vkGetQueueCheckpointDataNV(_queues[i], &count, null);

                report.AppendLine($"  Queue {i}: {count} reported");

                if (count == 0)
                    continue;

                VkCheckpointDataNV* data = stackalloc VkCheckpointDataNV[(int)count];
                for (uint c = 0; c < count; c++)
                    data[c] = new VkCheckpointDataNV();

                vkGetQueueCheckpointDataNV(_queues[i], &count, data);

                long lowest = long.MaxValue;
                long highest = 0;

                for (uint c = 0; c < count; c++)
                {
                    long id = (long)data[c].pCheckpointMarker;
                    bool stillKnown = id > 0 && _nextCheckpoint - id < MaxCheckpoints;
                    string name = stillKnown ? _checkpointNames[id % MaxCheckpoints] : "(overwritten)";

                    report.AppendLine($"    #{id} at {data[c].stage}: {name}");

                    if (id > 0)
                    {
                        lowest = Math.Min(lowest, id);
                        highest = Math.Max(highest, id);
                    }
                }

                // Every checkpoint between the last one finished and the last one started, as one of those was
                // executing when the device was lost
                if (highest > lowest && highest - lowest <= 256 && _nextCheckpoint - lowest < MaxCheckpoints)
                {
                    report.AppendLine("    In flight:");
                    for (long id = lowest + 1; id < highest; id++)
                    {
                        report.AppendLine($"      #{id}: {_checkpointNames[id % MaxCheckpoints]}");
                    }
                }
            }
        }

        private void AppendDeviceFault(StringBuilder report)
        {
            if (!_hasDeviceFault)
            {
                report.AppendLine("  VK_EXT_device_fault is not supported.");
                return;
            }

            VkDeviceFaultCountsEXT counts = new VkDeviceFaultCountsEXT();
            VkResult result = vkGetDeviceFaultInfoEXT(_gd.Device, &counts, null);
            report.AppendLine($"  Query: {result}, {counts.addressInfoCount} addresses, {counts.vendorInfoCount} vendor records");

            VkDeviceFaultAddressInfoEXT* addresses = stackalloc VkDeviceFaultAddressInfoEXT[(int)counts.addressInfoCount];
            VkDeviceFaultVendorInfoEXT* vendor = stackalloc VkDeviceFaultVendorInfoEXT[(int)counts.vendorInfoCount];

            // The binary is only for the vendor's tools, and wasn't asked for
            counts.vendorBinarySize = 0;

            VkDeviceFaultInfoEXT info = new VkDeviceFaultInfoEXT
            {
                pAddressInfos = addresses,
                pVendorInfos = vendor
            };

            vkGetDeviceFaultInfoEXT(_gd.Device, &counts, &info);

            report.AppendLine($"  {Util.GetString((byte*)info.description)}");

            for (uint i = 0; i < counts.addressInfoCount; i++)
            {
                report.AppendLine($"  {addresses[i].addressType} at 0x{addresses[i].reportedAddress:X} (precision {addresses[i].addressPrecision})");
            }

            for (uint i = 0; i < counts.vendorInfoCount; i++)
            {
                report.AppendLine($"  {Util.GetString((byte*)vendor[i].description)}: code {vendor[i].vendorFaultCode}, data {vendor[i].vendorFaultData}");
            }
        }

        private void AppendMemoryBudget(StringBuilder report)
        {
            if (!_hasMemoryBudget)
            {
                report.AppendLine("  VK_EXT_memory_budget is not supported.");
                return;
            }

            VkPhysicalDeviceMemoryBudgetPropertiesEXT budget = new VkPhysicalDeviceMemoryBudgetPropertiesEXT();
            VkPhysicalDeviceMemoryProperties2 properties = new VkPhysicalDeviceMemoryProperties2 { pNext = &budget };

            vkGetPhysicalDeviceMemoryProperties2(_gd.PhysicalDevice, &properties);

            for (uint i = 0; i < properties.memoryProperties.memoryHeapCount; i++)
            {
                VkMemoryHeap heap = properties.memoryProperties.memoryHeaps[(int)i];
                const float MiB = 1024 * 1024;

                report.AppendLine($"  Heap {i} ({heap.flags}): {budget.heapUsage[(int)i] / MiB:F0} MiB used, " +
                    $"{budget.heapBudget[(int)i] / MiB:F0} MiB budget, {heap.size / MiB:F0} MiB total");
            }
        }

        private static void AppendApplicationState(StringBuilder report)
        {
            Func<string> context = GraphicsDevice.DeviceLostContext;

            if (context == null)
            {
                report.AppendLine("  Nothing registered.");
                return;
            }

            // It may want a lock held by a thread that is stuck waiting on the GPU
            Task<string> state = Task.Run(context);
            report.AppendLine(state.Wait(1000) ? state.Result : "  Timed out.");
        }
    }
}
