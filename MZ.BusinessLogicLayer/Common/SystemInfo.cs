using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using Newtonsoft.Json;

namespace BusinessLogicLayer.Common
{
    public class HardDiskInfo
    {
        /// <summary>
        /// 盘符
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 可用空间
        /// </summary>
        public string TotalSize { get; set; }

        /// <summary>
        /// 剩余空间
        /// </summary>
        public string FreeSize { get; set; }

        /// <summary>
        /// 获取所有驱动盘信息
        /// </summary>
        public static List<HardDiskInfo> GetAllHardDiskInfo()
        {
            List<HardDiskInfo> list = new List<HardDiskInfo>();
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.IsReady)
                {
                    list.Add(new HardDiskInfo { Name = d.Name, FreeSize = d.AvailableFreeSpace.ToString(), TotalSize = d.TotalSize.ToString() });
                }
            }
            return list;
        }

        /// <summary>
        /// 根据盘符获取磁盘信息
        /// </summary>
        public HardDiskInfo GetHardDiskInfoByName(string diskName)
        {
            DriveInfo drive = new DriveInfo(diskName);
            return new HardDiskInfo { FreeSize = drive.AvailableFreeSpace.ToString(), TotalSize = drive.TotalSize.ToString(), Name = drive.Name };
        }
    }

    public class CpuInfo
    {
        private static readonly PerformanceCounter Pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        /// <summary>
        /// CPU使用率
        /// </summary>
        public static double Cpu()
        {
            Pc.NextValue();
            Thread.Sleep(500);
            return Pc.NextValue();
        }
    }

    public class MemoryInfo
    {
        private static MemoryInfo instance;
        private static long physicalMemory;
        private static readonly object Locker = new object();
        private MemoryInfo()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                if (mo["TotalPhysicalMemory"] != null)
                {
                    physicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }

        /// <summary>
        /// 总物理内存
        /// </summary>
        public long TotalPhysicalMemory => physicalMemory;

        /// <summary>
        /// 可用内存
        /// </summary>
        public long FreePhysicalMemory
        {
            get
            {
                long availablebytes = 0;
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (var o in mos.GetInstances())
                {
                    var mo = (ManagementObject)o;
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }


        public static MemoryInfo GetInfo()
        {
            if (instance == null)
            {
                lock (Locker)
                {
                    if (instance == null)
                    {
                        instance = new MemoryInfo();
                    }
                }
            }
            return instance;
        }
    }

    public class SystemInfo
    {
        /// <summary>
        /// CPU使用率
        /// </summary>
        public double Cpu { get; set; }

        /// <summary>
        /// 物理内存
        /// </summary>
        public long TotalMemory { get; set; }

        /// <summary>
        /// 可用内存
        /// </summary>
        public long FreeMemory { get; set; }

        /// <summary>
        /// 磁盘信息
        /// </summary>
        public List<HardDiskInfo> Disk { get; set; }

        /// <summary>
        /// 获取CPU使用率,内存以及磁盘空间
        /// </summary>
        /// <returns>json</returns>
        public static string GetSystemPerformance()
        {
            SystemInfo info = new SystemInfo { Cpu = CpuInfo.Cpu() };
            var memory = MemoryInfo.GetInfo();
            info.TotalMemory = memory.TotalPhysicalMemory;
            info.FreeMemory = memory.FreePhysicalMemory;
            info.Disk = HardDiskInfo.GetAllHardDiskInfo();
            return JsonConvert.SerializeObject(info);
        }
    }
}
