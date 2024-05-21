using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;

namespace Framework
{
    internal static class Memory
    {
        public static void init(string gameName, string moduleName)
        {
            Console.WriteLine("Loading dma");
            Offsets.vmm = new Vmm("-printf", "-device", "fpga", "-v");

            Console.WriteLine("Finding process");

            while (true)
            {
                if (Offsets.vmm.PidGetFromName(gameName, out Offsets.processPid))
                {
                    Console.WriteLine("Found Game.");
                    break;
                }
                else
                {
                    Console.WriteLine("Game could not be found, Trying again.");
                    Thread.Sleep(5000);
                }
            }

            Console.WriteLine("Fixing cr3");
            if (Vmm.FixCr3(Offsets.vmm, Offsets.processPid, moduleName))
            {
                Console.WriteLine("Cr3 fixed successfully");
                Vmm.MAP_MODULEENTRY Dll = Offsets.vmm.Map_GetModuleFromName(Offsets.processPid, moduleName);
                Offsets.GameAssembly = Dll.vaBase;
            }
            else
            {
                Console.WriteLine("Failed to fix cr3");
            }
        }
        #region Regular Rd/Wr
        public static T Read<T>(ulong address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    if (Offsets.vmm.MemRead(Offsets.processPid, address, (uint)size, (nint)pBuffer) == size)
                    {
                        return ByteArrayToStructure<T>(buffer);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to read {typeof(T).Name} from memory.");
                        return default(T);
                    }
                }
            }
        }
        public static bool Write<T>(ulong address, T data) where T : struct
        {
            byte[] buffer = StructureToByteArray(data);
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    return Offsets.vmm.MemWrite(Offsets.processPid, address, (uint)buffer.Length, (nint)pBuffer);
                }
            }
        }
        #endregion
        #region Scatter
        public static bool PrepareWrite<T>(VmmScatter scatter, ulong address, T data) where T : struct
        {
            byte[] buffer = StructureToByteArray(data);
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    return scatter.PrepareWrite(address, buffer);
                }
            }
        }
        //public static T ScatterRead<T>(VmmScatter scatter, ulong address)
        //{
        //    int size = Marshal.SizeOf(typeof(T));
        //    byte[] buffer = new byte[size];
        //
        //    unsafe
        //    {
        //
        //    }
        //}
        public static VmmScatter createScatter()
        {
            VmmScatter scatter = Offsets.vmm.Scatter_Initialize();
            return scatter;
        }
        #endregion
        #region Utility
        private static byte[] StructureToByteArray<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] array = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, array, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return array;
        }

        private static T ByteArrayToStructure<T>(byte[] byteArray) where T : struct
        {
            T structure;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(byteArray, 0, ptr, size);
                structure = Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return structure;
        }
        #endregion
    }
}