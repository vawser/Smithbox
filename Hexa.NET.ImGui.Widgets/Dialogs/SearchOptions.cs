namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.Utilities.IO;
    using System;
    using System.IO;

    public struct SearchOptions
    {
        public bool Enabled;
        public string Pattern = string.Empty;
        public SearchOptionsFlags Flags;
        public SearchFilterDate DateModified;
        public SearchFilterSize FileSize;

        public SearchOptions()
        {
        }

        public readonly bool Filter(in FileMetadata metadata)
        {
            if ((metadata.Attributes & FileAttributes.Directory) != 0)
            {
                return true;
            }

            if ((Flags & SearchOptionsFlags.FilterDate) != 0)
            {
                DateTime now = DateTime.Now;
                DateTime startDate = DateTime.MinValue;

                switch (DateModified)
                {
                    case SearchFilterDate.Today:
                        startDate = now.Date; // Start of today
                        break;

                    case SearchFilterDate.Yesterday:
                        startDate = now.Date.AddDays(-1);
                        break;

                    case SearchFilterDate.Week:
                        startDate = now.AddDays(-7);
                        break;

                    case SearchFilterDate.Month:
                        startDate = now.AddMonths(-1);
                        break;

                    case SearchFilterDate.LastMonth:
                        startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                        break;

                    case SearchFilterDate.Year:
                        startDate = now.AddYears(-1);
                        break;

                    case SearchFilterDate.LastYear:
                        startDate = new DateTime(now.Year - 1, 1, 1);
                        break;
                }

                // If the item doesn't match the date filter, return false
                if (metadata.LastWriteTime < startDate)
                {
                    return false;
                }
            }

            if ((Flags & SearchOptionsFlags.FilterSize) != 0)
            {
                long minSize = 0;
                long maxSize = long.MaxValue;

                switch (FileSize)
                {
                    case SearchFilterSize.Empty:
                        maxSize = 0;
                        break;

                    case SearchFilterSize.Tiny:
                        minSize = 1;
                        maxSize = 1024; // Up to 1 KB
                        break;

                    case SearchFilterSize.Small:
                        minSize = 1024;
                        maxSize = 1024 * 1024; // 1 KB to 1 MB
                        break;

                    case SearchFilterSize.Medium:
                        minSize = 1024 * 1024;
                        maxSize = 1024 * 1024 * 10; // 1 MB to 10 MB
                        break;

                    case SearchFilterSize.Large:
                        minSize = 1024 * 1024 * 10;
                        maxSize = 1024 * 1024 * 100; // 10 MB to 100 MB
                        break;

                    case SearchFilterSize.Huge:
                        minSize = 1024 * 1024 * 100;
                        maxSize = 1024 * 1024 * 1000; // 100 MB to 1 GB
                        break;

                    case SearchFilterSize.Gigantic:
                        minSize = 1024 * 1024 * 1000; // Greater than 1 GB
                        break;
                }

                // If the item doesn't match the size filter, return false
                if (metadata.Size < minSize || metadata.Size > maxSize)
                {
                    return false;
                }
            }

            return true;
        }
    }
}