using HarmonyLib;
using System;
using System.Runtime.InteropServices;

namespace ServerCharacters.Patches
{
    // Token: 0x02000003 RID: 3
    internal class ZSteamSocketPatches
    {
        // Token: 0x02000004 RID: 4
        [HarmonyPatch(typeof(ZSteamSocket), "RegisterGlobalCallbacks")]
        private class rgcfix
        {
            private static void Postfix()
            {
                try
                {
                    GCHandle gchandle = GCHandle.Alloc(30000f, GCHandleType.Pinned);
                    GCHandle gchandle2 = GCHandle.Alloc(1, GCHandleType.Pinned);
                    GCHandle gchandle3 = GCHandle.Alloc(2097152, GCHandleType.Pinned);
                    GCHandle gchandle4 = GCHandle.Alloc(1048576, GCHandleType.Pinned);
                    bool flag = ZSteamSocket.m_statusChanged != null;
                    if (flag)
                    {
                        bool isServer = ZNet.m_isServer;
                        if (isServer)
                        {
                            ZLog.Log("SOCKETPARAMS SERVER");
                            SteamGameServerNetworkingUtils.SetConfigValue(25, 1, IntPtr.Zero, 3, gchandle.AddrOfPinnedObject());
                            SteamGameServerNetworkingUtils.SetConfigValue(23, 1, IntPtr.Zero, 1, gchandle2.AddrOfPinnedObject());
                            SteamGameServerNetworkingUtils.SetConfigValue(10, 1, IntPtr.Zero, 1, gchandle3.AddrOfPinnedObject());
                            SteamGameServerNetworkingUtils.SetConfigValue(11, 1, IntPtr.Zero, 1, gchandle3.AddrOfPinnedObject());
                            SteamGameServerNetworkingUtils.SetConfigValue(9, 1, IntPtr.Zero, 1, gchandle4.AddrOfPinnedObject());
                        }
                        else
                        {
                            ZLog.Log("SOCKETPARAMS CLIENT");
                            SteamNetworkingUtils.SetConfigValue(25, 1, IntPtr.Zero, 3, gchandle.AddrOfPinnedObject());
                            SteamNetworkingUtils.SetConfigValue(23, 1, IntPtr.Zero, 1, gchandle2.AddrOfPinnedObject());
                            SteamNetworkingUtils.SetConfigValue(10, 1, IntPtr.Zero, 1, gchandle3.AddrOfPinnedObject());
                            SteamNetworkingUtils.SetConfigValue(11, 1, IntPtr.Zero, 1, gchandle3.AddrOfPinnedObject());
                            SteamNetworkingUtils.SetConfigValue(9, 1, IntPtr.Zero, 1, gchandle4.AddrOfPinnedObject());
                        }
                        gchandle.Free();
                        gchandle2.Free();
                        gchandle3.Free();
                        gchandle4.Free();
                        ZLog.Log("SOCKETPARAMS PATCHED");
                    }
                    else
                    {
                        ZLog.LogError("SOCKETPARAMS SKIPPED");
                    }
                }
                catch (Exception ex)
                {
                    ZLog.LogError("SOCKETPARAMS CATCH " + ex.Message);
                }
            }
        }
    }
}