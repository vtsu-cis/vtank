using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace VTankOptionsSpace
{
    public class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;

            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;    // Declared wrong in the full framework
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;

            public int dmPositionX; // Using a PointL Struct does not work
            public int dmPositionY;

        }

        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings
            (string deviceName, int modeNum, ref DEVMODE devMode);

        private const long DM_BITSPERPEL = 0x04;//&H40000;
        private const long DM_PELSWIDTH = 0x08;//& H80000;
        private const long DM_PELSHEIGHT = 0x01;//& H100000;

        public List<DEVMODE> AvailScrRes = null;
        public int piAvailableDisplayModes;

        public long MaxHRes
        {
            get
            {
                int iAns = 0;
                int iTest = 0;
                int iCtr = 0;
                for (iCtr = 0; iCtr < AvailScrRes.Count; iCtr++)
                {
                    iTest = AvailScrRes[iCtr].dmPelsWidth;
                    if (iTest > iAns)
                    {
                        iAns = iTest;
                    }
                }

                return iAns;
            }
        }

        public long MaxVRes
        {
            get
            {
                int iAns = 0;
                int iTest = 0;
                int iCtr = 0;
                for (iCtr = 0; iCtr < AvailScrRes.Count; iCtr++)
                {
                    iTest = AvailScrRes[iCtr].dmPelsHeight;
                    if (iTest > iAns)
                    {
                        iAns = iTest;
                    }
                }

                return iAns;
            }
        }

        public int AvailableDisplayModes
        {
            get
            {
                piAvailableDisplayModes = AvailScrRes.Count;
                return piAvailableDisplayModes;
            }
        }

        public void FindValidDisplayModes()
        {
            int dMode = -1;
            AvailScrRes = new List<DEVMODE>();

            DEVMODE DM = new DEVMODE();
            DM.dmSize = (short)Marshal.SizeOf(DM);
            DM.dmFields = (int)(DM_PELSWIDTH | DM_PELSHEIGHT | DM_BITSPERPEL);
            while (User32.EnumDisplaySettings(null, dMode, ref DM) > 0)
            {
                AvailScrRes.Add(DM);
                dMode++;
            }

            piAvailableDisplayModes = AvailScrRes.Count;
        }
    }
}
