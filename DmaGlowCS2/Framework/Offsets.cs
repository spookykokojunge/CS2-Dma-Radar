using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    internal class Offsets
    {
#pragma warning disable CS8618

        public static Vmm vmm;
        public static ulong GameAssembly;
        public static uint processPid;

        // Static padding for all offsets
        public static ulong Static = 0xB8; // Static padding

        // Admin offsets
        public static ulong adminBase = 0x3AC4730; // ConVar.Admin_TypeInfo
        public static ulong adminTime = 0x0; // public static float admintime;
        public static ulong adminWaterEffect = 0x11;// public static bool underwater_effect;

        // Convar graphics offsets
        public static ulong graphicsBase = 0x3A87040; // ConVar.Graphics_TypeInfo
        public static ulong graphicsFov = 0x18; // private static float _fov;
    }
}
