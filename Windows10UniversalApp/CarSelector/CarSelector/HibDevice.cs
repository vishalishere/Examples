using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CarSelector
{
    class HibDevice
    {
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int Initialize(string bsPathname);
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int IdentifyBoard(byte pbyBoardType, byte pbyDownloadRunning, string bsSoftwareName, ulong plSoftwareVersion );
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int IdentifyHibDriver(byte pbyDriverType, string bsSoftwareName, ulong pdwSoftwareVersion, ushort pwBuildNumber);
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int ExecuteCommand(ulong dwInBufferSize, byte[] byInBuffer, ulong dwOutBufferSize, byte[] byOutBuffer, ulong pdwBytesReturned, ulong dwAsyncClientId);
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int InitializeRemoteKeys();
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern int FindChannelKeyset(ulong dwClientId);
        [DllImport(@"..\SharedOutput\Tlb\HibDevice.tlb")]
        public static extern void UpdateHibConnectStatus(uint bsPathname);
    }
}
