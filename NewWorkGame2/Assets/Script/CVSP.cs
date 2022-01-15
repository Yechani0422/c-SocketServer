namespace CVSP
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.IO;
    using System.Runtime.InteropServices;

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CVSP
    {
        public byte cmd;
        public byte option;
        public short packetLength;
    }

    public sealed class SpecificationCVSP
    {
        public static byte CVSP_VER = (byte)0x01;

        public static byte CVSP_JOINREQ = (byte)0x01;
        public static byte CVSP_JOINRES = (byte)0x02;
        public static byte CVSP_CHATTINGREQ = (byte)0x03;
        public static byte CVSP_CHATTINGRES = (byte)0x04;
        public static byte CVSP_OPERATIONREQ = (byte)0x05;
        public static byte CVSP_MONITORINGMSG = (byte)0x06;
        public static byte CVSP_LEAVEREQ = (byte)0x07;
        

        public static byte CVSP_ATTACK = (byte)0x08;
        public static byte CVSP_DIE = (byte)0x09;
        public static byte CVSP_SCORE = (byte)0x10;

        public static byte CVSP_SUCCESS = (byte)0x01;
        public static byte CVSP_FAILE = (byte)0x02;
        public static byte CVSP_NEWUSER = (byte)0x03;

        public static int CVSP_SIZE = 4;
        public static int CVSP_BUFFERSIZE = 4096;

        public static int ServerPort = 5004;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PlayerInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string id;
        [MarshalAs(UnmanagedType.R4)]
        public float posX;
        [MarshalAs(UnmanagedType.R4)]
        public float posY;
        [MarshalAs(UnmanagedType.R4)]
        public float posZ;
        [MarshalAs(UnmanagedType.R4)]
        public float quatX;
        [MarshalAs(UnmanagedType.R4)]
        public float quatY;
        [MarshalAs(UnmanagedType.R4)]
        public float quatZ;
        [MarshalAs(UnmanagedType.R4)]
        public float quatW;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct AttackInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string id;
        [MarshalAs(UnmanagedType.R4)]
        public float posX;
        [MarshalAs(UnmanagedType.R4)]
        public float posY;
        [MarshalAs(UnmanagedType.R4)]
        public float posZ;
        [MarshalAs(UnmanagedType.R4)]
        public float quatX;
        [MarshalAs(UnmanagedType.R4)]
        public float quatY;
        [MarshalAs(UnmanagedType.R4)]
        public float quatZ;
        [MarshalAs(UnmanagedType.R4)]
        public float quatW;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HpInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string bulletOwner;
        [MarshalAs(UnmanagedType.R4)]
        public int hp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScoreInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string id;        
        [MarshalAs(UnmanagedType.R4)]
        public int kills;
        [MarshalAs(UnmanagedType.R4)]
        public int deaths;
    }
}